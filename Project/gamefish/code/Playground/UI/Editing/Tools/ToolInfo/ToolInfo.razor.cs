using System;
using GameFish;

namespace Playground.Razor;

partial class ToolInfo
{
	protected static Editor Editor => Editor.Instance;

	protected static EditorTool ActiveTool => Editor?.Tool;

	protected override int BuildHash()
		=> HashCode.Combine( ActiveTool );
}
