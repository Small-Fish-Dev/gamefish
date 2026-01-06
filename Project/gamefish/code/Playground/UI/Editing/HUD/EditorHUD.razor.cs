using System;
using GameFish;

namespace Fishbox.Razor;

/// <summary>
/// The HUD that shows when you don't have the menu open.
/// </summary>
partial class EditorHUD
{
	protected static Editor Editor => Editor.Instance;

	protected static bool IsMenuOpen => EditorMenu.IsOpen;

	protected override int BuildHash()
		=> HashCode.Combine( IsMenuOpen );
}
