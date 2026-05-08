namespace GameFish;

public abstract class PlayerModule : PawnModule
{
	protected const int PLAYER_ORDER = PAWN_ORDER - 1000;

	public override bool IsParent( ModuleEntity comp )
		=> comp is Player;

	public Player Player => Parent as Player;

	public PawnController Controller => Pawn?.Controller;
	public ControllerPhysics Physics => Controller?.Physics;
}
