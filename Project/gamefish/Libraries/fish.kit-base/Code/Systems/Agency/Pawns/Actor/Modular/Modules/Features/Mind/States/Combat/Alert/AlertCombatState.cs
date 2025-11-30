namespace GameFish;

public partial class AlertCombatState : CombatState
{
	/// <summary> Idle after losing line of sight for this long. </summary>
	[Property, Feature( COMBAT )]
	public virtual float Patience { get; set; } = 30f;

	public override CombatType Type { get; } = CombatType.Alert;

	public override void Think( in float deltaTime, in bool isFixedUpdate )
	{
		base.Think( deltaTime, isFixedUpdate );

		// Stay full alert if target has been seen very recently.
		if ( !LastSeenTarget.HasValue || LastSeenTarget > Patience )
			TryEnd();
	}

	public override bool TryAlert()
		=> true;

	public override bool TryEnd()
	{
		if ( TryAlert() )
			return true;

		return base.TryEnd();
	}
}
