namespace GameFish;

partial class ActorMind
{
	/// <summary> The pawn we're aiming for. </summary>
	[Sync] new public BasePawn Target { get; set; }

	/// <summary> When the target was last directly within our vision. </summary>
	[Sync] new public TimeSince? LastSeenTarget { get; set; }

	/// <summary> The origin of the target when they were last seen. </summary>
	[Sync] new public Vector3? LastKnownTargetPosition { get; set; }

	/// <summary> Where we're trying to look at. </summary>
	[Sync] new public Vector3? TargetAimPosition { get; set; }

	/// <summary>
	/// Forces the current target.
	/// </summary>
	public void SetTarget( BasePawn target, bool isVisible = false, bool knowPosition = false, bool isFighting = true )
	{
		if ( Target.IsValid() && Target == target )
			return;

		Target = target;

		var seenPos = isVisible
			? GetTargetAimPosition( target )
			: IsPawnVisible( target, out var hitPos ) ? hitPos : null;

		if ( isVisible || seenPos is not null )
			OnTargetVisible( target, seenPos ?? target.Center );

		if ( IsTargetVisible() || knowPosition )
			LastKnownTargetPosition = GetTargetPosition( target );
	}

	/// <summary>
	/// Does a vision check for the specified pawn. Might be expensive, depending.
	/// </summary>
	/// <param name="pawn"> Them. </param>
	/// <param name="aimPos"> The position we could see them at. </param>
	/// <returns> If we could see the pawn. </returns>
	public virtual bool IsPawnVisible( BasePawn pawn, out Vector3? aimPos )
	{
		if ( Detection?.IsPawnVisible( pawn, out aimPos ) is true )
			return true;

		aimPos = null;
		return false;
	}

	/// <summary>
	/// Actively looking at someone we just don't like.
	/// </summary>
	public virtual void OnTargetVisible( BasePawn enemy, in Vector3? at = null )
	{
		if ( !enemy.IsValid() )
			return;

		LastSeenTarget = 0f;

		TargetAimPosition = ActiveEquip?.PrimaryFunction?.GetTargetAimPosition( enemy, at ) ?? at;
		LastKnownTargetPosition = GetTargetPosition( enemy );

		State?.OnTargetVisible( enemy, at );
	}

	/// <returns> The world position of where we think the target was. </returns>
	protected virtual Vector3? GetTargetPosition( BasePawn target = null )
	{
		target ??= Target;

		if ( !target.IsValid() )
			return null;

		return target.Center;
	}

	protected virtual Vector3? GetTargetAimPosition( BasePawn target = null )
	{
		target ??= Target;

		if ( !target.IsValid() )
			return null;

		// Allow equipment to affect our aim(such as shooting a projectile ahead).
		if ( ActiveEquip is var equip && equip.IsValid() )
			if ( equip.GetTargetAimPosition( target, target.Center ) is Vector3 equipAim )
				return equipAim;

		// Default to the approximate center of the target.
		return target.Center;
	}
}
