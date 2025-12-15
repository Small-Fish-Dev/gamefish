namespace Playground;

[Icon( "build" )]
public abstract class EditorTool : PlaygroundModule
{
	protected const int EDITOR_ORDER = DEFAULT_ORDER - 1000;

	public override bool IsParent( ModuleEntity comp )
		=> comp is Editor;

	public Editor Editor => Parent as Editor;

	[Property]
	[Feature( EDITOR ), Group( DISPLAY ), Order( EDITOR_ORDER )]
	public string ToolName { get; set; } = "Tool";

	[Property, TextArea]
	[Feature( EDITOR ), Group( DISPLAY ), Order( EDITOR_ORDER )]
	public string ToolDescription { get; set; } = "Does stuff.";

	[Property, WideMode]
	[Feature( EDITOR ), Group( INPUT ), Order( EDITOR_ORDER )]
	public List<ToolFunction> FunctionHints { get; set; }

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
