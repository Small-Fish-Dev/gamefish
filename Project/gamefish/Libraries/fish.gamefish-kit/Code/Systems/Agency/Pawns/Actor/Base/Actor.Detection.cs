namespace GameFish;

partial class Actor
{
	/// <summary>
	/// The most recent time any target was detected(if ever).
	/// </summary>
	[Sync]
	public TimeSince? SinceTargetDetected { get; set; }

	/// <summary>
	/// Where the primary target was last seen(if anywhere).
	/// </summary>
	[Sync]
	public Vector3? LastKnownTargetPosition { get; set; }

	/// <summary>
	/// Look/listen out for things of interest.
	/// </summary>
	protected virtual void UpdateDetection( in float deltaTime )
	{
		UpdateVision( in deltaTime );
	}

	/// <summary>
	/// We have just seen, heard or otherwise detected an enemy. <br />
	/// Used to know the location of and prioritize between targets and such.
	/// </summary>
	/// <param name="target"> The enemy pawn that we've detected. </param>
	/// <param name="at"> Where we detected them at. </param>
	protected virtual void OnTargetDetected( Pawn target, in Vector3 at )
	{
		if ( !HasTarget( target ) )
		{
			// If we have a current valid target then see if we can switch.
			if ( Target.IsValid() )
			{
				// Compare the distance to see if the new target is closer.
				if ( GetTargetOrigin( Target ) is Vector3 oldTargetPos
					&& GetTargetOrigin( target ) is Vector3 newTargetPos )
				{
					var oldTargetDist = EyePosition.Distance( oldTargetPos );
					var newTargetDist = EyePosition.Distance( newTargetPos );

					// If our current target is closer then ignore them.
					if ( oldTargetDist <= newTargetDist )
						return;
				}
			}

			// If we can't change target then uhh don't?
			if ( !TryTarget( target ) )
				return;
		}

		SinceTargetDetected = 0f;
		LastKnownTargetPosition = GetTargetOrigin( target ) ?? at;
	}
}
