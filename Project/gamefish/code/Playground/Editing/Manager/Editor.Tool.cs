namespace Playground;

partial class Editor
{
	[Property]
	[Title( "Active" )]
	[Feature( EDITOR ), Group( MODE ), Order( TOOL_ORDER )]
	public EditorTool Tool
	{
		get => _mode;
		protected set
		{
			var old = _mode;
			_mode = value;

			OnToolSet( _mode, old );
		}
	}

	protected EditorTool _mode;

	public virtual bool TrySetTool( EditorTool tool )
	{
		if ( !tool.IsValid() )
		{
			Tool = null;
			return true;
		}

		if ( !tool.IsClientAllowed( Client.Local ) )
			return false;

		Tool = tool;

		return true;
	}

	protected virtual void OnToolSet( EditorTool newTool, EditorTool oldTool )
	{
		if ( oldTool.IsValid() )
			oldTool.OnExit();

		if ( newTool.IsValid() )
			newTool.OnEnter();
	}

	protected virtual void SimulateTool( in float deltaTime, bool isFixedUpdate )
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
}
