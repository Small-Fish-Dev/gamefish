namespace GameFish;

/// <summary>
/// Fancy words representing numbers 1-9.
/// </summary>
public enum EquipSlot : int
{
	/// <summary>
	/// No particular slot. <br />
	/// Automatic placement.
	/// </summary>
	[Title( "None" )] None = 0,

	/// <summary> Primary. </summary>
	[Title( "1" )] Primary = 1,

	/// <summary> Secondary. </summary>
	[Title( "2" )] Secondary = 2,

	/// <summary> Tertiary. </summary>
	[Title( "3" )] Tertiary = 3,

	/// <summary> Quatenary. </summary>
	[Title( "4" )] Quaternary = 4,

	/// <summary> Quinary. </summary>
	[Title( "5" )] Quinary = 5,

	/// <summary> Senary. </summary>
	[Title( "6" )] Senary = 6,

	/// <summary> Septenary. </summary>
	[Title( "7" )] Septenary = 7,

	/// <summary> Octonary. </summary>
	[Title( "8" )] Octonary = 8,

	/// <summary> Nonary. </summary>
	[Title( "9" )] Nonary = 9,
}
