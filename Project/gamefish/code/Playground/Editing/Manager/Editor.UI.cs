namespace Fishbox;

partial class Editor
{
	public const float TRACE_DISTANCE_DEFAULT = 32768f;

	[Property]
	[InputAction]
	[Title( "Toggle Menu" )]
	[Feature( EDITOR ), Group( INPUT )]
	public string ToggleMenuAction { get; set; } = "Editor";

	public bool IsMenuDown => !ToggleMenuAction.IsBlank() && Input.Down( ToggleMenuAction );
	public bool IsMenuPressed => !ToggleMenuAction.IsBlank() && Input.Pressed( ToggleMenuAction );
	public bool IsMenuReleased => !ToggleMenuAction.IsBlank() && Input.Released( ToggleMenuAction );

	[Property]
	[InputAction]
	[Title( "Toggle Cursor" )]
	[Feature( EDITOR ), Group( INPUT )]
	public string ToggleCursorAction { get; set; } = "Cursor";

	public bool IsCursorDown => !ToggleCursorAction.IsBlank() && Input.Down( ToggleCursorAction );
	public bool IsCursorPressed => !ToggleCursorAction.IsBlank() && Input.Pressed( ToggleCursorAction );
	public bool IsCursorReleased => !ToggleCursorAction.IsBlank() && Input.Released( ToggleCursorAction );

	/// <summary>
	/// Should the menu be open?
	/// </summary>
	public virtual bool IsOpen
	{
		get => _isOpen;
		set
		{
			if ( _isOpen == value )
				return;

			_isOpen = value;
			OnSetOpen( _isOpen );
		}
	}

	protected bool _isOpen;

	/// <summary>
	/// Should the menu be open?
	/// </summary>
	public virtual bool ShowCursor
	{
		get => _showCursor;
		set
		{
			if ( _showCursor == value )
				return;

			_showCursor = value;
			OnSetShowCursor( _showCursor );
		}
	}

	protected bool _showCursor;

	public virtual TimeSince? LastOpened { get; set; }

	protected virtual void UpdateUI()
	{
		UpdateCursor();
		UpdateMenu();
	}

	protected virtual void UpdateCursor()
	{
		if ( IsOpen )
		{
			if ( IsCursorPressed )
				ShowCursor = !ShowCursor;
		}
		else
		{
			ShowCursor = IsCursorDown;
		}
	}

	protected virtual void OnSetShowCursor( in bool isVisible )
	{
		Mouse.Visibility = isVisible
			? MouseVisibility.Visible
			: MouseVisibility.Auto;

		if ( Tool.IsValid() )
			Tool.OnCursorToggled( in isVisible );
	}

	protected virtual void UpdateMenu()
	{
		if ( IsMenuPressed )
			OnPressedMenu();

		if ( IsMenuReleased )
			OnReleasedMenu();
	}

	protected virtual void OnPressedMenu()
	{
		IsOpen = !IsOpen;

		if ( IsOpen )
			LastOpened = 0f;
	}

	protected virtual void OnReleasedMenu()
	{
		// Auto-close if held long enough.
		if ( LastOpened.HasValue )
			if ( LastOpened.Value > 0.3f )
				IsOpen = false;
	}

	protected virtual void OnSetOpen( in bool isOpen )
	{
		ShowCursor = isOpen;
	}

	public static bool TryGetAimRay( Scene sc, out Ray ray )
	{
		var cam = sc?.Camera;

		if ( !sc.IsValid() || !cam.IsValid() )
		{
			ray = default;
			return false;
		}

		if ( Mouse.Active )
			ray = cam.ScreenPixelToRay( Mouse.Position );
		else
			ray = new( cam.WorldPosition, cam.WorldRotation.Forward );

		return true;
	}

	public static SceneTraceResult Trace( Scene sc, in Ray ray, in float? dist = null )
	{
		if ( !sc.IsValid() )
			return default;

		var to = ray.Project( dist ?? TRACE_DISTANCE_DEFAULT );

		var tr = sc.Trace.Ray( ray.Position, to );

		var objPawn = Client.Local?.Pawn?.GameObject;

		if ( objPawn.IsValid() )
			tr = tr.IgnoreGameObjectHierarchy( objPawn );

		return tr.Run();
	}

	public static bool TryTrace( Scene sc, out SceneTraceResult tr, in float? dist = null )
	{
		if ( !TryGetAimRay( sc, out var ray ) )
		{
			tr = default;
			return false;
		}

		tr = Trace( sc, ray, dist: dist );

		return true;
	}
}
