namespace GameFish;

/// <summary>
/// A module for a <see cref="PawnController"/>.
/// </summary>
[Icon( "directions_run" )]
public abstract partial class ControllerModule : Module
{
	public override bool IsParent( ModuleEntity comp )
		=> comp is PawnController;

	public PawnController Controller => Parent as PawnController;

	public Rigidbody Rigidbody => Controller?.Rigidbody;
	public Vector3 Gravity => Controller?.Gravity ?? default;

	public Pawn Pawn => Controller?.Pawn;
	public PawnView View => Pawn?.View;
}
