namespace GameFish;

partial class BaseController
{
	protected const int PHYSICS_ORDER = PAWN_ORDER + 100;
	protected const int MOVEMENT_ORDER = PHYSICS_ORDER + 100;
	protected const int PAWN_DEBUG_ORDER = PAWN_ORDER + 900;

	public Rigidbody Rigidbody => GameObject?.GetCached( ref _rb, FindMode.EverythingInSelf | FindMode.InAncestors );

	protected Rigidbody _rb;

	public virtual Vector3 Velocity
	{
		get => Rigidbody?.Velocity ?? Vector3.Zero;
		set
		{
			if ( Rigidbody.IsValid() )
				Rigidbody.Velocity = value;

			OnSetVelocity( in value );
		}
	}

	protected virtual void OnSetVelocity( in Vector3 vel )
	{
	}

	/// <summary>
	/// Movement/collision logic tries to stay this far away
	/// from surfaces to prevent getting stuck in them.
	/// </summary>
	[Property]
	[Range( 0.01f, 5f, clamped: false )]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public float SkinWidth { get; set; } = 0.5f;

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

	[Sync]
	public Vector3 WishVelocity
	{
		get => _wishVel;
		set
		{
			if ( _wishVel == value )
				return;

			_wishVel = value;
			OnSetWishVelocity( in value );
		}
	}

	protected Vector3 _wishVel = Vector3.Zero;

	[Property]
	[Title( "Is Grounded" )]
	[Feature( PAWN ), Group( DEBUG ), Order( PAWN_DEBUG_ORDER )]
	protected bool InspectorIsGrounded => IsGrounded;

	[Normal]
	[Property]
	[Title( "Ground Normal" )]
	[Feature( PAWN ), Group( DEBUG ), Order( PAWN_DEBUG_ORDER )]
	protected Vector3 InspectorGroundNormal => GroundNormal;

	[Sync]
	public bool IsGrounded
	{
		get => _isGrounded;
		set
		{
			if ( _isGrounded == value )
				return;

			_isGrounded = value;
			OnSetIsGrounded( in value );
		}
	}

	protected bool _isGrounded;

	[Sync] public Vector3 GroundNormal { get; set; } = Vector3.Up;
	[Sync] public Collider GroundCollider { get; set; }
	[Sync] public GameObject GroundObject { get; set; }

	public virtual Vector3 Gravity => Scene?.PhysicsWorld?.Gravity ?? default;

	/// <summary>
	/// The current movement data/utility.
	/// Used to manually move stuff with collision.
	/// </summary>
	public virtual MoveHelper Mover
	{
		get => _move;
		set => _move = value;
	}

	protected MoveHelper _move;

	public bool CanSimulate() => !IsProxy;

	/// <summary>
	/// Called when <see cref="IsGrounded"/> is changed to something else.
	/// </summary>
	protected virtual void OnSetIsGrounded( in bool isGrounded )
	{
		GroundCollider = null;
		GroundObject = null;
	}

	protected virtual void OnSetWishVelocity( in Vector3 wishVel )
	{
	}

	/// <summary>
	/// Tells this controller to perform its movement logic.
	/// </summary>
	public virtual bool TryMove( in float deltaTime, in bool isFixedUpdate, in Vector3 wishVel = default )
	{
		WishVelocity = wishVel;

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
		ApplyFriction( in deltaTime );

		if ( IsMovementAllowed() )
			Velocity = Velocity.AddClamped( WishVelocity * Acceleration * deltaTime, MoveSpeed );
	}

	/// <summary>
	/// Allows you to adjust movement results.
	/// </summary>
	protected virtual void PostMove( in float deltaTime )
	{
	}

	/// <summary>
	/// This is where solid object filters and such go.
	/// </summary>
	/// <returns> The basis of every collison trace. </returns>
	public virtual SceneTrace BuildTrace()
	{
		if ( !Scene.IsValid() )
			return default;

		return Scene.Trace
			.Size( BBox.FromHeightAndRadius( 16f, 8f ) )
			.IgnoreGameObjectHierarchy( GameObject );
	}

	/// <summary>
	/// Creates the default collision trace and sets the start and end points.
	/// </summary>
	/// <returns> The basis of every collison trace(including a start/end). </returns>
	public virtual SceneTrace BuildTrace( Vector3 from, Vector3 to )
		=> BuildTrace().FromTo( from, to );

	/// <summary>
	/// Creates the default collision trace and sets the end point relative to our starting position.
	/// </summary>
	/// <returns> The basis of every collison trace(including a start/end). </returns>
	public virtual SceneTrace BuildTrace( Vector3 delta )
		=> BuildTrace( WorldPosition, WorldPosition + delta );

	/// <summary>
	/// Reduces velocity over time.
	/// You should apply this before adding velocity.
	/// </summary>
	protected virtual void ApplyFriction( in float deltaTime )
	{
		if ( !IsGrounded )
			return;

		Velocity = Velocity.WithFriction( Friction, deltaTime );
	}

	/// <summary>
	/// Moves using traces using a relative vector for the destination.
	/// Basically adds <paramref name="delta"/> to the current position.
	/// </summary>
	public void MoveBy( in Vector3 delta )
		=> MoveTo( WorldPosition + delta );

	/// <summary>
	/// Moves using traces from the current position towards the destination.
	/// </summary>
	public void MoveTo( in Vector3 to )
		=> Move( WorldPosition, in to );

	/// <summary>
	/// Moves using traces from one position to another.
	/// </summary>
	public virtual void Move( in Vector3 from, in Vector3 to )
	{
		if ( from == to )
			return;

		var move = Mover ??= new();

		move.WithTrace( BuildTrace() )
			.Move( from, to, Velocity );

		WorldPosition = move.Position;
		Velocity = move.Velocity;
	}
}
