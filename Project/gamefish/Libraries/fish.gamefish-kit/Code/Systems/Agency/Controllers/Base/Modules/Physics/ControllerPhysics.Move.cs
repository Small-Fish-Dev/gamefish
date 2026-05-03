namespace GameFish;

partial class ControllerPhysics
{
	/// <summary> Allow projecting momentum along surfaces? </summary>
	[Property]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public bool SlidingEnabled { get; set; } = true;

	/// <summary> What position the movement started at. </summary>
	protected Vector3 StartPosition { get; set; }

	/// <summary> What rotation the movement started at. </summary>
	protected Rotation StartRotation { get; set; }

	/// <summary> The currently projected end position. </summary>
	protected Vector3 Position { get; set; }

	/// <summary> The currently projected end rotation. </summary>
	protected Rotation Rotation { get; set; }

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
		StartPosition = tFrom.Position;
		StartRotation = tFrom.Rotation;

		Position = StartPosition;
		Rotation = StartRotation;

		Direction = tFrom.Position.Direction( in to );
		Distance = tFrom.Position.Distance( in to );

		Run();
	}

	/// <summary>
	/// Perform our movement/collision algorithm according to parameters.
	/// </summary>
	protected virtual void Run()
	{
		if ( Distance > 0f )
			Project();

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

		var tPawn = Pawn.WorldTransform;

		tPawn.Position = Position;
		tPawn.Rotation = Rotation;

		Pawn.WorldTransform = tPawn;
	}

	/// <summary>
	/// Move our hypothetical position/rotation and resolve collisions.
	/// </summary>
	protected virtual void Project()
	{
		if ( Direction == default || Distance <= 0f )
			return;

		var dest = Position + (Direction * Distance);

		// Add some skin to the trace.
		var skin = SkinWidth;
		dest += Direction * skin;

		var trMove = Trace( Position, dest ).Run();

		// No need to resolve collisions if there wasn't one.
		if ( !trMove.Hit )
		{
			Position += Direction * Distance;
			Distance = 0f;
			return;
		}

		// Stuck inside of something.
		if ( trMove.StartedSolid )
		{
			// TODO: Proper unstuck algorithm.
			var vNormalSkin = trMove.Normal * SkinWidth;
			var vEndSkin = trMove.EndPosition + vNormalSkin;

			Position = vEndSkin;
			// Velocity = default;

			if ( !IsEmpty( vEndSkin, out _ ) )
				return;
		}

		OnCollision( in trMove, Direction );
	}

	/// <summary>
	/// Responds to hits from projected movement.
	/// </summary>
	protected virtual void OnCollision( in SceneTraceResult trHit, in Vector3 dir )
	{
		SnapTo( in trHit );

		Distance -= trHit.Distance;

		if ( Distance <= 0f )
			return;

		if ( SlidingEnabled )
			Slide( in dir, in trHit.Normal );
	}

	protected virtual void SnapTo( in SceneTraceResult trMove )
	{
		var hitGround = IsGround( trMove.Normal );

		if ( hitGround )
		{
			IsGrounded = true;
			Position = trMove.EndPosition + (Up * SkinWidth);
		}
		else
		{
			Position = trMove.EndPosition + (trMove.Normal * SkinWidth);
		}
	}

	protected virtual void StickToGround()
	{
		if ( !GroundingEnabled )
			return;

		var stickDist = IsGrounded ? GroundDistance : SkinWidth.Max( 1f );

		var trGround = Trace()
			.FromTo( Position, Position + (Down * stickDist) )
			.Run();

		IsGrounded = trGround.Hit && IsGround( in trGround.Normal );

		if ( IsGrounded )
		{
			SnapTo( trGround );

			GroundNormal = trGround.Normal;
			Velocity = Vector3.VectorPlaneProject( Velocity, Up );
		}
	}

	/// <summary>
	/// Redirects momentum along the hit surface.
	/// </summary>
	/// <param name="normal"> The direction of the surface to slide along. </param>
	protected void Slide( in Vector3 normal )
		=> Slide( Direction, in normal );

	/// <summary>
	/// Redirects momentum along the hit surface.
	/// </summary>
	/// <param name="moveDir"> The direction of movement. </param>
	/// <param name="normal"> The direction of the surface to slide along. </param>
	protected virtual void Slide( in Vector3 moveDir, in Vector3 normal )
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
