namespace GameFish;

public partial class IdleCombatState : CombatState
{
	public override CombatType Type { get; } = CombatType.Idle;

	public override void Think( in float deltaTime, in bool isFixedUpdate )
	{
		base.Think( deltaTime, isFixedUpdate );

		// Nothing.. adds up, right?
		// TODO: Maybe patrol your home area, interact with environment.
	}

	public override bool TryIdle()
		=> true;
}
