namespace GameFish;

partial class Library
{
	/// <summary>
	/// Should the current object be highlighted?
	/// </summary>
	public static bool IsHighlighting => Gizmo.IsSelected || Game.ActiveScene.InGame();
}
