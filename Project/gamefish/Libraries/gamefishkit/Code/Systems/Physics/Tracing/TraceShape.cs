namespace GameFish;

/// <summary>
/// The kinds of shapes usable when tracing.
/// </summary>
[DefaultValue( Line )]
public enum TraceShape
{
	/// <summary> Straight from A to B. </summary>
	[Icon( "📏" )] Line,

	/// <summary> A bounding box. </summary>
	[Icon( "🔳" )] Box,

	/// <summary> A radius in 3D. </summary>
	[Icon( "⚪" )] Sphere,

	/// <summary> A pill shape. </summary>
	[Icon( "💊" )] Capsule,

	/// <summary> A filled tube. </summary>
	[Icon( "🍼" )] Cylinder,
}
