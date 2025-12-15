namespace Playground;

public struct ToolFunction
{
	private const int DISPLAY_ORDER = 0;
	private const int ACTION_ORDER = 1;

	[KeyProperty]
	[Group( DISPLAY ), Order( DISPLAY_ORDER )]
	public string Name { get; set; }

	[TextArea]
	[Group( DISPLAY ), Order( DISPLAY_ORDER )]
	public string Description { get; set; }

	[KeyProperty, InputAction]
	[Group( INPUT ), Order( DISPLAY_ORDER )]
	public string Action { get; set; } = "Attack1";

	[KeyProperty]
	[Group( INPUT ), Order( DISPLAY_ORDER )]
	public InputMode Mode { get; set; } = InputMode.Pressed;

	public ToolFunction() { }

	public readonly bool IsInputting()
	{
		if ( Action.IsBlank() )
			return false;

		return Mode.Active( Action );
	}
}
