using System;
using GameFish;

namespace Playground.Razor;

partial class ToolGroup
{
	protected static Editor Editor => Editor.Instance;

	protected static EditorTool ActiveTool => Editor?.Tool;

	protected static IEnumerable<EditorTool> AllTools => Editor?.GetModules<EditorTool>();

	[Parameter]
	public ToolType Type { get; set; }

	[Parameter]
	public bool IsCollapsed { get; set; } = false;

	/// <summary>
	/// The tool group's title/header(if any).
	/// </summary>
	protected string Header { get; set; }

	/// <summary>
	/// The tools within this group.
	/// </summary>
	protected IEnumerable<EditorTool> Tools { get; set; }

	protected override void OnParametersSet()
	{
		base.OnParametersSet();

		Header = Type.GetAttributeOfType<GroupAttribute>()?.Value;

		Tools = AllTools.Where( tool => tool.IsValid() && tool.ToolType == Type );
	}

	public void Toggle()
	{
		IsCollapsed = !IsCollapsed;
	}

	public static void Select( EditorTool tool )
	{
		if ( Editor.TryGetInstance( out var s ) )
			s.TrySetTool( tool );
	}

	protected override int BuildHash()
		=> HashCode.Combine( Tools, ActiveTool, IsCollapsed );
}
