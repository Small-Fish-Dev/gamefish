namespace GameFish;

partial class PawnView
{
	protected const string SPECTATING = "👻 Spectating";

	/// <summary>
	/// Allow spectators to target this pawn.
	/// </summary>
	[Property]
	[Feature( SPECTATING )]
	public virtual bool AllowSpectators { get; set; } = false;

	/// <param name="spec"> A spectator. </param>
	/// <returns> If the spectator can target this pawn. </returns>
	public virtual bool CanSpectate( BasePawn spec )
	{
		if ( !AllowSpectators || !spec.IsValid() )
			return false;

		return true;
	}

	/// <param name="spec"> A spectator. </param>
	/// <returns> If the spectator successfully targeted this pawn. </returns>
	public virtual bool TrySpectate( BasePawn spec )
	{
		// Only views meant spectate can.
		return false;
	}
}
