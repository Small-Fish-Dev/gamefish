using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;

namespace Playground;

[Icon( "build" )]
public abstract partial class EditorTool : PlaygroundModule
{
	protected const int EDITOR_ORDER = DEFAULT_ORDER - 1000;
	protected const int EDITOR_DEBUG_ORDER = EDITOR_ORDER - 100;

	protected const int INPUT_ORDER = EDITOR_ORDER + 25;
	protected const int SOUNDS_ORDER = EDITOR_ORDER + 50;
	protected const int PREFABS_ORDER = EDITOR_ORDER + 100;
	protected const int SETTINGS_ORDER = EDITOR_ORDER + 150;

	public const string DEFAULT_EMOJI = "⚪";

	public override bool IsParent( ModuleEntity comp )
		=> comp is Editor;

	public Editor Editor => Parent as Editor;

	public static Client Client => Client.Local;

	public bool IsMenuOpen => Editor.IsValid() && Editor.IsOpen;
	public bool ShowCursor => Editor.IsValid() && Editor.ShowCursor;

	public bool IsSelected => Editor.IsValid() && Editor.Tool == this;

	/// <summary>
	/// Restricts this tool the the host and/or authorized users only.
	/// </summary>
	[Property]
	[Feature( EDITOR ), Group( SECURITY ), Order( EDITOR_ORDER )]
	public bool IsAdminOnly { get; set; } = false;

	[Property]
	[Title( "Group" )]
	[Feature( EDITOR ), Group( DISPLAY ), Order( EDITOR_ORDER )]
	public ToolType ToolType { get; set; } = ToolType.Default;

	[Property]
	[Title( "Emoji" )]
	[Feature( EDITOR ), Group( DISPLAY ), Order( EDITOR_ORDER )]
	public string ToolEmoji { get; set; } = DEFAULT_EMOJI;

	[Property]
	[Title( "Name" )]
	[Feature( EDITOR ), Group( DISPLAY ), Order( EDITOR_ORDER )]
	public string ToolName { get; set; } = "Tool";

	[Property, TextArea]
	[Title( "Description" )]
	[Feature( EDITOR ), Group( DISPLAY ), Order( EDITOR_ORDER )]
	public string ToolDescription { get; set; } = "Does stuff.";

	/// <summary>
	/// Quickly checks permision and gives you a client reference.
	/// </summary>
	/// <returns> If the connection(such as an RPC caller) is allowed to use this tool. </returns>
	protected bool TryUse( Connection cn, out Client cl )
		=> Server.TryFindClient( cn, out cl ) && IsClientAllowed( cl );

	public virtual bool IsClientAllowed( Client cl )
		=> !IsAdminOnly || cl?.Connection?.IsHost is true;

	public virtual void OnEnter()
	{
		ClearTarget();
	}

	public virtual void OnExit() { }

	public virtual void FrameSimulate( in float deltaTime )
	{
		FindTarget( in deltaTime );

		UpdateActions( in deltaTime );

		UpdateOrigin( in deltaTime );

		RenderHelpers();
	}

	public virtual void FixedSimulate( in float deltaTime )
	{
	}

	/// <summary>
	/// Resets the tool's operating parameters.
	/// </summary>
	protected virtual void Clear()
	{
		ClearTarget();
		ClearOrigin();
	}

	protected virtual bool TryGetOffsetFromTrace( in SceneTraceResult tr, out Offset offset )
	{
		if ( !tr.Hit || !tr.GameObject.IsValid() )
		{
			offset = default;
			return false;
		}

		var tOrigin = tr.GameObject.WorldTransform;

		var pos = tOrigin.PointToLocal( tr.EndPosition );
		var r = tOrigin.RotationToLocal( Rotation.LookAt( tr.Normal ) );

		offset = new( pos, r );

		return true;
	}

	/// <summary>
	/// Do stuff directly from action buttons being pressed.
	/// </summary>
	protected virtual void UpdateActions( in float deltaTime )
	{
		// Panel sends left/right click events.
		// TODO: Not this. Something less stupid.
		if ( !Mouse.Active )
		{
			if ( Input.MouseWheel != default )
				TryMouseWheel( -Input.MouseWheel );

			if ( PressedPrimary )
				if ( TryTrace( out var tr ) )
					OnPrimary( in tr );

			if ( PressedSecondary )
				if ( TryTrace( out var tr ) )
					OnSecondary( in tr );

			if ( PressedMiddleMouse )
				if ( TryTrace( out var tr ) )
					OnMiddleMouse( in tr );
		}

		if ( PressedReload )
			if ( TryTrace( out var tr ) )
				OnReload( in tr );

		if ( PressedUse )
			if ( TryTrace( out var tr ) )
				OnUse( in tr );
	}

	protected virtual void OnPrimary( in SceneTraceResult tr )
	{
	}

	protected virtual void OnSecondary( in SceneTraceResult tr )
	{
	}

	protected virtual void OnMiddleMouse( in SceneTraceResult tr )
	{
	}

	protected virtual void OnUse( in SceneTraceResult tr )
	{
	}

	protected virtual void OnReload( in SceneTraceResult tr )
	{
	}

	protected virtual void OnScroll( in Vector2 scroll )
	{
	}

}
