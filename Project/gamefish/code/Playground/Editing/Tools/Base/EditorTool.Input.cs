namespace Playground;

partial class EditorTool
{
	public static bool HoldingAlt => Input.Keyboard.Down( "ALT" );
	public static bool HoldingShift => Input.Keyboard.Down( "SHIFT" );
	public static bool HoldingControl => Input.Keyboard.Down( "CTRL" );

	public static bool PressedUse => IsPressed( "Use", isKeyboard: false );
	public static bool PressedReload => IsPressed( "Reload", isKeyboard: false );
	public static bool PressedPrimary => IsPressed( "Attack1", isKeyboard: false );
	public static bool PressedSecondary => IsPressed( "Attack2", isKeyboard: false );

	public static bool HoldingUse => IsDown( "Use", isKeyboard: false );
	public static bool HoldingReload => IsDown( "Reload", isKeyboard: false );
	public static bool HoldingPrimary => IsDown( "Attack1", isKeyboard: false );
	public static bool HoldingSecondary => IsDown( "Attack2", isKeyboard: false );

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

	/// <returns> If this action/key is allowed and was pressed. </returns>
	protected static bool IsPressed( string code, bool isKeyboard )
	{
		if ( !Client.IsValid() || code.IsBlank() )
			return false;

		return Client.IsButtonPressed( code, isKeyboard );
	}

	/// <returns> If this action/key is allowed and being pressed. </returns>
	protected static bool IsDown( string code, bool isKeyboard )
	{
		if ( !Client.IsValid() || code.IsBlank() )
			return false;

		return Client.IsButtonDown( code, isKeyboard );
	}
}
