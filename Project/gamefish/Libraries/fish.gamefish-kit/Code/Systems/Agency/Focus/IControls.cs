namespace GameFish;

/// <summary>
/// Allows you to block input(where this is checked) from anywhere in the scene.
/// </summary>
public partial interface IControls
{
	/// <summary>
	/// If true: prevents looking around.
	/// </summary>
	public bool HasAimingFocus { get; }

	/// <summary>
	/// If true: ignore mouse wheel scrolling.
	/// </summary>
	public bool HasScrollFocus { get; }

	/// <summary>
	/// If true: prevents analogue(such as WASD) movement.
	/// </summary>
	public bool HasMovingFocus { get; }

	/// <summary>
	/// If true: prevents jumping, crouching, shooting etc.
	/// </summary>
	public bool HasActionFocus { get; }

	/// <summary>
	/// All focus interfaces in the main, actively played game scene.
	/// </summary>
	public static IEnumerable<IControls> All => Game.ActiveScene?.GetAll<IControls>();

	public static bool BlockAiming => All?.Any( f => f?.HasAimingFocus is true ) is true;
	public static bool BlockScroll => All?.Any( f => f?.HasScrollFocus is true ) is true;
	public static bool BlockMoving => All?.Any( f => f?.HasMovingFocus is true ) is true;
	public static bool BlockActions => All?.Any( f => f?.HasActionFocus is true ) is true;

	public static Vector2 Scroll => Input.MouseWheel == default || BlockScroll ? default : Input.MouseWheel;
}
