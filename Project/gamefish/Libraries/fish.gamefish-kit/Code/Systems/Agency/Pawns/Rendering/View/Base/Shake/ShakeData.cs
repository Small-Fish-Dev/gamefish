namespace GameFish;

/// <summary>
/// A rumble from somewhere in the world.
/// </summary>
public readonly struct ShakeData : IValid
{
	public readonly bool IsValid => ITransform.IsValid( Origin )
		&& Intensity != 0f
		&& Radius > 0;

	/// <summary>
	/// The shake will push your view from here.
	/// </summary>
	public readonly Vector3 Origin { get; }

	/// <summary>
	/// The distance from the origin where intensity is at minimum.
	/// </summary>
	public readonly float Radius { get; } = 2048;

	/// <summary>
	/// The maximum strength of the shake.
	/// </summary>
	public readonly float Intensity { get; } = 2048;

	/// <summary>
	/// Always has at least this much intensity.
	/// </summary>
	public readonly float Minimum { get; } = 0f;

	/// <summary>
	/// Always has at least this much intensity.
	/// </summary>
	public readonly float? Duration { get; }

	public ShakeData() { }

	public ShakeData( in Vector3 from, in float intensity )
	{
		Origin = from;
		Intensity = intensity;
	}

	public ShakeData( in Vector3 from, in float intensity, in float r, in float min = 0f )
	{
		Origin = from;
		Radius = r;

		Intensity = intensity;
		Minimum = min;
	}
}
