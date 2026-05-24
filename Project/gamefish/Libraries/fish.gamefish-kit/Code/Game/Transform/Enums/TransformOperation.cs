namespace GameFish;

/// <summary>
/// Indicates how to affect a transform.
/// </summary>
public enum TransformOperation
{
	/// <summary> No effect. </summary>
	None,

	/// <summary> Offset. </summary>
	Modify,

	/// <summary> Override. </summary>
	Set,
}
