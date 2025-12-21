using System;
using GameFish;

namespace Playground.Razor;

partial class EditorMenu
{
	protected static Editor Editor => Editor.Instance;
	protected static bool HasEditor => Editor.IsValid();

	public static bool IsOpen => Editor?.IsOpen is true;
	public static bool ShowCursor => Editor?.ShowCursor is true;

	protected static string MenuClass => IsOpen ? "show" : "hide";

	protected override int BuildHash()
		=> HashCode.Combine( IsOpen );
}
