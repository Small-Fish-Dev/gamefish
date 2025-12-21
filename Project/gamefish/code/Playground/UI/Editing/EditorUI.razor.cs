using System;
using GameFish;

namespace Playground.Razor;

partial class EditorUI
{
	protected static Editor Editor => Editor.Instance;
	protected static bool HasEditor => Editor.IsValid();

	public static bool IsOpen => Editor?.IsOpen is true;
	public static bool ShowCursor => Editor?.ShowCursor is true;

	public override bool WantsMouseInput()
		=> ShowCursor;

	protected override int BuildHash()
		=> HashCode.Combine( IsOpen );
}
