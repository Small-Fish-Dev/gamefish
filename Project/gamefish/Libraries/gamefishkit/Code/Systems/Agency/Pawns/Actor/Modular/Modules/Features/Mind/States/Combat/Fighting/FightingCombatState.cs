namespace GameFish;

public partial class FightingCombatState : CombatState
{
	/// <summary> Lower to alert after losing line of sight for this long. </summary>
	[Property, Feature( COMBAT )]
	public virtual float Patience { get; set; } = 10f;

	public override CombatType Type { get; } = CombatType.Fighting;

	public override void Think( in float deltaTime, in bool isFixedUpdate )
	{
		base.Think( deltaTime, isFixedUpdate );

		// Stay full alert if target has been seen very recently.
		if ( !LastSeenTarget.HasValue || LastSeenTarget > Patience )
			TryEnd();
	}

	public override bool TryFighting()
		=> true;

	public override bool TryEnd()
	{
		if ( TryAlert() )
			return true;

		return base.TryEnd();
	}
}
