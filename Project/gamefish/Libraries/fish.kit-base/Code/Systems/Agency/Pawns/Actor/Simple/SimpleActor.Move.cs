using System;
using Sandbox.Navigation;

namespace GameFish;

partial class SimpleActor
{
	/// <summary>
	/// The place we're immediately trying to get to.
	/// </summary>
	[Sync]
	public Vector3? Destination
	{
		get => _destination;
		set
		{
			if ( _destination == value )
				return;

			_destination = value;
			OnSetDestination( in value );
		}
	}

	protected Vector3? _destination = null;

	protected override void UpdateNavigation( in float deltaTime )
	{
		base.UpdateNavigation( deltaTime );

		if ( GetDestination() is Vector3 dest )
			TryMoveTo( dest );
	}

	/// <returns> The exact goal position(or null). </returns>
	protected virtual Vector3? GetDestination()
	{
		if ( !Target.IsValid() )
			return null;

		// Shrimply chase the target by default.
		if ( TargetVisible )
			return GetTargetOrigin( Target );

		return LastKnownTargetPosition;
	}

	/// <summary>
	/// Called when <see cref="Destination"/> has been set.
	/// </summary>
	protected virtual void OnSetDestination( in Vector3? dest )
	{
		if ( IsProxy )
			return;

		if ( !dest.HasValue )
			StopMoving();
	}

	protected override void Move( in float deltaTime, in bool isFixedUpdate )
	{
		if ( !Controller.IsValid() )
			return;

		Vector3 wishVel = GetWishVelocity();
		Controller.TryMove( in deltaTime, in isFixedUpdate, in wishVel );
	}

	public override Vector3 GetWishVelocity()
	{
		if ( !IsAlive || Destination is null )
			return Vector3.Zero;

		var dest = Destination.Value;
		var moveDir = WorldPosition.Direction( dest );
		var moveSpeed = GetWishSpeed();

		return moveDir * moveSpeed;
	}

	public virtual void StopMoving()
	{
		Destination = null;
		SetWishVelocity( Vector3.Zero );
	}

	/// <summary>
	/// Attempts to direct this actor towards a position.
	/// </summary>
	/// <param name="to"> The place we're trying to get to. </param>
	/// <param name="haltUponFail"> Stop moving if one couldn't be found? </param>
	/// <returns> If we could move towards that point. </returns>
	public virtual bool TryMoveTo( in Vector3 to, bool haltUponFail = false )
	{
		if ( GetNearestPoint( to ) is Vector3 dest )
		{
			Destination = dest;
			return true;
		}

		if ( haltUponFail )
			StopMoving();

		return false;
	}

	protected virtual NavMeshPath? CalculatePath( in Vector3 from, in Vector3 to )
	{
		if ( from == to )
			return null;

		try
		{
			return Scene?.NavMesh?.CalculatePath( new CalculatePathRequest() { Start = from, Target = to } );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( CalculatePath )} exception: " + e );
			return null;
		}
	}

	/// <returns> The position we should move towards to reach the destination. </returns>
	public virtual Vector3? GetNearestPoint( in Vector3 to )
	{
		if ( !Scene.IsValid() || Scene.NavMesh is null || !Scene.NavMesh.IsEnabled )
			return null;

		// If we are basically there then don't bother.
		var groundPos = WorldPosition;

		if ( groundPos.AlmostEqual( to, 10f ) )
			return null;

		// Ask the nav mesh where to go.
		var path = CalculatePath( groundPos, to );

		return path?.Points?.ElementAtOrDefault( 1 ).Position;
	}
}
