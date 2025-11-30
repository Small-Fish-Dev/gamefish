namespace GameFish;

partial class ActorMind
{
	public override Vector3 Center => Actor?.Center ?? base.Center;

	/// <summary>
	/// Called just before movement logic is performed to determine the wish velocity.
	/// </summary>
	/// <param name="deltaTime"></param>
	/// <param name="speed"> The target movement speed. </param>
	/// <param name="wishVel"> The final movement velocity. </param>
	public virtual void PreMove( in float deltaTime, in float speed, ref Vector3 wishVel )
	{
		var dest = GetDestination( ActiveEquip );

		if ( dest.HasValue && Controller.IsValid() )
			wishVel = Controller.GetWishVelocity( Center.Direction( dest.Value ) );

		State?.PreMove( in deltaTime, dest, in speed, ref wishVel );
	}

	/// <summary>
	/// Lets the mind decide where it wants to travel.
	/// </summary>
	/// <param name="equip"> The equipment we'd be moving with(or our active one). </param>
	/// <returns> The place we should be moving to(if any). </returns>
	public virtual Vector3? GetDestination( Equipment equip = null )
	{
		var moveDest = State?.GetDestination( equip );

		if ( moveDest.HasValue )
			return moveDest.Value;

		// As a fallback just get within our ideal equipment distance.
		if ( equip.IsValid() && equip.IdealRange is FloatRange range )
		{
			if ( (TargetAimPosition ?? LastKnownTargetPosition) is not Vector3 targetPos )
				return null;

			var targetDist = EyePosition.Distance( targetPos );
			return GetRangeDestination( in targetDist, in range );
		}

		return null;
	}

	/// <returns> If we should move to a position more within range. </returns>
	public virtual Vector3? GetRangeDestination( in float targetDist, in FloatRange range )
	{
		if ( LastKnownTargetPosition is not Vector3 targetPos )
			return null;

		// Try to stay within the range of our best weapon.
		var aimPos = EyePosition;
		var dirFromTarget = targetPos.Direction( aimPos );
		var distFromTarget = targetDist;

		if ( distFromTarget > range.Max )
			return Navigation?.GetClosestPoint( targetPos + (dirFromTarget * range.Max) ) ?? default;
		else if ( distFromTarget < range.Min )
			return Navigation?.GetClosestPoint( targetPos + (dirFromTarget * range.Min) ) ?? default;

		return default;
	}
}
