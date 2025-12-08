namespace GameFish;

partial class Library
{
	/// <summary>
	/// Should the current object be highlighted?
	/// </summary>
	public static bool IsHighlighting => Game.ActiveScene.InGame() || Gizmo.IsSelected;
}
