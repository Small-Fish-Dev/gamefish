namespace GameFish;

/// <summary>
/// A <see cref="PawnView"/> that can look through a <see cref="SpectatorPawn"/> target's eyes.
/// </summary>
public partial class SpectatorView : PawnView
{
	public override BasePawn Pawn => ModuleParent is SpectatorPawn spec
		? spec.Spectating ?? base.Pawn
		: base.Pawn;
}
