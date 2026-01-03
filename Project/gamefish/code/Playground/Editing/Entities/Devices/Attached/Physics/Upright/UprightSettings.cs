namespace Playground;

public partial struct UprightSettings
{
	[Range( 1f, 5000f )]
	public float Force { get; set; } = 2000f;

	[Range( 0f, 30f )]
	public float Damping { get; set; } = 12f;

	// public bool  { get; set; } = 5f;

	public UprightSettings() { }
}
