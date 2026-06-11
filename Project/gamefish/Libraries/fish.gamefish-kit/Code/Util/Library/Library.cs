namespace GameFish;

/// <summary>
/// 🎮🐟 <br />
/// Provides utilities(such as extensions) to the Game Fish library.
/// </summary>
public static partial class Library
{
	public const string NAME = "Game Fish";
	public const string PURPOSE = "Making s&box game creation so much easier.";

	public const string GROUP_MISC = $"{NAME} (Misc)";
	public const string GROUP_DEBUG = $"{NAME} (Debug)";
	public const string GROUP_PAWNS = $"{NAME} (Pawns)";
	public const string GROUP_LOGIC = $"{NAME} (Logic)";

	public static string GameIdent => Package.TryParseIdent( Game.Ident, out var info ) ? info.package : null;
}
