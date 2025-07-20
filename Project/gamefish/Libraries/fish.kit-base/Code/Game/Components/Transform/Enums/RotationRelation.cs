namespace GameFish;

/// <summary>
/// Indicates what direction to move towards or rotate around.
/// </summary>
public enum RotationRelation
{
	/// <summary>
	/// Rotate relative to the object's rotation.
	/// </summary>
	[Icon( "🔀" )]
	Object,

	/// <summary>
	/// Rotate relative to a specific axis.
	/// </summary>
	[Icon( "🔄" )]
	Axis,

	/// <summary>
	/// Rotate on the static world axis with an offset.
	/// </summary>
	[Icon( "🌐" )]
	Absolute,
}
