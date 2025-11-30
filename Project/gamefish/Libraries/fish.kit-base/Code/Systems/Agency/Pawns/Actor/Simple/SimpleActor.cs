namespace GameFish;

/// <summary>
/// A non-modular autonomous pawn. Capable of detection, navigation and combat.
/// <br />
/// <b> NOTE: </b> You can use <see cref="ModularActor"/>
/// for a more powerful, customizable NPC base.
/// </summary>
public abstract partial class SimpleActor : Actor
{
	protected override void OnStart()
	{
		base.OnStart();

		// Auto-wake.
		if ( MindState is MentalState.Asleep )
			MindState = MentalState.Idle;
	}

	protected override void Think( in float deltaTime, in bool isFixedUpdate )
	{
		UpdatePerception( in deltaTime );
		UpdateMentalState( in deltaTime );
		UpdateNavigation( in deltaTime );
		UpdateAttacking( in deltaTime );
	}
}
