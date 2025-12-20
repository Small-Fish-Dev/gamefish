namespace Playground;

partial class EditorTool
{
	public static bool HoldingAlt => Input.Keyboard.Down( "Left Alt" );
	public static bool HoldingShift => Input.Keyboard.Down( "Shift" );
	public static bool HoldingControl => Input.Keyboard.Down( "Control" );

	public static bool PressedUse => Input.Pressed( "Use" );
	public static bool PressedReload => Input.Pressed( "Reload" );
	public static bool PressedPrimary => Input.Pressed( "Attack1" );
	public static bool PressedSecondary => Input.Pressed( "Attack2" );

	public static bool HoldingUse => Input.Down( "Use" );
	public static bool HoldingReload => Input.Down( "Reload" );
	public static bool HoldingPrimary => Input.Down( "Attack1" );
	public static bool HoldingSecondary => Input.Down( "Attack2" );

	public virtual bool PreventAiming => IsMenuOpen;
	public virtual bool PreventMoving => false;

	[Title( "Hints" )]
	[Property, WideMode]
	[Feature( EDITOR ), Group( INPUT ), Order( INPUT_ORDER )]
	public List<ToolFunction> FunctionHints { get; set; }

	[Property]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public TraceFilter Filter { get; set; }

	public virtual bool TryTrace( out SceneTraceResult tr )
		=> Editor.TryTrace( Scene, out tr );
}
