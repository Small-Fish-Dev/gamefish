namespace GameFish;

partial class SimpleActor
{
	protected override void OnTarget( Pawn target )
	{
		if ( MentalState is Mind.Fighting )
			return;

		if ( IsTargetVisible( target ) )
			MentalState = Mind.Fighting;
		else
			MentalState = Mind.Alert;
	}

	protected override void OnTargetDetected( Pawn target, in Vector3 at )
	{
		base.OnTargetDetected( target, at );

		OnMindDetectTarget( target, in at );
	}

	public override Vector3? GetLastKnownTargetOrigin( Pawn target = null )
	{
		// TODO: Patrols. Walk around initial area by default.
		if ( MentalState is Mind.Idle or Mind.Asleep )
			return null;

		if ( !IsTargeting() )
			return null;

		target ??= Target;

		// TEMP: Prevents them just standing there if you ring around the rosie.
		if ( MentalState is Mind.Fighting )
			return GetTargetOrigin( target );

		if ( IsTargetVisible( target ) )
			return GetTargetOrigin( target );

		return LastKnownTargetPosition;
	}

	public override bool IsAiming()
	{
		if ( MentalState is Mind.Fighting )
			return false;

		return base.IsAiming();
	}
}
