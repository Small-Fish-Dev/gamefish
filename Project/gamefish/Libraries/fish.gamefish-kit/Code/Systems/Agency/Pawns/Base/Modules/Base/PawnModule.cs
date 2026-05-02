using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// A module for a <see cref="Pawn"/>.
/// </summary>
[Icon( "person" )]
public abstract partial class PawnModule : Module
{
	protected const int PAWN_ORDER = DEFAULT_ORDER - 1000;

	public override bool IsParent( ModuleEntity comp )
		=> comp is Pawn;

	public Pawn Pawn => Parent as Pawn;
}
