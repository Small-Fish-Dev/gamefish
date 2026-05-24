namespace GameFish;

/// <summary>
/// A non-modular autonomous pawn. Capable of detection, navigation and combat.
/// <br /> <br />
/// <b> NOTE: </b> You can use <see cref="ModularActor"/>
/// for a more powerful, customizable NPC base.
/// </summary>
public partial class SimpleActor : Actor
{
	protected override void OnStart()
	{
		base.OnStart();

		OnMindStart();
	}

	protected override void Think( in float deltaTime, in bool isFixedUpdate )
	{
		base.Think( in deltaTime, in isFixedUpdate );

		UpdateMentalState( in deltaTime );

		UpdateNavigation( in deltaTime );

		UpdateAiming( in deltaTime );
		UpdateAttacking( in deltaTime );
	}

	public override bool CanAttack()
	{
		if ( !IsFighting )
			return false;

		return base.CanAttack();
	}
}
