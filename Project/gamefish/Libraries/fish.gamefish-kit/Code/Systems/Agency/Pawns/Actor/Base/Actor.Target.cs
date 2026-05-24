using System.Text.Json.Serialization;

namespace GameFish;

partial class Actor
{
	/// <summary>
	/// Our primary active target. Probably an enemy.
	/// </summary>
	[Title( "Target" )]
	[Property, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( ACTOR ), Group( COMBAT )]
	protected Pawn InspectorTarget
	{
		get => Target;
		set => Target = value;
	}

	/// <summary>
	/// The current primary target(if any).
	/// <br /> <br />
	/// <b> NOTE: </b> Use <see cref="IsTargeting"/> to check if we are targeting at all.
	/// <br /> <br />
	/// <b> NOTE: </b> Use <see cref="HasTarget"/> to check if a specific pawn is targeted.
	/// </summary>
	[Sync]
	public Pawn Target
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

	protected Pawn _target;

	protected virtual void OnSetTarget( Pawn newTarget, Pawn oldTarget = null )
	{
		if ( newTarget.IsValid() )
			OnTarget( newTarget );

		// Might exist but have been destroyed.
		if ( oldTarget is not null )
			OnTargetLost( oldTarget );
	}

	/// <summary>
	/// The target was set.
	/// </summary>
	/// <param name="target"> The guy we don't like. </param>
	protected virtual void OnTarget( Pawn target )
	{
	}

	protected virtual void OnTargetLost( Pawn target )
	{
	}

	/// <summary>
	/// Start targeting this pawn if possible.
	/// </summary>
	/// <returns> If the target was set. </returns>
	public virtual bool TryTarget( Pawn pawn )
	{
		if ( IsProxy )
			return false;

		if ( !IsTargetValid( pawn ) )
			return false;

		Target = pawn;
		return true;
	}

	/// <summary>
	/// A quick check to see if this is capable of and has <b>ANY</b> active target.
	/// </summary>
	/// <returns> If we have an active valid target. </returns>
	public virtual bool IsTargeting()
	{
		if ( !GameObject.IsValid() )
			return false;

		if ( !IsAlive )
			return false;

		return IsTargetValid( Target );
	}

	/// <summary>
	/// A quick check to see if we are capable of and actively targeting that pawn.
	/// </summary>
	/// <returns> If that pawn is a target of ours. </returns>
	public virtual bool HasTarget( Pawn target )
	{
		if ( !IsTargeting() )
			return false;

		return Target == target;
	}

	/// <summary>
	/// A quick check to see if a pawn is ever allowed to be targeted.
	/// </summary>
	/// <returns> If the specified pawn is a valid target. </returns>
	public virtual bool IsTargetValid( Pawn pawn )
	{
		if ( !pawn.IsValid() || !pawn.GameObject.IsValid() )
			return false;

		if ( !pawn.IsAlive )
			return false;

		return true;
	}

	/// <returns> The world position of where we think a pawn is standing. </returns>
	public virtual Vector3? GetPawnOrigin( Pawn pawn )
	{
		if ( !pawn.IsValid() )
			return null;

		return pawn.Center;
	}

	/// <returns> The world position of where we think the target is standing. </returns>
	public virtual Vector3? GetTargetOrigin( Pawn target = null )
	{
		target ??= Target;

		return GetPawnOrigin( target );
	}

	/// <returns> The accurate last known/seen target position. </returns>
	public virtual Vector3? GetLastKnownTargetOrigin( Pawn target = null )
	{
		if ( !IsTargeting() )
			return null;

		target ??= Target;

		// If they're visible then just get their position.
		if ( IsTargetVisible( target ) )
			return GetTargetOrigin( target );

		return LastKnownTargetPosition;
	}
}
