namespace Playground;

public partial struct GlueSettings
{
	/// <summary>
	/// The stickiness of our glue.
	/// </summary>
	[Range( 1f, 15f )]
	public float Strength { get; set; } = 15f;

	/// <summary>
	/// How much forces are softened.
	/// </summary>
	[Range( 0f, 10f )]
	public float Damping { get; set; } = 1f;

	public GlueSettings() { }
}
