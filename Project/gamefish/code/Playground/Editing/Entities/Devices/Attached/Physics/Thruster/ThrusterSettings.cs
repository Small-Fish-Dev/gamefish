namespace Playground;

public partial struct ThrusterSettings
{
	/// <summary>
	/// The push to apply.
	/// </summary>
	[Range( 1f, 9999f )]
	public float Force { get; set; } = 1000f;

	/// <summary>
	/// The keyboard code(if any) to push it forward.
	/// </summary>
	[Title( "Forward Key" )]
	public string KeyForward { get; set; } = "KP_8";

	/// <summary>
	/// The keyboard code(if any) to push it backward.
	/// </summary>
	[Title( "Backward Key" )]
	public string KeyBackward { get; set; } = "KP_2";

	public ThrusterSettings() { }
}
