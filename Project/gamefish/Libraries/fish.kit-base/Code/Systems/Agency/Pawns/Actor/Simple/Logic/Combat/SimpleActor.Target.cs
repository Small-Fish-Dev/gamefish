using System.Text.Json.Serialization;

namespace GameFish;

partial class SimpleActor
{
	[Title( "Target" )]
	[Property, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( ACTOR ), Group( COMBAT )]
	protected BasePawn InspectorTarget
	{
		get => Target;
		set => Target = value;
	}

	/// <summary>
	/// Them.
	/// </summary>
	[Sync]
	public BasePawn Target
	{
		get => _target.IsValid() ? _target : null;
		protected set
		{
			if ( _target == value )
				return;

			var old = _target;
			_target = value;

			OnSetTarget( newTarget: value, oldTarget: old );
		}
	}

	protected BasePawn _target;

	/// <summary>
	/// Is our target visible(since the previous tick)?
	/// </summary>
	[Sync] public bool TargetVisible { get; set; }

	/// <summary> When the target was last directly within our vision. </summary>
	[Sync] public TimeSince? LastSeenTarget { get; set; }

	/// <summary> The origin of the target when they were last seen. </summary>
	[Sync] public Vector3? LastKnownTargetPosition { get; set; }

	/// <summary> Where we're trying to look at. </summary>
	[Sync] public Vector3? TargetAimPosition { get; set; }

	/// <returns> If the specified pawn was a valid target. </returns>
	public virtual bool CanTarget( BasePawn pawn )
	{
		if ( !this.IsValid() || !IsAlive )
			return false;

		if ( !pawn.IsValid() || !pawn.IsAlive )
			return false;

		return IsEnemy( pawn );
	}

	/// <summary>
	/// Sets our current target if it's a valid one.
	/// </summary>
	/// <returns> If the target was set. </returns>
	public virtual bool TrySetTarget( BasePawn pawn )
	{
		if ( !CanTarget( pawn ) )
			return false;

		Target = pawn;
		return true;
	}

	public virtual void LoseTarget()
	{
	}

	protected virtual void OnSetTarget( BasePawn newTarget, BasePawn oldTarget = null )
	{
		if ( !newTarget.IsValid() )
			Destination = null;
	}

	/// <returns> The world position of where we think the target is. </returns>
	public virtual Vector3? GetTargetOrigin( BasePawn target = null )
	{
		target ??= Target;

		if ( !target.IsValid() )
			return null;

		return target.WorldPosition;
	}

	/// <returns> Where we should aim at this target(such as ahead). </returns>
	public virtual Vector3? GetTargetAimPosition( BasePawn target = null, Vector3? at = null )
	{
		if ( !AllowAiming )
			return null;

		target ??= Target;

		if ( !target.IsValid() )
			return null;

		// Allow equipment to affect our aim(such as shooting a projectile ahead).
		if ( ActiveEquip is var equip && equip.IsValid() )
			if ( equip.GetTargetAimPosition( target, at ?? target.Center ) is Vector3 equipAim )
				return equipAim;

		// Default to the approximate center of the target.
		return at ?? target.Center;
	}

	/// <returns> The accurate last seen target position. </returns>
	public virtual Vector3? GetLastTargetPosition()
	{
		if ( TargetVisible && Target.IsValid() )
			return GetTargetOrigin( Target ) ?? LastKnownTargetPosition;

		return LastKnownTargetPosition;
	}
}
