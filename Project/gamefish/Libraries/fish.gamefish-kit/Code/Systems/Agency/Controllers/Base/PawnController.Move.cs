namespace GameFish;

partial class PawnController
{
	protected const int PHYSICS_ORDER = PAWN_ORDER + 100;
	protected const int MOVEMENT_ORDER = PHYSICS_ORDER + 100;
	protected const int PAWN_DEBUG_ORDER = PAWN_ORDER + 900;

	public virtual Vector3 Gravity => Scene?.PhysicsWorld?.Gravity ?? default;

	/// <summary>
	/// Should this be able to input its movement?
	/// </summary>
	[Property]
	[Feature( PAWN )]
	[ToggleGroup( nameof( MovementEnabled ), Label = MOVEMENT )]
	public virtual bool MovementEnabled { get; set; } = true;

	/// <summary>
	/// The target movement speed to accelerate towards.
	/// </summary>
	[Property]
	[Title( "Speed (default)" )]
	[Range( 0f, 1000f, clamped: false )]
	[ToggleGroup( nameof( MovementEnabled ) )]
	[Feature( PAWN ), Order( MOVEMENT_ORDER )]
	public virtual float MoveSpeed { get; set; } = 250f;

	/// <summary>
	/// How quickly the target speed is reached.
	/// </summary>
	[Property]
	[Range( 0f, 100f, clamped: false )]
	[ToggleGroup( nameof( MovementEnabled ) )]
	[Feature( PAWN ), Order( MOVEMENT_ORDER )]
	public virtual float Acceleration { get; set; } = 10f;

	/// <summary>
	/// Slows their speed down over time.
	/// </summary>
	[Property]
	[ToggleGroup( nameof( MovementEnabled ) )]
	[Feature( PAWN ), Order( MOVEMENT_ORDER )]
	public virtual Friction Friction { get; set; } = new();

	[Property]
	[Title( "Is Grounded" )]
	[ShowIf( nameof( InGame ), true )]
	[Feature( PAWN ), Group( DEBUG ), Order( PAWN_DEBUG_ORDER )]
	protected bool InspectorIsGrounded => IsGrounded;

	[Normal]
	[Property]
	[Title( "Ground Normal" )]
	[ShowIf( nameof( InGame ), true )]
	[Feature( PAWN ), Group( DEBUG ), Order( PAWN_DEBUG_ORDER )]
	protected Vector3 InspectorGroundNormal => Physics?.GroundNormal ?? default;

	public virtual bool IsGrounded
	{
		get => Physics?.IsGrounded is true;
		set
		{
			if ( Physics.IsValid() )
				Physics.IsGrounded = value;
		}
	}

	public virtual Vector3 GroundNormal
	{
		get => Physics?.GroundNormal ?? Vector3.Up;
		set
		{
			if ( Physics.IsValid() )
				Physics.GroundNormal = value;
		}
	}

	public virtual Collider GroundCollider
	{
		get => Physics?.GroundCollider;
		set
		{
			if ( Physics.IsValid() )
				Physics.GroundCollider = value;
		}
	}

	public virtual GameObject GroundObject
	{
		get => Physics?.GroundObject;
		set
		{
			if ( Physics.IsValid() )
				Physics.GroundObject = value;
		}
	}

	/// <inheritdoc cref="ControllerPhysics.OnSetIsGrounded"/>
	public virtual void OnSetIsGrounded( in bool isGrounded )
	{
	}

	/// <inheritdoc cref="ControllerPhysics.OnSetGroundNormal"/>
	public virtual void OnSetGroundNormal( in Vector3 normal )
	{
	}

	/// <inheritdoc cref="ControllerPhysics.OnSetGroundCollider"/>
	public virtual void OnSetGroundCollider( Collider c )
	{
	}

	/// <inheritdoc cref="ControllerPhysics.OnSetGroundObject"/>
	public virtual void OnSetGroundObject( GameObject obj )
	{
	}

	/// <summary>
	/// Tells this controller to perform its movement logic.
	/// </summary>
	public virtual bool TryMove( in float deltaTime, in bool isFixedUpdate )
	{
		Move( in deltaTime );

		return true;
	}

	/// <summary>
	/// Directly executes this controller's movement logic.
	/// </summary>
	protected virtual void Move( in float deltaTime )
	{
		PreMove( in deltaTime );
		PostMove( in deltaTime );
	}

	/// <summary>
	/// Prepares the main movement logic for execution.
	/// A good place to apply your friction and wish velocity.
	/// </summary>
	protected virtual void PreMove( in float deltaTime )
	{
		if ( IsGrounded )
			ApplyFriction( in deltaTime );

		if ( IsMovementAllowed() )
		{
			var addVel = WishVelocity * Acceleration * deltaTime;
			var speedLimit = GetMovementSpeed();

			Velocity = Velocity.AddClamped( addVel, speedLimit );
		}
	}

	/// <summary>
	/// Allows you to adjust movement results.
	/// </summary>
	protected virtual void PostMove( in float deltaTime )
	{
	}

	/// <summary>
	/// Reduces velocity over time.
	/// You should apply this before adding velocity.
	/// </summary>
	protected virtual void ApplyFriction( in float deltaTime )
	{
		Velocity = Velocity.WithFriction( Friction, deltaTime );
	}

	public SceneTrace Trace() => Physics?.Trace() ?? default;

	public void Move( in Vector3 from, in Vector3 to ) => Physics?.Move( from, to );
	public void Move( in Transform tFrom, in Transform tDest ) => Physics?.Move( tFrom, tDest );
}
