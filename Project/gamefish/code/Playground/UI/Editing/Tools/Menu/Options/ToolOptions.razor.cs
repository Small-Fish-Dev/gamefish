using System;
using GameFish;

namespace Playground.Razor;

/// <summary>
/// Lists tools in groups you can collapse.
/// </summary>
partial class ToolOptions
{
	protected static Editor Editor => Editor.Instance;

	protected static EditorTool ActiveTool => Editor?.Tool;

	protected static IEnumerable<PropertyDescription> GetToolOptions()
	{
		if ( !ActiveTool.IsValid() )
			return null;

		return TypeLibrary.GetPropertyDescriptions( ActiveTool )
			.Where( p => p?.HasAttribute<ToolOptionAttribute>() is true )
			.OrderBy( p => p.GetDisplayInfo().Order );
	}

	protected override int BuildHash()
		=> HashCode.Combine( Editor, ActiveTool, (RealTime.Now % 1f * 2f).Floor() );
}
