namespace GameFish;

/// <summary>
/// Allows you to block input(where this is checked) from anywhere in the scene.
/// </summary>
public partial interface IFocus
{
	/// <summary>
	/// If true: prevents looking around. <br />
	/// </summary>
	public virtual bool HasAimingFocus => false;

	/// <summary>
	/// If true: prevents analogue(such as WASD) movement. <br />
	/// </summary>
	public virtual bool HasMovingFocus => false;

	/// <summary>
	/// If true: prevents jumping, crouching, shooting etc. <br />
	/// </summary>
	public virtual bool HasActionFocus => false;

	/// <summary>
	/// All focus interfaces in the main, actively played game scene.
	/// </summary>
	public static IEnumerable<IFocus> All => Game.ActiveScene?.GetAll<IFocus>();

	public static bool Aiming => All?.Any( f => f?.HasAimingFocus is true ) is true;
	public static bool Moving => All?.Any( f => f?.HasMovingFocus is true ) is true;
	public static bool Action => All?.Any( f => f?.HasActionFocus is true ) is true;
}
