namespace Playground;

public partial struct ArmSettings
{
	/// <summary>
	/// The speed to rotate.
	/// </summary>
	[Range( 0.1f, 360f )]
	public float Speed { get; set; } = 120f;

	[Title( "Pitch Up" )]
	public string KeyPitchUp { get; set; } = "KP_9";

	[Title( "Pitch Down" )]
	public string KeyPitchDown { get; set; } = "KP_3";

	[Title( "Yaw Left" )]
	public string KeyYawUp { get; set; }

	[Title( "Yaw Right" )]
	public string KeyYawDown { get; set; }

	public ArmSettings() { }
}
