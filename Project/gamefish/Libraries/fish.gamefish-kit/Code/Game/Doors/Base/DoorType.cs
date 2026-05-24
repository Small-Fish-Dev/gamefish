namespace GameFish;

[DefaultValue( Rotating )]
public enum DoorType
{
	/// <summary>
	/// Turns on a pivot.
	/// </summary>
	[Icon( "♻" )]
	Rotating,

	/// <summary>
	/// Moves back and forth.
	/// </summary>
	[Icon( "⛸" )]
	Sliding,

	/// <summary>
	/// Manual open/close offsets.
	/// </summary>
	[Icon( "👨‍💻" )]
	Manual,
}