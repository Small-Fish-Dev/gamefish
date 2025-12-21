using System;
using GameFish;

namespace Playground.Razor;

/// <summary>
/// Has a header you can click on to toggle its contents.
/// </summary>
partial class EditorMenuPanel
{
	protected static Editor Editor => Editor.Instance;

	protected static bool ShowCursor => Editor?.ShowCursor is true;

	[Parameter]
	public string PanelTitle { get; set; }

	[Parameter]
	public bool ShowContents { get; set; } = true;
	public string ContentsClass => ShowContents ? "show" : "hide";

	public override bool WantsMouseInput()
		=> ShowCursor;

	public void TogglePanel()
		=> ShowContents = !ShowContents;

	protected override int BuildHash()
		=> HashCode.Combine( Editor, ShowCursor, ContentsClass );
}
