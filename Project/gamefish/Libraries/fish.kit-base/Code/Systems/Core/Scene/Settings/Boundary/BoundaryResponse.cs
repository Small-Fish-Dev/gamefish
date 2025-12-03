namespace GameFish;

/// <summary>
/// What happens in relation to a boundary?
/// </summary>
public enum BoundaryResponse
{
	/// <summary>
	/// Not a dang thing.
	/// </summary>
	[Icon( "🥱" )] Nothing,

	/// <summary>
	/// Drag 'em back.
	/// </summary>
	[Icon( "🌌" )] Teleport,

	/// <summary>
	/// Revive them.
	/// </summary>
	[Icon( "✨" )] Respawn,

	/// <summary>
	/// Get rid of 'em.
	/// </summary>
	[Icon( "💥" )] Destroy,

	/// <summary>
	/// Beat 'em up
	/// </summary>
	[Icon( "👊" )] Damage,
}
