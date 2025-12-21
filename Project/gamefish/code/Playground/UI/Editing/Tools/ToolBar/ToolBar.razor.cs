using System;
using GameFish;

namespace Playground.Razor;

partial class ToolBar
{
	protected static Editor Editor => Editor.Instance;

	protected static bool ShowCursor => Editor?.ShowCursor is true;

	protected static EditorTool ActiveTool => Editor?.Tool;
	protected static IEnumerable<EditorTool> AllTools => Editor?.GetModules<EditorTool>();

	public override bool WantsMouseInput()
		=> ShowCursor;

	protected static IOrderedEnumerable<ToolType> GetToolGroups()
	{
		return AllTools?.ToList()
			.Where( tool => tool.IsValid() )
			.Select( tool => tool.ToolType )
			.DistinctBy( type => type )
			.OrderBy( type => type.GetAttributeOfType<OrderAttribute>()?.Value );
	}

	protected override int BuildHash()
		=> HashCode.Combine( AllTools, ActiveTool );
}
