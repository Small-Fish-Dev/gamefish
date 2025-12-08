namespace GameFish;

partial class BaseController
{
	protected const int MOVEMENT_ORDER = PAWN_ORDER + 1000;

	/// <summary>
	/// Movement/collision logic tries to stay this far away
	/// from surfaces to prevent getting stuck in them.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( PHYSICS ), Order( PAWN_ORDER )]
	public float SkinWidth { get; set; } = 0.2f;

	/// <summary>
	/// Should the owner be able to input their movement?
	/// </summary>
	[Property]
	[Feature( PAWN )]
	[ToggleGroup( nameof( AllowMovement ), Label = MOVEMENT )]
	public virtual bool AllowMovement { get; set; } = true;

	/// <summary>
	/// The target movement speed to accelerate towards.
	/// </summary>
	[Property]
	[Title( "Move Speed" )]
	[Range( 0f, 1000f, clamped: false )]
	[ToggleGroup( nameof( AllowMovement ) )]
	[Feature( PAWN ), Order( MOVEMENT_ORDER )]
	public virtual float MoveSpeed { get; set; } = 250f;

	/// <summary>
	/// How quickly the target speed is reached.
	/// </summary>
	[Property]
	[Range( 0f, 100f, clamped: false )]
	[ToggleGroup( nameof( AllowMovement ) )]
	[Feature( PAWN ), Order( MOVEMENT_ORDER )]
	public virtual float Acceleration { get; set; } = 10f;

	/// <summary>
	/// Reduces horizontal velocity over time.
	/// </summary>
	[Property]
	[ToggleGroup( nameof( AllowMovement ) )]
	[Feature( PAWN ), Order( MOVEMENT_ORDER )]
	public virtual Friction Friction { get; set; }

	public virtual Vector3 Velocity
	{
		get => Pawn?.Velocity ?? Vector3.Zero;
		set
		{
			if ( Pawn.IsValid() )
				Pawn.Velocity = value;
		}
	}

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

	public virtual bool IsGrounded { get; set; } = false;
	public virtual Vector3 GroundNormal { get; set; } = Vector3.Up;

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
	/// Called by the pawn's owner to run controller logic.
	/// Typically ran before movement.
	/// </summary>
	public virtual void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
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
	/// Prepares your main movement logic for execution.
	/// A good place to apply your friction and wish velocity.
	/// </summary>
	protected virtual void PreMove( in float deltaTime )
	{
		ApplyFriction( in deltaTime );

		if ( AllowMovement )
			Velocity = Velocity.AddClamped( WishVelocity * Acceleration * deltaTime, MoveSpeed );
	}

	/// <summary>
	/// Allows you to adjust movement results.
	/// </summary>
	protected virtual void PostMove( in float deltaTime )
	{
	}

	/// <summary>
	/// This is where your solid object filters and such go.
	/// </summary>
	/// <returns> The basis of every collison trace. </returns>
	public virtual SceneTrace Trace()
	{
		if ( !Scene.IsValid() )
			return default;

		return Scene.Trace
			.Size( BBox.FromHeightAndRadius( 16f, 8f ) )
			// .Sphere( 16f, from, to )
			.IgnoreGameObjectHierarchy( GameObject );
	}

	/// <summary>
	/// Creates your default collision trace and sets the start and end points.
	/// </summary>
	/// <returns> The basis of every collison trace(including a start/end). </returns>
	public SceneTrace Trace( in Vector3 from, in Vector3 to )
		=> Trace().FromTo( from, to );

	/// <summary>
	/// Reduces velocity over time.
	/// You should apply this before adding velocity.
	/// </summary>
	protected virtual void ApplyFriction( in float deltaTime )
	{
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

		move.WithTrace( Trace() )
			.Move( from, to, Velocity );

		WorldPosition = move.Position;
	}
}
