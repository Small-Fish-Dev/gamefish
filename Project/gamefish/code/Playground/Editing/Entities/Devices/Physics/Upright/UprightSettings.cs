namespace Playground;

public partial struct UprightSettings
{
	[Range( 1f, 50f )]
	public float Force { get; set; } = 200f;

	[Range( 0f, 99f )]
	public float Damping { get; set; } = 10f;

	// public bool  { get; set; } = 5f;

	public UprightSettings() { }
}
