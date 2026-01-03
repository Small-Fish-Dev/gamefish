namespace Playground;

public partial struct SpringSettings
{
	/// <summary>
	/// The extra length it can spring.
	/// </summary>
	[Range( 1f, 2f )]
	public float Slack { get; set; } = 256f;

	/// <summary>
	/// The softening of spring forces.
	/// </summary>
	[Range( 0f, 10f )]
	public float Damping { get; set; } = 0.7f;

	/// <summary>
	/// The strength of the spring.
	/// </summary>
	[Range( 0.1f, 20f )]
	public float Springiness { get; set; } = 5f;

	/// <summary>
	/// The length multiplayer to toggle to.
	/// </summary>
	[Range( 0f, 10f )]
	public float ToggleLength { get; set; } = 0.1f;

	[Title( "Toggle" )]
	public string KeyToggle { get; set; } = "KP_INSERT";

	public SpringSettings() { }
}
