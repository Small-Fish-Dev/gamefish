using System;
using GameFish;

namespace Playground.Razor;

partial class EditorHints
{
	protected static Editor Editor => Editor.Instance;

	protected static bool HasEditor => Editor.IsValid();
	protected static bool IsEditorOpen => Editor?.IsOpen is true;

	protected static string EditorMenuAction => Editor?.ToggleMenuAction;
	protected static string EditorCursorAction => Editor?.ToggleCursorAction;

	protected static Client Client => Client.Local;
	protected static bool IsSpectator => Client?.Pawn is Spectator;

	protected override int BuildHash()
		=> HashCode.Combine( HasEditor, IsEditorOpen, Client, IsSpectator );
}
