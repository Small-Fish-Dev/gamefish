using System;

namespace GameFish;

[Flags]
public enum GizmoPass
{
	/// <summary>
	/// Not rendered at all.
	/// </summary>
	[Hide]
	[Icon( "🙈" )]
	None = 0 << 1,

	/// <summary>
	/// Seen in-world(not through walls).
	/// </summary>
	[Icon( "🌎" )]
	Depth = 1 << 1,

	/// <summary>
	/// Seen through all walls.
	/// </summary>
	[Icon( "🌐" )]
	Overlay = 1 << 2,

	/// <summary>
	/// Rendered both with and without any depth.
	/// </summary>
	[Hide]
	[Icon( "🌭" )]
	Both = Depth | Overlay,
}
