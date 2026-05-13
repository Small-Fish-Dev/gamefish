namespace GameFish;

partial class PawnController
{
	protected const int PHYSICS_ORDER = PAWN_ORDER + 100;
	protected const int MOVEMENT_ORDER = PHYSICS_ORDER + 100;
	protected const int PAWN_DEBUG_ORDER = PAWN_ORDER + 900;

	public float SkinWidth => Physics?.SkinWidth ?? 0f;

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
	[Title( "Max Speed (default)" )]
	[Range( 0f, 1000f, clamped: false )]
	[ToggleGroup( nameof( MovementEnabled ) )]
	[Feature( PAWN ), Order( MOVEMENT_ORDER )]
	public virtual float MoveSpeed { get; set; } = 250f;

	/// <summary>
	/// How quickly maximum speed is reached while grounded.
	/// </summary>
	[Property]
	[Range( 0f, 50f, clamped: false )]
	[ToggleGroup( nameof( MovementEnabled ) )]
	[Feature( PAWN ), Order( MOVEMENT_ORDER )]
	public virtual float Acceleration { get; set; } = 10f;

	/// <summary>
	/// How quickly maximum speed is reached while airborne.
	/// </summary>
	[Property]
	[Range( 0f, 20f, clamped: false )]
	[ToggleGroup( nameof( MovementEnabled ) )]
	[Feature( PAWN ), Order( MOVEMENT_ORDER )]
	public virtual float AirAcceleration { get; set; } = 3f;

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
	protected Vector3 InspectorGroundNormal => GroundNormal;

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

	/// <inheritdoc cref="ControllerPhysics.OnSetGroundObject"/>
	public virtual void OnSetGroundObject( GameObject obj )
	{
	}

	/// <inheritdoc cref="ControllerPhysics.OnSetGroundCollider"/>
	public virtual void OnSetGroundCollider( Collider c )
	{
	}

	/// <inheritdoc cref="ControllerPhysics.OnSetGroundBody"/>
	public virtual void OnSetGroundBody( Rigidbody rb )
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

		Physics?.Simulate( in deltaTime );

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
			ApplyAcceleration( in deltaTime );
	}

	/// <summary>
	/// Allows you to adjust movement results.
	/// </summary>
	protected virtual void PostMove( in float deltaTime )
	{
		if ( !IsGrounded )
			ApplyGravity( in deltaTime );
	}

	/// <summary>
	/// Reduces velocity over time.
	/// You typically apply this before <see cref="ApplyAcceleration"/>.
	/// </summary>
	protected virtual void ApplyFriction( in float deltaTime )
	{
		// Apply only horizontal friction while grounded.
		if ( IsGrounded )
		{
			Velocity.Separate( Up, out var upVel, out var hVel );

			hVel = hVel.WithFriction( Friction, in deltaTime );
			Velocity = hVel + upVel;

			return;
		}

		// Apply friction on each axis if airborne.
		Velocity = Velocity.WithFriction( Friction, in deltaTime );
	}

	/// <summary>
	/// Adds intended velocity.
	/// </summary>
	protected virtual void ApplyAcceleration( in float deltaTime )
	{
		var accel = IsGrounded ? Acceleration : AirAcceleration;
		var addVel = WishVelocity * accel * deltaTime;

		addVel.Separate( Up, out var upAdd, out var hAdd );
		Velocity.Separate( Up, out var upVel, out var hVel );

		var moveSpeed = GetMovementSpeed();
		var currentSpeed = hVel.Length;
		var speedLimit = moveSpeed.Max( currentSpeed );

		hVel = (hVel + hAdd).ClampLength( speedLimit );
		upVel += upAdd;

		Velocity = hVel + upVel;
	}

	protected virtual void ApplyGravity( in float deltaTime )
	{
		Physics?.ApplyGravity( Gravity, in deltaTime );
	}

	public SceneTrace TracePhysics( in Vector3 to, in float skin = 0f )
	{
		if ( !Pawn.IsValid() )
			return default;

		return TracePhysics( Pawn.WorldTransform, in to, skin: in skin );
	}

	public SceneTrace TracePhysics( in Vector3 from, in Vector3 to, in float skin = 0f )
	{
		if ( !Pawn.IsValid() )
			return default;

		var tOrigin = Pawn.WorldTransform.WithPosition( in from );

		return TracePhysics( in tOrigin, in to, skin: in skin );
	}

	public SceneTrace TracePhysics( in Transform tFrom, in Vector3 to, in float skin = 0f )
	{
		if ( !Physics.IsValid() )
			return default;

		return Physics.Trace( in tFrom, in to, skin: in skin );
	}

	public void MoveTo( in Vector3 to ) => Physics?.MoveTo( in to );
	public void MoveDelta( in Vector3 delta ) => Physics?.MoveDelta( in delta );

	public void Move( in Vector3 from, in Vector3 to ) => Physics?.Move( in from, in to );
	public void Move( in Transform tFrom, in Vector3 to ) => Physics?.Move( in tFrom, to );
}
