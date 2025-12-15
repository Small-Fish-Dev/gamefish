using GameFish;

namespace Playground;

/// <summary>
/// Allows for the editing of what's around you.
/// </summary>
[Group( NAME )]
[Icon( "edit_note" )]
public partial class Editor : Singleton<Editor>
{
	protected const int EDITOR_ORDER = DEFAULT_ORDER - 1000;

	protected const int MODE_ORDER = EDITOR_ORDER + 1;
	protected const int TRACING_ORDER = EDITOR_ORDER + 100;

	public const float TRACE_DISTANCE_DEFAULT = 32768f;

	[Property, InlineEditor]
	[Title( "Draw Entity Boxes" )]
	[Feature( EDITOR ), Group( DEBUG )]
	public bool DrawEntityBounds { get; set; }

	[Property]
	[InputAction]
	[Title( "Show Menu" )]
	[Feature( EDITOR ), Group( INPUT )]
	public string ShowMenuAction { get; set; } = "Editor";

	[Property]
	[Title( "Active" )]
	[Feature( EDITOR ), Group( MODE ), Order( MODE_ORDER )]
	public EditorTool Tool
	{
		get => _mode;
		set
		{
			var old = _mode;
			_mode = value;

			OnEditorToolSet( _mode, old );
		}
	}

	private EditorTool _mode;

	/// <summary>
	/// Should the menu be open?
	/// </summary>
	public bool IsOpen
	{
		get => _isOpen;
		set
		{
			_isOpen = value;
			Mouse.Visibility = _isOpen ? MouseVisibility.Visible : MouseVisibility.Auto;
		}
	}

	protected bool _isOpen;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// There can only be one.. (active, at a time)
		if ( TryGetInstance( out var e ) && e != this )
			return;

		UpdateMenu();

		SimulateTool( Time.Delta, isFixedUpdate: false );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		// There can only be one.. (active, at a time)
		if ( TryGetInstance( out var e ) && e != this )
			return;

		SimulateTool( Time.Delta, isFixedUpdate: true );
	}

	protected void UpdateMenu()
	{
		if ( ShowMenuAction.IsBlank() )
			return;

		if ( Input.Pressed( ShowMenuAction ) )
			IsOpen = true;

		if ( Input.Released( ShowMenuAction ) )
			IsOpen = false;
	}

	protected void SimulateTool( in float deltaTime, bool isFixedUpdate )
	{
		if ( !IsOpen )
			return;

		if ( !Tool.IsValid() )
			return;

		if ( isFixedUpdate )
			Tool.FixedSimulate( deltaTime );
		else
			Tool.FrameSimulate( deltaTime );
	}

	protected static void OnEditorToolSet( EditorTool neworTool, EditorTool oldorTool )
	{
		if ( oldorTool.IsValid() )
			oldorTool.OnExit();

		if ( neworTool.IsValid() )
			neworTool.OnEnter();
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

		return tr.Hit;
	}

	[Rpc.Broadcast( NetFlags.UnreliableNoDelay )]
	public virtual void BroadcastImpulse( GameObject obj, Vector3 vel )
	{
		if ( !obj.IsValid() )
			return;

		const FindMode findMode = FindMode.EnabledInSelf | FindMode.InAncestors;

		if ( obj.Components.TryGet<IVelocity>( out var iVel, findMode ) )
			iVel.Velocity += vel;
		else if ( obj.Components.TryGet<Rigidbody>( out var rb, findMode ) )
			rb.Velocity += vel;
	}
}
