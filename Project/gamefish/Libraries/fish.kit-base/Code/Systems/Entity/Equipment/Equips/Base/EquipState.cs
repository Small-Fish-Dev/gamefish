namespace GameFish;

/// <summary>
/// Indicates if an equipment is dropped, deployed, or holstered.
/// </summary>
public enum EquipState
{
	/// <summary>
	/// Probably on the ground somewhere.
	/// </summary>
	Dropped = 1,

	/// <summary>
	/// Actively held in an <see cref="Pawn"/>'s hands.
	/// </summary>
	Deployed = 2,

	/// <summary>
	/// Kept in <see cref="PawnEquipment.Equipped"/> but not visible.
	/// </summary>
	Holstered = 3,
}
