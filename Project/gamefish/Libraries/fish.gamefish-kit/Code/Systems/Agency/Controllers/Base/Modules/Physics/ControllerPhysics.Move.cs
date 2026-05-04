namespace GameFish;

partial class ControllerPhysics
{
	/// <summary>
	/// The limit of movement algorithm iterations that can be performed.
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

	/// <summary> What position the movement started at. </summary>
	protected Transform Start = global::Transform.Zero;

	/// <summary> The inverse of the offset we're applying from the start. </summary>
	protected Offset Delta = new();

	/// <summary> The currently projected end position/rotation. </summary>
	protected Transform Result = global::Transform.Zero;

	/// <summary> The currently projected movement direction. </summary>
	protected Vector3 Direction { get; set; }

	/// <summary> The distance left to travel. </summary>
	protected float Distance
	{
		get => _dist;
		set => _dist = value.Positive();
	}

	private float _dist = 0f;

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
		Direction = tFrom.Position.Direction( in to );
		Distance = tFrom.Position.Distance( in to );

		Start = tFrom.WithOffset( TraceOffset );
		Delta = Start.ToLocal( tFrom );

		Result = Start;

		Run();
	}

	/// <summary>
	/// Perform our movement/collision algorithm according to parameters.
	/// </summary>
	protected virtual void Run()
	{
		for ( var i = 0; i < MaxIterations; i++ )
		{
			if ( Distance <= 0f )
				break;

			Project();
		}

		End();
	}

	/// <summary>
	/// Finalize movement projection.
	/// </summary>
	protected virtual void End()
	{
		StickToGround();

		Apply();
	}

	/// <summary>
	/// Applies projection results such as position/rotation.
	/// </summary>
	protected virtual void Apply()
	{
		if ( !Pawn.IsValid() )
			return;

		Result = Result.ToWorld( Delta );

		Pawn.WorldTransform = Result.WithScale( Pawn.WorldScale );
	}

	/// <summary>
	/// Move our hypothetical position/rotation and resolve collisions using default skin width.
	/// </summary>
	protected virtual void Project()
		=> Project( SkinWidth.Positive() );

	/// <summary>
	/// Move our hypothetical position/rotation and resolve collisions with a specific skin width.
	/// </summary>
	protected virtual void Project( in float skin )
	{
		if ( Direction == default || Distance <= 0f )
			return;

		var trMove = ProjectionTrace();

		// No need to resolve collisions if there wasn't one.
		if ( !trMove.Hit )
		{
			OnFreeMove( in trMove, Direction, in skin );

			if ( Distance <= 0 )
				return;
		}

		// Stuck inside of something.
		if ( trMove.StartedSolid )
		{
			OnStartedSolid( in trMove, Direction, in skin );

			if ( Distance <= 0 )
				return;
		}

		OnCollision( in trMove, Direction, in skin );
	}

	/// <summary>
	/// Responds to a projected movement being completely unobstructed.
	/// </summary>
	protected virtual void OnFreeMove( in SceneTraceResult trMove, in Vector3 dir, in float skin = 0f )
	{
		Result.Position += Direction * Distance;
		Distance = 0f;
	}

	protected virtual void OnStartedSolid( in SceneTraceResult trSolid, in Vector3 dir, in float skin = 0f )
	{
		// TODO: Proper unstuck algorithm.
		Result.Position += trSolid.Normal * skin;
		Distance -= skin;
	}

	/// <summary>
	/// Responds to hits from projected movement.
	/// </summary>
	protected virtual void OnCollision( in SceneTraceResult trHit, in Vector3 dir, in float skin = 0f )
	{
		SnapTo( in trHit, in skin );

		Distance -= trHit.Distance;

		if ( Distance <= 0f )
			return;

		if ( SlidingEnabled )
			Slide( in dir, in trHit.Normal, in skin );
	}

	protected virtual void SnapTo( in SceneTraceResult trMove, in float skin = 0f )
	{
		var hitGround = IsGround( trMove.Normal );

		if ( hitGround )
		{
			IsGrounded = true;
			Result.Position = trMove.EndPosition + (Up * skin);
		}
		else
		{
			// var vWallProject = Vector3.VectorPlaneProject( trMove.Direction, trMove.Normal );
			// var dir = (vWallProject + trMove.Normal).Normal;

			var dir = trMove.Normal;

			Result.Position = trMove.EndPosition + (dir * skin);
		}
	}

	protected virtual void StickToGround()
	{
		if ( !GroundingEnabled )
			return;

		var stickDist = IsGrounded ? GroundDistance.Max( SkinWidth ) : SkinWidth;
		var trGround = GroundTrace( stickDist );

		IsGrounded = trGround.Hit && IsGround( in trGround.Normal );

		if ( IsGrounded )
		{
			SnapTo( in trGround, SkinWidth );

			GroundNormal = trGround.Normal;
			Velocity = Vector3.VectorPlaneProject( Velocity, Up );
		}
	}

	/// <summary>
	/// Redirects momentum along the hit surface.
	/// </summary>
	/// <param name="normal"> The direction of the surface to slide along. </param>
	/// <param name="skin"> The skin width. </param>
	protected void Slide( in Vector3 normal, in float skin = 0f )
		=> Slide( Direction, in normal, in skin );

	/// <summary>
	/// Redirects momentum along the hit surface.
	/// </summary>
	/// <param name="moveDir"> The direction of movement. </param>
	/// <param name="normal"> The direction of the surface to slide along. </param>
	/// <param name="skin"> The skin width. </param>
	protected virtual void Slide( in Vector3 moveDir, in Vector3 normal, in float skin = 0f )
	{
		if ( moveDir == default || normal == default )
			return;

		var dot = moveDir.Dot( normal );

		if ( dot == 0f )
		{
			Distance = 0f;
			Direction = default;

			Velocity = default;

			return;
		}

		if ( IsGround( normal ) )
			Velocity = Vector3.VectorPlaneProject( Velocity, Up );
		else
			Velocity = Vector3.VectorPlaneProject( Velocity, normal );

		var vProject = Vector3.VectorPlaneProject( in moveDir, in normal );

		Direction = vProject.Normal;
		Distance *= vProject.Length;
	}
}
