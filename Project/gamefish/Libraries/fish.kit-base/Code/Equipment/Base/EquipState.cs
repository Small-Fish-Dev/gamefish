namespace GameFish;

/// <summary>
/// Indicates if an equipment is dropped, deployed, or holstered.
/// </summary>
public enum EquipState
{
	/// <summary>
	/// Probably on the ground somewhere.
	/// </summary>
	Dropped,

	/// <summary>
	/// Actively held in an <see cref="BasePawn"/>'s hands.
	/// </summary>
	Deployed,

	/// <summary>
	/// Kept in <see cref="PawnEquipment.Equipped"/> but not visible.
	/// </summary>
	Holstered,
}
