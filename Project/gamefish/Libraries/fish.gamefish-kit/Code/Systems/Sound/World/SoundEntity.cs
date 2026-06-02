using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.JavaScript;
using Sandbox.Audio;

namespace GameFish;

[Icon( "volume_up" )]
[EditorHandle( Icon = "🔊" )]
public partial class SoundEntity : Entity
{
	protected const int SOUND_ORDER = DEFAULT_ORDER - 1000;

	protected const int SOUND_NETWORKING_ORDER = SOUND_ORDER + 20;
	protected const int SOUND_SOUNDS_ORDER = SOUND_ORDER + 50;

	/// <summary>
	/// If enabled: the sound follows this object.
	/// </summary>
	[Property]
	[Icon( "🐤" )]
	[Feature( SOUND )]
	[Order( SOUND_ORDER )]
	[Title( "Following" )]
	public bool FollowingEnabled { get; set; } = true;

	/// <summary>
	/// If enabled: clears all sounds on this object.
	/// </summary>
	[Property]
	[Icon( "🔇" )]
	[Feature( SOUND )]
	[Order( SOUND_ORDER )]
	[Title( "Stop Previous" )]
	public bool StopPreviousEnabled { get; set; } = true;

	/// <summary>
	/// The sound to play.
	/// </summary>
	[Property]
	[Title( "Sounds" )]
	[Order( SOUND_SOUNDS_ORDER )]
	[WideMode( HasLabel = false )]
	[InlineEditor( Label = false )]
	[Feature( SOUND ), Group( SOUNDS )]
	public SoundTable Table { get; set; } = new();

	/// <summary>
	/// Previews the sound.
	/// </summary>
	[Button( "Play" )]
	[Order( SOUND_SOUNDS_ORDER + 1 )]
	[Feature( SOUND ), Group( SOUNDS )]
	protected void InspectorPlaySound()
		=> RpcBroadcastPlay();

	/// <summary>
	/// Tries to play the sound locally.
	/// </summary>
	/// <returns> If the sound could be played. </returns>
	public virtual bool TryPlay()
	{
		if ( GameObject.IsDestroyed() )
			return false;

		if ( !Table.IsValid() )
			return false;

		if ( !TrySelect( out var entry ) )
			return false;

		if ( StopPreviousEnabled )
			GameObject.StopAllSounds( fadeOutTime: 0f );

		return TryPlayEntry( entry );
	}

	protected virtual bool TrySelect( out SoundEntry entry )
	{
		entry = Table?.Pick();
		return entry.IsValid();
	}

	protected virtual bool TryPlayEntry( SoundEntry entry )
	{
		if ( entry.IsSoundEvent )
			return TryPlaySoundEvent( entry );

		if ( entry.IsSoundFile )
			return TryPlaySoundFile( entry );

		return false;
	}

	protected virtual bool TryPlaySoundEvent( SoundEntry entry )
	{
		if ( entry is null )
			return false;

		var sndEvent = entry.SoundEvent;

		if ( !sndEvent.IsValid() )
			return false;

		var handle = Sound.Play( sndEvent );

		OnSoundHandleCreated( handle, entry );

		return handle.IsValid();
	}

	protected virtual bool TryPlaySoundFile( SoundEntry entry )
	{
		if ( entry is null )
			return false;

		var sndFile = entry.SoundFile;

		if ( !sndFile.IsValid() )
			return false;

		var handle = Sound.PlayFile( sndFile );

		OnSoundHandleCreated( handle, entry );

		return handle.IsValid();
	}

	/// <summary>
	/// Configures sound handles according to its entry's configuration.
	/// </summary>
	protected virtual void OnSoundHandleCreated( SoundHandle handle, SoundEntry entry )
	{
		if ( !handle.IsValid() || entry is null )
			return;

		// Volume
		if ( entry.HasVolume )
			handle.Volume = entry.Volume.GetValue();

		// Pitch
		if ( entry.HasPitch )
			handle.Pitch = entry.Pitch.GetValue();

		// Falloff
		if ( entry.HasFalloff )
		{
			// Fully 3D, no UI.
			handle.SpacialBlend = 1f;
			handle.ListenLocal = false;

			// Fade out over distance.
			handle.DistanceAttenuation = true;
			handle.Distance = entry.Distance;
			handle.Falloff = entry.FalloffCurve;
		}

		// Mixer
		if ( entry.HasMixer )
		{
			var mixer = Mixer.FindMixerByName( entry.Mixer.Name );
			handle.TargetMixer = mixer ?? Mixer.Default;
		}

		// Follow
		handle.Transform = WorldTransform;

		if ( FollowingEnabled && GameObject.IsValid() )
		{
			handle.Parent = GameObject;
			handle.FollowParent = true;

			handle.LocalTransform = global::Transform.Zero;
		}
	}

	/// <summary>
	/// Broadcasts the sound to play to all others.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.SendImmediate )]
	public void RpcBroadcastPlay()
		=> TryPlay();
}
