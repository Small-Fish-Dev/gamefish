using GameFish;

namespace GameFish;

/// <summary>
/// Lets players activate components that implement <see cref="IActivate"/>.
/// </summary>
[Title( "Logic Activation Usable" )]
public partial class ActivationUsable : UsableModule
{
	/// <summary>
	/// The parent component implementing <see cref="IActivate"/> to use.
	/// </summary>
	[Property]
	[InputAction]
	[Feature( USE ), Group( LOGIC )]
	public virtual IActivate Target
	{
		get => _target ??= Parent as IActivate;
		set => _target = value;
	}

	protected IActivate _target;

	public override bool IsParent( ModuleEntity comp )
		=> comp is IActivate;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// DebugInput();
	}

	public override bool IsUsable( Pawn pawn )
		=> base.IsUsable( pawn ) && Target?.CanActivate( pawn ) is true;

	protected override void OnUse( Pawn pl )
	{
		base.OnUse( pl );

		Target?.TryActivate( pl );
	}
}
