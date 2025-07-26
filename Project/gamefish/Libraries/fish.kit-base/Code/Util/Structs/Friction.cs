namespace GameFish;

/// <summary>
/// Inspector-friendly data for the <see cref="Vector3.WithFriction(float, float)"/> method.
/// </summary>
public struct Friction
{
	[KeyProperty]
	public float Value { get; set; } = 5f;

	[KeyProperty]
	public float StopSpeed { get; set; } = 140f;

	public Friction() { }

	public Friction( in float val, in float stopSpeed )
	{
		Value = val;
		StopSpeed = stopSpeed;
	}
}
