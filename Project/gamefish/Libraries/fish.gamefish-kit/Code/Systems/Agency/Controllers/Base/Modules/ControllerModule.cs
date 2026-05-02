namespace GameFish;

/// <summary>
/// A module for a <see cref="PawnController"/>.
/// </summary>
[Icon( "directions_run" )]
public abstract partial class ControllerModule : Module
{
	public override bool IsParent( ModuleEntity comp )
		=> comp is PawnController;
}
