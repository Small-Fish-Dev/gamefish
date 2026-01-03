namespace Playground;

public partial struct PhysicsWheelSettings
{
	// [Range( 0.1f, 20f )]
	// public float Damping { get; set; } = 120f;

	[Title( "Forward" )]
	public string KeyForward { get; set; } = "KP_8";

	[Title( "Reverse" )]
	public string KeyReverse { get; set; } = "KP_2";

	[Title( "Turn Left" )]
	public string KeyLeft { get; set; } = "KP_4";

	[Title( "Turn Right" )]
	public string KeyRight { get; set; } = "KP_6";

	public PhysicsWheelSettings() { }
}
