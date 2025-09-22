namespace GameFish;

/// <summary>
/// A player-only pawn that uses the <see cref="PlayerController"/>.
/// </summary>
public partial class PlayerPawn : BasePawn
{
	/// <summary>
	/// Only player <see cref="Agent"/>s can own a player pawn.
	/// </summary>
	public override bool AllowOwnership( Agent agent )
	{
		if ( !agent.IsValid() || !agent.IsPlayer )
			return false;

		return base.AllowOwnership( agent );
	}
}
