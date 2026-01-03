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
	public static bool PressedMiddleMouse => IsPressed( "MMB", isKeyboard: false );

	public static bool HoldingUse => IsDown( "Use", isKeyboard: false );
	public static bool HoldingReload => IsDown( "Reload", isKeyboard: false );
	public static bool HoldingPrimary => IsDown( "Attack1", isKeyboard: false );
	public static bool HoldingSecondary => IsDown( "Attack2", isKeyboard: false );
	public static bool HoldingMiddleMouse => IsDown( "MMB", isKeyboard: false );

	public static bool ReleasedUse => IsReleased( "Use", isKeyboard: false );
	public static bool ReleasedReload => IsReleased( "Reload", isKeyboard: false );
	public static bool ReleasedPrimary => IsReleased( "Attack1", isKeyboard: false );
	public static bool ReleasedSecondary => IsReleased( "Attack2", isKeyboard: false );
	public static bool ReleasedMiddleMouse => IsReleased( "MMB", isKeyboard: false );

	public virtual bool HasAimingFocus => Mouse.Active;
	public virtual bool HasScrollFocus => Mouse.Active || HasAimingFocus;
	public virtual bool HasActionFocus => true; //ShowCursor is true;
	public virtual bool HasMovingFocus => false;

	[Title( "Hints" )]
	[Property, WideMode]
	[Feature( EDITOR ), Group( INPUT ), Order( INPUT_ORDER )]
	public List<ToolFunction> FunctionHints { get; set; }

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

	public virtual void OnCursorToggled( in bool isOpen )
	{
	}

	/// <returns> If the default editor behavior should be prevented. </returns>
	public virtual bool TryLeftClick()
	{
		// this.Log( "Left clicked." );

		if ( !TryTrace( out var tr ) )
			return false;

		OnPrimary( in tr );
		return true;
	}

	/// <returns> If the default editor behavior should be prevented. </returns>
	public virtual bool TryRightClick()
	{
		// this.Log( "Right clicked." );

		if ( !TryTrace( out var tr ) )
			return false;

		OnSecondary( in tr );
		return true;
	}

	/// <returns> If the default editor behavior should be prevented. </returns>
	public virtual bool TryMiddleClick()
	{
		// this.Log( "Middle clicked." );

		if ( !TryTrace( out var tr ) )
			return false;

		OnMiddleMouse( in tr );
		return true;
	}

	public virtual void OnMouseUp( in MouseButtons mb )
	{
		// this.Log( $"Mouse up:[{mb}]" );
	}

	/// <returns> If the default editor behavior should be prevented. </returns>
	public virtual bool TryMouseWheel( in Vector2 scroll )
	{
		// this.Log( $"Mouse wheel:[{dir}]" );

		OnScroll( in scroll );
		return true;
	}

	/// <returns> If the default editor behavior should be prevented. </returns>
	public virtual bool TryMouseDrag( in Vector2 delta )
	{
		// this.Log( "Mouse dragged." );

		return false;
	}

	public virtual void OnMouseDragEnd()
	{
		// this.Log( "Mouse drag ended." );
	}
}
