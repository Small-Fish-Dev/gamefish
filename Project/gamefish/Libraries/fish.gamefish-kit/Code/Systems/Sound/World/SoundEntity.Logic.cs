namespace GameFish;

partial class SoundEntity : IActivate
{
	/// <summary>
	/// If enabled: tells all other connections to play the sound when activated by logic.
	/// </summary>
	[Property]
	[Title( "Broadcasted" )]
	[Order( SOUND_LOGIC_ORDER )]
	[Feature( LOGIC ), Group( NETWORKING )]
	public virtual bool BroadcastingEnabled { get; set; } = true;

	public virtual bool CanActivate( object source )
	{
		if ( !GameObject.IsValid() || GameObject.IsDestroyed )
			return false;

		return Sound.IsValid();
	}

	public virtual bool TryActivate( object source = null, object value = null )
	{
		if ( !CanActivate( source ) )
			return false;

		if ( !BroadcastingEnabled )
			return TryPlay();

		RpcBroadcastPlay();
		return true;
	}
}
