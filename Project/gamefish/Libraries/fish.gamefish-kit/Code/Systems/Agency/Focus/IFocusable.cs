namespace GameFish;

/// <summary>
/// Allows pawns to direct their inputs into this.
/// May optionally affect the pawn's view.
/// </summary>
public interface IFocusable
{
	/// <summary>
	/// If defined: try to look towards this point.
	/// </summary>
	public Vector3? FocusViewTarget { get; }

	/// <summary>
	/// If defined: move the camera here.
	/// </summary>
	public Offset? FocusViewOrigin { get; }

	public bool TryEnterFocus( Pawn pawn );
	public bool TryLeaveFocus( Pawn pawn );
}
