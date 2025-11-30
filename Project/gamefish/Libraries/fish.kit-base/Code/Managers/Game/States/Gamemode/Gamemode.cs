namespace GameFish;

/// <summary>
/// A state module for <see cref="GameManager"/> that's focused on gameplay.
/// <br /> <br />
/// <b> TIP: </b> Inherit this to easily make your own gamemodes.
/// <br /> <br />
/// <b> TODO: </b> Round logic, objectives.
/// </summary>
public abstract partial class Gamemode : GameState
{
	/// <summary>
	/// The currently active gamemode(if any).
	/// </summary>
	public new static Gamemode Current => GameManager.Instance?.State as Gamemode;

	public override string Name { get; } = "Gamemode";
	public override string Description { get; } = "A gamemode.";
}
