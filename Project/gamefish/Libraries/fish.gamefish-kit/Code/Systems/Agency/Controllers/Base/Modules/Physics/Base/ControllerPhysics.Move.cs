using System.Numerics;

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
	/// The limit of attempts to get us unstuck before giving up.
	/// </summary>
	[Property]
	[Range( 1, 64, clamped: false )]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public int MaxUnstuckTries { get; set; } = 32;

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
		var trBase = Trace( skin: 0f );
		var trSkin = Trace( skin: -SkinWidth );

		var dir = tFrom.Position.Direction( in to );
		var dist = tFrom.Position.Distance( in to );

		var tStart = tFrom.WithOffset( TraceOffset );
		Offset delta = tStart.ToLocal( tFrom );

		var move = new ProjectedMovement( in trBase, in trSkin, in tStart, in delta, in dir, in dist )
		{
			Velocity = Velocity,
			IsGrounded = IsGrounded && GroundingEnabled,
			GroundNormal = GroundNormal,
		};

		Run( move );
	}

	/// <summary>
	/// Perform our movement/collision algorithm according to parameters.
	/// </summary>
	protected virtual void Run( ProjectedMovement move )
	{
		for ( var i = 0; i < MaxIterations; i++ )
		{
			if ( move.Distance <= 0f )
				break;

			Project( move );
		}

		if ( move.IsGrounded )
			TryStickToGround( move );

		End( move );
	}

	/// <summary>
	/// Finalize movement projection.
	/// </summary>
	protected virtual void End( ProjectedMovement move )
	{
		if ( !IsStuck )
			LastVelocity = Velocity;

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
		if ( move is null )
			return;

		if ( move.Direction == default || move.Distance <= 0f )
			return;

		var trMove = move.Trace( skin: false ).Run();

		if ( !trMove.Hit )
		{
			OnFreeMove( in trMove, move );

			if ( move.Distance <= 0 )
				return;
		}

		// Allow for custom unstuck behavior.
		if ( trMove.StartedSolid )
			OnStuck( in trMove, move );

		OnCollide( in trMove, move );
	}

	/// <summary>
	/// Reponds to a completely unobstructed movement projection.
	/// </summary>
	protected virtual void OnFreeMove( in SceneTraceResult trMove, ProjectedMovement move )
	{
		move.Position = trMove.EndPosition;
		move.Distance = 0f;

		// Look for ground again in case we're floating.
		if ( move.IsGrounded )
			move.IsGrounded = IsGround( GroundTrace( move ) );
	}

	/// <summary>
	/// Responds to movement projection being stuck(possibly from the start).
	/// </summary>
	protected virtual void OnStuck( in SceneTraceResult trStuck, ProjectedMovement move )
	{
		move.IsStuck = true;

		if ( TryUnstuck( in trStuck, move ) )
		{
			move.IsStuck = false;
			return;
		}

		// Don't act like we're grounded if stuck in something.
		move.IsGrounded = false;
	}

	/// <summary>
	/// Responds to a movement projection hitting something.
	/// </summary>
	protected virtual void OnCollide( in SceneTraceResult trHit, ProjectedMovement move )
	{
		move.Distance -= trHit.Distance;

		if ( !TrySnapTo( in trHit, move ) )
			return;

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

		ClipVelocity( trGround.Normal, move );
	}

	protected bool TryStickToGround( ProjectedMovement move )
		=> TryStickToGround( GroundTrace( move ), move );

	protected virtual bool TryStickToGround( in SceneTraceResult trGround, ProjectedMovement move )
	{
		var hitGround = trGround.Hit && IsGround( in trGround.Normal );

		if ( !hitGround )
			return false;

		if ( !TrySnapTo( trGround, move ) )
			return false;

		OnGrounded( in trGround, move );
		return true;
	}

	protected virtual bool TrySnapTo( in SceneTraceResult trMove, ProjectedMovement move )
	{
		var tDest = move.WithPosition( trMove.EndPosition );

		if ( IsEmpty( tDest, out _, skin: -SkinWidth ) )
		{
			move.Point = tDest;
			return true;
		}

		return false;
	}

	protected virtual bool TryUnstuck( in Transform tStuck, ProjectedMovement move )
	{
		if ( IsEmpty( in tStuck, out var trStuck, skin: 0f ) )
		{
			IsStuck = false;
			return true;
		}

		return TryUnstuck( in trStuck, move );
	}

	protected virtual bool TryUnstuck( in SceneTraceResult trStuck, ProjectedMovement move )
	{
		if ( trStuck.StartedSolid )
			move.IsStuck = true;

		if ( !move.IsStuck )
			return true;

		Vector3 vSkin;
		Transform tEmpty;

		if ( LastVelocity is Vector3 lastVel )
		{
			vSkin = -lastVel.Normal * SkinWidth;
			tEmpty = move.Project( vSkin );

			if ( IsEmpty( tEmpty, out _, skin: 0f ) )
			{
				move.Point = tEmpty;
				move.IsStuck = false;

				return true;
			}
		}

		vSkin = trStuck.Normal * SkinWidth;
		tEmpty = move.Project( vSkin );

		if ( IsEmpty( in tEmpty, out _, skin: 0f ) )
		{
			move.Point = tEmpty;
			move.IsStuck = false;

			return true;
		}

		return move.IsStuck;
	}

	/// <summary>
	/// Redirects momentum along the hit surface.
	/// </summary>
	/// <param name="normal"> The direction of the surface to slide along. </param>
	/// <param name="move"> The current movement projection. </param>
	protected virtual void Slide( in Vector3 normal, ProjectedMovement move )
	{
		if ( normal == default || move.Direction == default )
			return;

		var dot = move.Direction.Dot( normal );

		if ( dot == 0f )
		{
			move.Distance = 0f;
			move.Direction = default;

			move.Velocity = default;

			return;
		}

		// Reduce velocity.
		ClipVelocity( in normal, move );

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
