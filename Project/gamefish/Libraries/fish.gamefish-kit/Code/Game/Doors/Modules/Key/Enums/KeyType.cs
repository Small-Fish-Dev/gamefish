namespace GameFish;

/// <summary>
/// Describes how a key opens a door.
/// </summary>
[DefaultValue( Color )]
public enum KeyType
{
	/// <summary>
	/// Color must match. Classic.
	/// </summary>
	[Icon( "🎨" )]
	Color,

	/// <summary>
	/// The exact ID must match.
	/// </summary>
	[Icon( "⌨" )]
	ID,
}
