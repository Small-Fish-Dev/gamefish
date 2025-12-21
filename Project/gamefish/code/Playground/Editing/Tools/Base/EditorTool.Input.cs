namespace Playground;

partial class EditorTool
{
	public static bool HoldingAlt => IsDown( "ALT", isKeyboard: true );
	public static bool HoldingShift => IsDown( "SHIFT", isKeyboard: true );
	public static bool HoldingControl => IsDown( "CTRL", isKeyboard: true );

	public static bool PressedUse => IsPressed( "Use", isKeyboard: false );
	public static bool PressedReload => IsPressed( "Reload", isKeyboard: false );
	public static bool PressedPrimary => IsPressed( "Attack1", isKeyboard: false );
	public static bool PressedSecondary => IsPressed( "Attack2", isKeyboard: false );

	public static bool HoldingUse => IsDown( "Use", isKeyboard: false );
	public static bool HoldingReload => IsDown( "Reload", isKeyboard: false );
	public static bool HoldingPrimary => IsDown( "Attack1", isKeyboard: false );
	public static bool HoldingSecondary => IsDown( "Attack2", isKeyboard: false );

	public static bool ReleasedUse => IsReleased( "Use", isKeyboard: false );
	public static bool ReleasedReload => IsReleased( "Reload", isKeyboard: false );
	public static bool ReleasedPrimary => IsReleased( "Attack1", isKeyboard: false );
	public static bool ReleasedSecondary => IsReleased( "Attack2", isKeyboard: false );

	public virtual bool PreventAiming => ShowCursor is true;
	public virtual bool PreventAction => true; //ShowCursor is true;

	public virtual bool PreventMoving => false;

	[Title( "Hints" )]
	[Property, WideMode]
	[Feature( EDITOR ), Group( INPUT ), Order( INPUT_ORDER )]
	public List<ToolFunction> FunctionHints { get; set; }

	[Property]
	[ToolOption]
	[Title( "Trace Filter" )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public TraceFilter Filter { get; set; }

	public virtual bool TryTrace( out SceneTraceResult tr )
		=> Editor.TryTrace( Scene, out tr );

	/// <returns> If this action/key is being pressed. </returns>
	protected static bool IsDown( string code, bool isKeyboard )
	{
		return isKeyboard
			? Input.Keyboard.Down( code )
			: Input.Down( code );
	}

	/// <returns> If this action/key was pressed. </returns>
	protected static bool IsPressed( string code, bool isKeyboard )
	{
		return isKeyboard
			? Input.Keyboard.Pressed( code )
			: Input.Pressed( code );
	}

	/// <returns> If this action/key was released. </returns>
	protected static bool IsReleased( string code, bool isKeyboard )
	{
		return isKeyboard
			? Input.Keyboard.Released( code )
			: Input.Released( code );
	}
}
