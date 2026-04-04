using System;

namespace GameFish;

/// <summary>
/// The relevant poles.
/// </summary>
public enum RotationAxis
{
	/// <summary>
	/// Up and down.
	/// </summary>
	[Icon( "↕" )]
	Pitch,

	/// <summary>
	/// Left and right.
	/// </summary>
	[Icon( "↔" )]
	Yaw,

	/// <summary>
	/// Sideways.
	/// </summary>
	[Icon( "🔄" )]
	Roll,
}
