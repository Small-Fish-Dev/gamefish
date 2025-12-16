using System;
using GameFish;

namespace Playground.Razor;

partial class ToolBar
{
	protected static Editor Editor => Editor.Instance;

	protected static EditorTool ActiveTool => Editor?.Tool;

	protected static IEnumerable<EditorTool> Tools => Editor?.GetModules<EditorTool>();

	public static void Select( EditorTool tool )
	{
		if ( Editor.TryGetInstance( out var s ) )
			s.TrySetTool( tool );
	}

	protected override int BuildHash()
		=> HashCode.Combine( Tools, ActiveTool );
}
