namespace GameFish;

/// <summary>
/// 🎮🐟 <br />
/// Provides utilities(such as extensions) to the Game Fish library.
/// </summary>
public static partial class Library
{
	public const string NAME = "Game Fish";
	public const string PURPOSE = "Making s&box game creation so much easier.";

	public static string GameIdent => Package.TryParseIdent( Game.Ident, out var info ) ? info.package : null;
}
