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
		var dir = tFrom.Position.Direction( in to );
		var dist = tFrom.Position.Distance( in to );

		var tStart = tFrom.WithOffset( TraceOffset );
		Offset delta = tStart.ToLocal( tFrom );

		Run( new( in tStart, in delta, in dir, in dist, Velocity ) );
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
			StickToGround( move );

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
		Velocity = move.Velocity;

		IsStuck = move.IsStuck;

		IsGrounded = move.IsGrounded;
		GroundNormal = move.GroundNormal;

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

		// Are we stuck in something to begin with?
		IsStuck = !IsEmpty( in move.Point, out var trStuck, skin: 0f );

		// If we can't get unstuck then just give up.
		if ( IsStuck && !TryUnstuck( in trStuck, move ) )
			return;

		var tProj = move.Projected();
		var trBase = Trace( in move.Point, tProj.Position, skin: 0f ).Run();

		if ( !trBase.Hit )
		{
			move.Point = tProj;
			move.Distance = 0f;
			return;
		}

		OnProjectedCollision( in trBase, move );
	}

	/// <summary>
	/// Responds to hits from projected movement.
	/// </summary>
	protected virtual void OnProjectedCollision( in SceneTraceResult trHit, ProjectedMovement move )
	{
		var isGround = IsGround( in trHit.Normal );

		if ( isGround )
			move.IsGrounded = true;

		SnapTo( in trHit, move );

		move.Distance -= trHit.Distance;

		if ( move.Distance <= 0f )
			return;

		if ( SlidingEnabled )
			Slide( in trHit.Normal, move );
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
		move.IsStuck = trStuck.StartedSolid;

		if ( !move.IsStuck )
			return true;

		Vector3 vSkin;
		Transform tAddSkin;

		if ( LastVelocity is Vector3 lastVel )
		{
			vSkin = -lastVel.Normal * SkinWidth;
			tAddSkin = move.Project( vSkin );

			if ( IsEmpty( tAddSkin, out _, 0f ) )
			{
				move.Point = tAddSkin;
				move.IsStuck = false;

				return true;
			}
		}

		/*
		vSkin = trStuck.Normal * SkinWidth;
		tAddSkin = move.WithPosition( trStuck.EndPosition + vSkin );

		if ( IsEmpty( in tAddSkin, out _, 0f ) )
		{
			move.Point = tAddSkin;
			move.IsStuck = false;

			return true;
		}
		*/

		return move.IsStuck;
	}

	protected virtual void SnapTo( in SceneTraceResult trMove, ProjectedMovement move )
	{
		var tSnapTo = move.Point.WithPosition( trMove.EndPosition );

		if ( IsEmpty( in tSnapTo, out _, skin: 0f ) )
		{
			move.Point = tSnapTo;
			return;
		}
	}

	protected virtual void StickToGround( ProjectedMovement move )
	{
		if ( !GroundingEnabled )
		{
			move.IsGrounded = false;
			return;
		}

		var skin = SkinWidth.Positive();

		var stickDist = GroundDistance.Max( in skin );
		var trGround = GroundTrace( move.Point, stickDist );

		move.IsGrounded = trGround.Hit && IsGround( in trGround.Normal );

		if ( move.IsGrounded )
		{
			SnapTo( trGround, move );

			move.Velocity = Vector3.VectorPlaneProject( Velocity, Up );
			move.GroundNormal = trGround.Normal;
		}
	}

	/// <summary>
	/// Redirects momentum along the hit surface.
	/// </summary>
	/// <param name="normal"> The direction of the surface to slide along. </param>
	/// <param name="move"> The current movement projection. </param>
	protected virtual void Slide( in Vector3 normal, ProjectedMovement move )
	{
		if ( move.Direction == default || normal == default )
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
		var vel = move.Velocity;

		if ( IsGround( normal ) )
			move.Velocity = Vector3.VectorPlaneProject( in vel, Up );
		else
			move.Velocity = Vector3.VectorPlaneProject( in vel, in normal );

		// Affect direction/distance.
		var vProject = Vector3.VectorPlaneProject( in move.Direction, in normal );

		move.Direction = vProject.Normal;
		move.Distance *= vProject.Length;
	}
}
