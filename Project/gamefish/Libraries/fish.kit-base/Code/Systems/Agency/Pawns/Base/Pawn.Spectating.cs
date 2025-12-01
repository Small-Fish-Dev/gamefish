namespace GameFish;

partial class Pawn
{
	protected const int SPECTATOR_ORDER = 500;

	/// <summary>
	/// If true: indicate that spectators can spectate this pawn. <br />
	/// If false: all non-forced spectating is blocked.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( SPECTATOR ), Order( SPECTATOR_ORDER )]
	public virtual bool AllowSpectators { get; set; } = false;

	/// <returns> If this can spectate a target. </returns>
	public virtual bool CanSpectate( Pawn target )
		=> false;

	/// <param name="spec"> A spectator. </param>
	/// <returns> If the spectator can target this pawn. </returns>
	public virtual bool AllowSpectator( Pawn spec )
	{
		if ( !this.IsValid() || !AllowSpectators )
			return false;

		if ( !spec.IsValid() || spec == this )
			return false;

		return true;
	}

	/// <param name="target"> The pawn we're trying to spectate. </param>
	/// <returns> If the spectate attempt was successful. </returns>
	public virtual bool TrySpectate( Pawn target )
	{
		// Only spectators can spectate.
		return false;
	}
}
