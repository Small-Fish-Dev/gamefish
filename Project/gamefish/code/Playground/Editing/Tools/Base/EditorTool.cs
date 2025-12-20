namespace Playground;

[Icon( "build" )]
public abstract partial class EditorTool : PlaygroundModule
{
	protected const int EDITOR_ORDER = DEFAULT_ORDER - 1000;

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

	public virtual void OnEnter() { }
	public virtual void OnExit() { }

	public virtual void FrameSimulate( in float deltaTime )
	{
	}

	public virtual void FixedSimulate( in float deltaTime )
	{
	}

	public virtual void OnLeftClick()
	{
		// this.Log( "Left clicked." );
	}

	public virtual void OnRightClick()
	{
		// this.Log( "Right clicked." );
	}

	public virtual void OnMiddleClick()
	{
		// this.Log( "Middle clicked." );
	}

	public virtual void OnMouseUp( in MouseButtons mb )
	{
		// this.Log( $"Mouse up:[{mb}]" );
	}

	public virtual void OnMouseWheel( in Vector2 dir )
	{
		// this.Log( $"Mouse wheel:[{dir}]" );
	}

	public virtual void OnMouseDrag( in Vector2 delta )
	{
		// this.Log( "Mouse dragged." );
	}

	public virtual void OnMouseDragEnd()
	{
		// this.Log( "Mouse drag ended." );
	}
}
