namespace Playground;

public partial struct RopeSettings
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
	/// The speed to shorten/slack.
	/// </summary>
	[Range( 1f, 2000f )]
	public float LengthSpeed { get; set; } = 150f;

	[Title( "Shorten" )]
	public string KeyShorten { get; set; } = "KP_7";

	[Title( "Lengthen" )]
	public string KeyLengthen { get; set; } = "KP_1";

	public RopeSettings() { }
}
