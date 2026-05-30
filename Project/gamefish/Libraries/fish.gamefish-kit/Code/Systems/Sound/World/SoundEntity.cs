namespace GameFish;

[Icon( "volume_up" )]
[EditorHandle( Icon = "🔊" )]
public partial class SoundEntity : Entity
{
	protected const int SOUND_ORDER = DEFAULT_ORDER - 1000;
	protected const int SOUND_LOGIC_ORDER = SOUND_ORDER + 100;

	/// <summary>
	/// If enabled: clears all sounds on this object.
	/// </summary>
	[Property]
	[Feature( SOUND )]
	[Order( SOUND_ORDER )]
	[Title( "Stop Previous" )]
	public bool StopPreviousEnabled { get; set; } = true;

	/// <summary>
	/// The sound to play.
	/// </summary>
	[Property]
	[Feature( SOUND )]
	[Order( SOUND_ORDER )]
	public virtual SoundEvent Sound { get; set; }

	/// <summary>
	/// Previews the sound.
	/// </summary>
	[Button( "Play" )]
	[Feature( SOUND )]
	[Order( SOUND_ORDER )]
	protected void InspectorPlaySound()
		=> RpcBroadcastPlay();

	/// <summary>
	/// Tries to play the sound locally.
	/// </summary>
	/// <returns> If the sound could be played. </returns>
	public virtual bool TryPlay()
	{
		if ( !GameObject.IsValid() || GameObject.IsDestroyed )
			return false;

		if ( !Sound.IsValid() )
			return false;

		if ( StopPreviousEnabled )
			GameObject.StopAllSounds( fadeOutTime: 0f );

		GameObject.PlaySound( Sound );

		return true;
	}

	/// <summary>
	/// Broadcasts the sound to play to all others.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.SendImmediate )]
	public void RpcBroadcastPlay()
		=> TryPlay();
}
