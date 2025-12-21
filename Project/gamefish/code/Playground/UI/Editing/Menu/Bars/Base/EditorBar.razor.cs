using System;
using GameFish;

namespace Playground.Razor;

/// <summary>
/// A docker.
/// </summary>
partial class EditorBar
{
	protected static Editor Editor => Editor.Instance;

	protected static bool ShowCursor => Editor?.ShowCursor is true;

	public override bool WantsMouseInput()
		=> ShowCursor;

	protected override int BuildHash()
		=> HashCode.Combine( Editor, ShowCursor );
}
