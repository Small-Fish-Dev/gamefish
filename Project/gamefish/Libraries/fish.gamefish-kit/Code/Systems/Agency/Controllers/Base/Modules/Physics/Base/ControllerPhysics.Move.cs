namespace GameFish;

partial class ControllerPhysics
{
	/// <summary>
	/// The limit of imaginary movement/collision steps that can be performed.
	/// </summary>
	[Property]
	[Range( 1, 32, clamped: false )]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public int MaxIterations { get; set; } = 8;

	/// <summary>
	/// Allow projecting momentum along surfaces?
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public bool SlidingEnabled { get; set; } = true;

	/// <summary>
	/// Moves using a relative vector for the destination.
	/// Basically adds <paramref name="delta"/> to the current position.
	/// </summary>
	public void MoveDelta( in Vector3 delta )
		=> MoveTo( Origin.Position + delta );

	/// <summary>
	/// Moves from the current position towards the destination.
	/// </summary>
	public void MoveTo( in Vector3 to )
		=> Move( Origin.Position, in to );

	/// <summary>
	/// Moves from one position to another.
	/// </summary>
	public void Move( in Vector3 from, in Vector3 to )
		=> Move( Origin.WithPosition( from ), in to );

	/// <summary>
	/// Moves from a transform to a position.
	/// </summary>
	public virtual void Move( in Transform tFrom, in Vector3 to )
	{
		var move = new ProjectedMovement( this, in tFrom, in to );

		Run( move );
	}

	/// <summary>
	/// Perform our movement/collision algorithm according to parameters.
	/// </summary>
	protected virtual void Run( ProjectedMovement move )
	{
		if ( move is null )
			return;

		for ( var i = 0; i < MaxIterations; i++ )
		{
			Project( move );

			if ( move.Distance <= 0f )
				break;

			if ( move.Direction == default )
				break;
		}

		End( move );
	}

	/// <summary>
	/// Finalize movement projection.
	/// </summary>
	protected virtual void End( ProjectedMovement move )
	{
		if ( !move.IsStuck )
		{
			// Always detect ground after moving in case we're airborne.
			move.IsGrounded = IsGround( GroundTrace( move ) );

			if ( move.IsGrounded )
				TryStickToGround( move );

			// Remember velocity for unstuck.
			LastVelocity = move.Velocity;
		}

		if ( !move.IsGrounded )
			ClearGround( move );

		Apply( move );
	}

	/// <summary>
	/// Applies projection results such as position/rotation.
	/// </summary>
	protected virtual void Apply( ProjectedMovement move )
	{
		IsStuck = move.IsStuck;

		Velocity = move.Velocity;

		IsGrounded = move.IsGrounded && GroundingEnabled;
		GroundNormal = move.IsGrounded ? move.GroundNormal : -Gravity.Normal;

		GroundObject = move.GroundObject;
		GroundCollider = move.GroundCollider;
		GroundBody = move.GroundBody;

		if ( !Pawn.IsValid() )
			return;

		var tEnd = move.Point.ToWorld( move.Delta );

		if ( ITransform.IsValid( in tEnd ) )
			Pawn.WorldTransform = tEnd;
	}

	/// <summary>
	/// Move our hypothetical position/rotation and resolve collisions.
	/// </summary>
	protected virtual void Project( ProjectedMovement move )
	{
		var trMove = move.ProjectedTrace( skin: false ).Run();

		if ( trMove.StartedSolid )
		{
			// Allow for custom unstuck behavior.
			OnStuck( in trMove, move );
		}
		else if ( trMove.Hit )
		{
			// Hit something.
			OnCollide( in trMove, move );
		}
		else
		{
			// Moving without obstruction.
			OnFreeMove( in trMove, move );

			if ( move.Distance <= 0 )
				return;
		}
	}

	/// <summary>
	/// Reponds to a completely unobstructed movement projection.
	/// </summary>
	protected virtual void OnFreeMove( in SceneTraceResult trMove, ProjectedMovement move )
	{
		move.IsStuck = false;

		move.Position = trMove.EndPosition;
		move.Distance = 0f;
	}

	/// <summary>
	/// Responds to movement projection being stuck(possibly from the start).
	/// </summary>
	protected virtual void OnStuck( in SceneTraceResult trStuck, ProjectedMovement move )
	{
		move.IsStuck = true;

		if ( TryUnstuck( in trStuck, move ) )
			return;

		// Don't act like we're grounded if stuck in something.
		ClearGround( move );
	}

	/// <summary>
	/// Responds to a movement projection hitting something.
	/// </summary>
	protected virtual void OnCollide( in SceneTraceResult trHit, ProjectedMovement move )
	{
		if ( move.IsStuck || trHit.StartedSolid )
			return;

		move.Position = trHit.EndPosition;
		move.Distance -= trHit.Distance;

		if ( IsGround( in trHit.Normal ) )
			OnGrounded( in trHit, move );

		if ( SlidingEnabled )
			Slide( in trHit.Normal, move );
	}

	/// <summary>
	/// Responds to movement projection touching ground.
	/// </summary>
	protected virtual void OnGrounded( in SceneTraceResult trGround, ProjectedMovement move )
	{
		if ( !trGround.Hit || trGround.StartedSolid )
			return;

		move.IsGrounded = true;
		move.GroundNormal = trGround.Normal;

		move.GroundObject = trGround.GameObject;
		move.GroundCollider = trGround.Collider;
		move.GroundBody = trGround.Collider?.Rigidbody;

		ClipVelocity( trGround.Normal, move );
	}

	protected bool TryStickToGround( ProjectedMovement move )
		=> TryStickToGround( GroundTrace( move ), move );

	protected virtual bool TryStickToGround( in SceneTraceResult trGround, ProjectedMovement move )
	{
		if ( !IsGround( in trGround ) )
			return false;

		if ( !TrySnapTo( trGround, move ) )
			return false;

		OnGrounded( in trGround, move );
		return true;
	}

	/// <summary>
	/// Redirects momentum along the hit surface.
	/// </summary>
	/// <param name="normal"> The direction of the surface to slide along. </param>
	/// <param name="move"> The current movement projection. </param>
	protected virtual void Slide( in Vector3 normal, ProjectedMovement move )
	{
		if ( normal == default )
			return;

		// Project velocity along the plane.
		ClipVelocity( in normal, move );

		if ( move.Direction == default )
			return;

		// Affect direction/distance.
		var vProject = Vector3.VectorPlaneProject( in move.Direction, in normal );

		move.Direction = vProject.Normal;
		move.Distance *= vProject.Length;
	}

	protected virtual void ClipVelocity( in Vector3 normal, ProjectedMovement move )
	{
		var vel = move.Velocity;

		if ( IsGround( normal ) )
		{
			var flatSpeed = vel.Horizontal( in normal ).Length;
			var planeVel = Vector3.VectorPlaneProject( in vel, in normal );

			move.Velocity = planeVel.Normal * flatSpeed;
		}
		else
		{
			move.Velocity = Vector3.VectorPlaneProject( in vel, in normal );
		}
	}
}
