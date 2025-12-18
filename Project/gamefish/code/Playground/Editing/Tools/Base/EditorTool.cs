namespace Playground;

[Icon( "build" )]
public abstract partial class EditorTool : PlaygroundModule
{
	protected const int EDITOR_ORDER = DEFAULT_ORDER - 1000;

	protected const int INPUT_ORDER = EDITOR_ORDER + 25;
	protected const int PREFABS_ORDER = EDITOR_ORDER + 50;
	protected const int SETTINGS_ORDER = EDITOR_ORDER + 100;

	public const string DEFAULT_EMOJI = "⚪";

	public override bool IsParent( ModuleEntity comp )
		=> comp is Editor;

	public Editor Editor => Parent as Editor;

	public bool IsMenuOpen => Editor.IsValid() && Editor.IsOpen;
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

	[Title( "Hints" )]
	[Property, WideMode]
	[Feature( EDITOR ), Group( INPUT ), Order( INPUT_ORDER )]
	public List<ToolFunction> FunctionHints { get; set; }

	[Property]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public TraceFilter Filter { get; set; }

	public static bool HoldingAlt => Input.Keyboard.Down( "Left Alt" );
	public static bool HoldingShift => Input.Keyboard.Down( "Shift" );
	public static bool HoldingControl => Input.Keyboard.Down( "Control" );

	public static bool PressedUse => Input.Pressed( "Use" );

	public static bool PressedPrimary => Input.Pressed( "Attack1" );
	public static bool PressedSecondary => Input.Pressed( "Attack2" );

	/// <summary>
	/// Quickly checks permision and gives you a client reference.
	/// </summary>
	/// <returns> If the connection(such as an RPC caller) is allowed to use this tool. </returns>
	protected bool TryUse( Connection cn, out Client cl )
		=> Server.TryFindClient( cn, out cl ) && IsClientAllowed( cl );

	public virtual bool IsClientAllowed( Client cl )
		=> !IsAdminOnly || cl?.Connection?.IsHost is true;

	public virtual void OnEnter() { }
	public virtual void OnExit() { }

	public virtual void FrameSimulate( in float deltaTime )
	{
	}

	public virtual void FixedSimulate( in float deltaTime )
	{
	}

	public virtual bool TryTrace( out SceneTraceResult tr )
		=> Editor.TryTrace( Scene, out tr );
}
