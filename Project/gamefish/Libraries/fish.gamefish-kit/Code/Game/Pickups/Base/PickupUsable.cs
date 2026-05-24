namespace GameFish;

/// <summary>
/// Lets players interact with pickups.
/// </summary>
[Title( "Pickup Usable" )]
public partial class PickupUsable : UsableModule
{
	/// <summary>
	/// The parent component implementing <see cref="Pickup"/> to use.
	/// </summary>
	[Property]
	[InputAction]
	[Feature( USE ), Group( LOGIC )]
	public virtual Pickup Pickup
	{
		get => _target ??= Parent as Pickup;
		set => _target = value;
	}

	protected Pickup _target;

	public override bool IsParent( ModuleEntity comp )
		=> comp is Pickup;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// DebugInput();
	}

	public override bool IsUsable( Pawn pawn )
	{
		if ( !Pickup.IsValid() )
			return false;

		if ( pawn is not Player pl )
			return false;

		if ( !base.IsUsable( pl ) )
			return false;

		return Pickup.CanPickup( pl );
	}

	protected override void OnUse( Pawn pawn )
	{
		base.OnUse( pawn );

		if ( Pickup.IsValid() )
			Pickup.TryPickup( pawn as Player );
	}
}
