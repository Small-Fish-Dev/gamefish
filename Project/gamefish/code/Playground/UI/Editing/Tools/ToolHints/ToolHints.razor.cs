using System;
using GameFish;

namespace Playground.Razor;

partial class ToolHints
{
	protected static Editor Editor => Editor.Instance;

	protected static EditorTool ActiveTool => Editor?.Tool;
	protected static List<ToolFunction> Functions => ActiveTool?.FunctionHints;

	protected override int BuildHash()
		=> HashCode.Combine( ActiveTool, Functions );
}
