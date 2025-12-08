using Sandbox.Audio;

namespace GameFish;

partial class Entity
{
	/// <summary>
	/// Attaches an existing sound handle to this object.
	/// </summary>
	public SoundHandle AttachSound( SoundHandle soundHandle, Transform? tWorld = null )
	{
		if ( !soundHandle.IsValid() || !GameObject.IsValid() )
			return null;

		soundHandle.Parent = GameObject;
		soundHandle.FollowParent = true;
		soundHandle.LocalTransform = tWorld.HasValue
			? WorldTransform.ToLocal( tWorld.Value )
			: global::Transform.Zero;

		return soundHandle;
	}


	#region SoundEvent


	/// <summary>
	/// Allows the object's owner to broadcast a sound event on this object.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void BroadcastSound( SoundEvent soundEvent, Vector3? worldPos = null )
	{
		_ = worldPos is Vector3 sndPos
			? EmitSound( soundEvent, new Transform( sndPos ) )
			: EmitSound( soundEvent );
	}

	/// <summary>
	/// Allows the object's owner to broadcast a sound event on this object.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void BroadcastSound( SoundEvent soundEvent, SoundSettings settings )
		=> EmitSound( soundEvent, settings );

	/// <summary>
	/// Allows the host to broadcast a sound event on this object.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.HostOnly )]
	public void HostBroadcastSound( SoundEvent soundEvent, Vector3? worldPos = null )
	{
		_ = worldPos is Vector3 sndPos
			? EmitSound( soundEvent, new Transform( sndPos ) )
			: EmitSound( soundEvent );
	}

	/// <summary>
	/// Allows the host to broadcast a sound event on this object.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.HostOnly )]
	public void HostBroadcastSound( SoundEvent soundEvent, SoundSettings settings )
		=> EmitSound( soundEvent, settings );

	/// <summary>
	/// Plays a sound event locally that follows this object.
	/// </summary>
	public SoundHandle EmitSound( SoundEvent soundEvent )
	{
		if ( !soundEvent.IsValid() )
			return null;

		return AttachSound( Sound.Play( soundEvent ) );
	}

	/// <summary>
	/// Plays a sound event locally at a position that follows this object.
	/// </summary>
	public SoundHandle EmitSound( SoundEvent soundEvent, in Vector3 worldPos )
		=> EmitSound( soundEvent, new Transform( worldPos ) );

	/// <summary>
	/// Plays a sound event locally that follows this object.
	/// </summary>
	public SoundHandle EmitSound( SoundEvent soundEvent, Transform? tWorld = null )
	{
		if ( !soundEvent.IsValid() )
			return null;

		var soundHandle = Sound.Play( soundEvent, tWorld?.Position ?? WorldPosition );

		if ( !soundHandle.IsValid() )
			return null;

		return AttachSound( soundHandle, tWorld );
	}

	/// <summary>
	/// Plays a sound event locally at a transform.
	/// </summary>
	public SoundHandle EmitSound( SoundEvent soundEvent, in SoundSettings s )
	{
		if ( !soundEvent.IsValid() )
			return null;

		var soundHandle = Sound.Play( soundEvent, s.Transform?.Position ?? WorldPosition );

		if ( !soundHandle.IsValid() )
			return null;

		if ( s.Volume is float sndVol )
			soundHandle.Volume = sndVol;

		if ( s.Pitch is float sndPitch )
			soundHandle.Pitch = sndPitch;

		if ( s.Mixer is string sndMixer )
			soundHandle.TargetMixer = Mixer.FindMixerByName( sndMixer )
				?? soundHandle.TargetMixer
				?? Mixer.Default;

		if ( !s.Following.IsValid() )
			return AttachSound( soundHandle, s.Transform );

		Transform? tLocal = s.Transform is Transform tSound
				? s.Following.WorldTransform.ToWorld( tSound )
				: null;

		return AttachSound( soundHandle, tLocal );
	}


	#endregion SoundEvent


	#region SoundFile


	/// <summary>
	/// Allows the object's owner to broadcast a sound file on this object.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void BroadcastSound( SoundFile soundFile, Vector3? worldPos = null )
	{
		_ = worldPos is Vector3 sndPos
			? EmitSound( soundFile, new Transform( sndPos ) )
			: EmitSound( soundFile );
	}

	/// <summary>
	/// Allows the object's owner to broadcast a sound file on this object.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void BroadcastSound( SoundFile soundFile, SoundSettings settings )
		=> EmitSound( soundFile, settings );

	/// <summary>
	/// Allows the host to broadcast a sound file on this object.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.HostOnly )]
	public void HostBroadcastSound( SoundFile soundFile, Vector3? worldPos = null )
	{
		_ = worldPos is Vector3 sndPos
			? EmitSound( soundFile, new Transform( sndPos ) )
			: EmitSound( soundFile );
	}

	/// <summary>
	/// Allows the host to broadcast a sound file on this object.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.HostOnly )]
	public void HostBroadcastSound( SoundFile soundFile, SoundSettings settings )
		=> EmitSound( soundFile, settings );


	/// <summary>
	/// Plays a sound file locally on this object.
	/// </summary>
	public SoundHandle EmitSound( SoundFile soundFile )
		=> EmitSound( soundFile, new Transform( WorldPosition ) );

	/// <summary>
	/// Plays a sound file locally at a position.
	/// </summary>
	public SoundHandle EmitSound( SoundFile soundFile, in Vector3 pos )
		=> EmitSound( soundFile, new Transform( pos ) );

	/// <summary>
	/// Plays a sound file locally. <br />
	/// Uses the object's position if transform is not specified.
	/// </summary>
	public SoundHandle EmitSound( SoundFile soundFile, in Transform? tWorld = null )
	{
		if ( !soundFile.IsValid() )
			return null;

		return AttachSound( Sound.PlayFile( soundFile ), tWorld );
	}

	/// <summary>
	/// Plays a sound file locally at a transform.
	/// </summary>
	public SoundHandle EmitSound( SoundFile soundFile, in SoundSettings s )
	{
		if ( !soundFile.IsValid() )
			return null;

		var soundHandle = Sound.PlayFile( soundFile,
			volume: s.Volume ?? 1f,
			pitch: s.Pitch ?? 1f
		);

		if ( !soundHandle.IsValid() )
			return null;

		if ( s.Mixer is string sndMixer )
			soundHandle.TargetMixer = Mixer.FindMixerByName( sndMixer )
				?? soundHandle.TargetMixer
				?? Mixer.Default;

		if ( !s.Following.IsValid() )
			return AttachSound( soundHandle, s.Transform );

		Transform? tLocal = s.Transform is Transform tSound
				? s.Following.WorldTransform.ToWorld( tSound )
				: null;

		return AttachSound( soundHandle, tLocal );
	}


	#endregion SoundFile
}
