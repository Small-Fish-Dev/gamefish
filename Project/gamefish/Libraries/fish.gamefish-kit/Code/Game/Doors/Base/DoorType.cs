namespace GameFish;

[DefaultValue( Sliding )]
public enum DoorType
{
	/// <summary>
	/// Moves back and forth.
	/// </summary>
	[Icon( "⛸" )]
	Sliding,

	/// <summary>
	/// Turns on a pivot.
	/// </summary>
	[Icon( "♻" )]
	Rotating,

	/// <summary>
	/// Manual open/close offsets.
	/// </summary>
	[Icon( "👨‍💻" )]
	Manual,
}