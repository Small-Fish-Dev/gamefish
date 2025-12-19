namespace Playground;

partial class Editor
{
	public const float TRACE_DISTANCE_DEFAULT = 32768f;

	[Property]
	[InputAction]
	[Title( "Show Menu" )]
	[Feature( EDITOR ), Group( INPUT )]
	public string ShowMenuAction { get; set; } = "Editor";

	/// <summary>
	/// Should the menu be open?
	/// </summary>
	public virtual bool IsOpen
	{
		get => _isOpen;
		set
		{
			_isOpen = value;
			OnSetIsOpen( _isOpen );
		}
	}

	protected bool _isOpen;

	public virtual TimeSince? LastOpened { get; set; }

	protected void UpdateMenu()
	{
		if ( ShowMenuAction.IsBlank() )
			return;

		if ( Input.Pressed( ShowMenuAction ) )
			OnPressedOpen();

		if ( Input.Released( ShowMenuAction ) )
			OnReleasedOpen();
	}

	protected virtual void OnPressedOpen()
	{
		IsOpen = !IsOpen;

		if ( IsOpen )
			LastOpened = 0f;
	}

	protected virtual void OnReleasedOpen()
	{
		// Auto-close if held long enough.
		if ( LastOpened.HasValue )
			if ( LastOpened.Value > 0.3f )
				IsOpen = false;
	}

	protected virtual void OnSetIsOpen( in bool isOpen )
	{
		Mouse.Visibility = _isOpen ? MouseVisibility.Visible : MouseVisibility.Auto;
	}

	public static SceneTraceResult Trace( Scene sc, in Vector3 start, in Vector3 dir, in float? dist = null )
	{
		if ( !sc.IsValid() )
			return default;

		var to = start + dir * (dist ?? TRACE_DISTANCE_DEFAULT);

		var tr = sc.Trace.Ray( start, to );

		var objPawn = Client.Local?.Pawn?.GameObject;

		if ( objPawn.IsValid() )
			tr = tr.IgnoreGameObjectHierarchy( objPawn );

		return tr.Run();
	}

	public static bool TryTrace( Scene sc, out SceneTraceResult tr, in float? dist = null )
	{
		var cam = sc?.Camera;

		if ( !TryGetInstance( out var e ) || !sc.IsValid() || !cam.IsValid() )
		{
			tr = default;
			return false;
		}

		if ( Mouse.Active )
		{
			var ray = cam.ScreenPixelToRay( Mouse.Position );
			tr = Trace( sc, ray.Position, ray.Forward, dist );
		}
		else
		{
			tr = Trace( sc, cam.WorldPosition, cam.WorldRotation.Forward, dist );
		}

		return true;
	}
}
