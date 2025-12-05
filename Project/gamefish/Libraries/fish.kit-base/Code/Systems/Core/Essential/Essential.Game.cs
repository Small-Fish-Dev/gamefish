namespace GameFish;

partial class Essential
{
	protected const int GAME_ORDER = BOOT_ORDER + 100;
	protected const int GAME_DISPLAY_ORDER = GAME_ORDER - 1;

	/// <summary>
	/// What's your game called?
	/// </summary>
	[Property]
	[Title( "Name" )]
	[Feature( GAME ), Group( DISPLAY ), Order( GAME_DISPLAY_ORDER )]
	protected virtual string GameName { get; set; }

	/// <summary>
	/// What version is your game at?
	/// </summary>
	[Property]
	[Title( "Version" )]
	[Feature( GAME ), Group( DISPLAY ), Order( GAME_DISPLAY_ORDER )]
	protected virtual string GameVersion { get; set; } = "0.1a";
}
