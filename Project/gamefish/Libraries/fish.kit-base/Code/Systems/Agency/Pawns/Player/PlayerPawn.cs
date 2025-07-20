namespace GameFish;

/// <summary>
/// A player-only pawn that uses the <see cref="PlayerController"/>.
/// </summary>
public partial class PlayerPawn : ControllerPawn
{
	public const string GROUP_PLAYER = "Player";

	/// <summary>
	/// Only player <see cref="Agent"/>s can own a player pawn.
	/// </summary>
	public override bool AllowOwnership( Agent agent )
	{
		if ( !agent.IsValid() || !agent.IsPlayer )
			return false;

		if ( !base.AllowOwnership( agent ) )
			return false;

		return true;
	}
}
