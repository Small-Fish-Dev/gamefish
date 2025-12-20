namespace Playground;

public partial struct SpinnerSettings
{
	/// <summary>
	/// The spin speed to apply.
	/// </summary>
	[Range( 0.1f, 360f )]
	public float Speed { get; set; } = 30f;

	/// <summary>
	/// The keyboard code(if any) to rotate forward.
	/// </summary>
	[Title( "Forward Key" )]
	public string KeyForward { get; set; } = "KP_6";

	/// <summary>
	/// The keyboard code(if any) to rotate backward.
	/// </summary>
	[Title( "Reverse Key" )]
	public string KeyReverse { get; set; } = "KP_4";

	public SpinnerSettings() { }
}
