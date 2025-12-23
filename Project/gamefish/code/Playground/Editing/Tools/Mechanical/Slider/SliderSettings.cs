namespace Playground;

public partial struct SliderSettings
{
	/// <summary>
	/// The speed to slide back and forth.
	/// </summary>
	[Range( 1f, 2000f )]
	public float Speed { get; set; } = 120f;

	[Title( "Toggle" )]
	public string KeyToggle { get; set; } = "KP_MULTIPLY";

	[Title( "Shorten" )]
	public string KeyShorten { get; set; } = "KP_MINUS";

	[Title( "Lengthen" )]
	public string KeyLengthen { get; set; } = "KP_PLUS";

	public SliderSettings() { }
}
