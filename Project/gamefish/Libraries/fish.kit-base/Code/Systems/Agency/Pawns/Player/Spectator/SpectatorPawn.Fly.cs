namespace GameFish;

partial class SpectatorPawn
{
	[Property]
	[InputAction]
	[Title( "Run" )]
	[Feature( SPECTATOR ), Group( INPUT )]
	public virtual string RunAction { get; set; } = "Run";
	public virtual bool AllowRunning => !string.IsNullOrWhiteSpace( RunAction );

	[Property]
	[InputAction]
	[Title( "Ascend" )]
	[Feature( SPECTATOR ), Group( INPUT )]
	public virtual string AscendAction { get; set; } = "Jump";
	public virtual bool AllowAscend => !string.IsNullOrWhiteSpace( AscendAction );

	[Property]
	[InputAction]
	[Title( "Descend" )]
	[Feature( SPECTATOR ), Group( INPUT )]
	public virtual string DescendAction { get; set; } = "Duck";
	public virtual bool AllowDescend => !string.IsNullOrWhiteSpace( DescendAction );

	/// <summary>
	/// Allow flying around while not spectating someone.
	/// </summary>
	[Property]
	[Title( "Enabled" )]
	[Feature( SPECTATOR ), Group( FLYING )]
	public virtual bool FlyingEnabled { get; set; } = true;

	/// <summary>
	/// The speed to move while not spectating someone.
	/// </summary>
	[Property]
	[Title( "Speed" )]
	[Range( 0f, 5000f, clamped: false )]
	[Feature( SPECTATOR ), Group( FLYING )]
	public virtual float FlyingSpeed { get; set; } = 1000f;

	/// <summary>
	/// The speed to move while not spectating someone.
	/// </summary>
	[Property]
	[Title( "Run Speed" )]
	[Range( 0f, 5000f, clamped: false )]
	[Feature( SPECTATOR ), Group( FLYING )]
	public virtual float FlyingRunSpeed { get; set; } = 2000f;

	/// <summary>
	/// The speed to move while not spectating someone.
	/// </summary>
	[Property]
	[Title( "Friction" )]
	[Feature( SPECTATOR ), Group( FLYING )]
	public virtual Friction FlyingFriction { get; set; }

	/// <summary>
	/// Should we collide while moving when not spectating someone?
	/// </summary>
	[Property]
	[Title( "Collision" )]
	[Feature( SPECTATOR ), Group( FLYING )]
	public bool FlyingCollision { get; set; } = false;

	/// <summary>
	/// Should we collide while moving when not spectating someone?
	/// </summary>
	[Property]
	[Title( "Collision Radius" )]
	[Feature( SPECTATOR ), Group( FLYING )]
	[ShowIf( nameof( FlyingCollision ), true )]
	public float FlyingCollisionRadius { get; set; } = 16f;

	/// <summary>
	/// Collide with objects using these tags.
	/// </summary>
	[Property]
	[Title( "Hit Tags" )]
	[Feature( SPECTATOR ), Group( FLYING )]
	[ShowIf( nameof( FlyingCollision ), true )]
	public TagSet FlyingHitTags { get; set; } = ["solid"];

	/// <summary>
	/// Go through objects with these tags.
	/// </summary>
	[Property]
	[Title( "Ignore Tags" )]
	[Feature( SPECTATOR ), Group( FLYING )]
	[ShowIf( nameof( FlyingCollision ), true )]
	public TagSet FlyingIgnoreTags { get; set; } = ["pawn"];

	public virtual bool ShouldCollide()
		=> FlyingCollision;

	protected virtual void DoFlying( in float deltaTime )
	{
		if ( Spectating.IsValid() || !FlyingEnabled )
			return;

		var view = View;

		if ( !view.IsValid() )
			return;

		var speed = AllowRunning && Input.Down( RunAction )
			? FlyingRunSpeed
			: FlyingSpeed;

		WishVelocity = Input.AnalogMove * speed;

		if ( AllowAscend && Input.Down( AscendAction ) )
			WishVelocity += Vector3.Up * speed;

		if ( AllowDescend && Input.Down( DescendAction ) )
			WishVelocity += Vector3.Down * speed;

		var rAim = view.ViewRotation;

		Velocity += rAim * WishVelocity * deltaTime;

		// No need to trace if not colliding.
		if ( !ShouldCollide() )
		{
			view.ViewPosition += Velocity * deltaTime;
			goto Friction;
		}

		// Use a collision helper.
		var trace = Scene.Trace
			.UsePhysicsWorld()
			.Radius( FlyingCollisionRadius )
			.WithAnyTags( FlyingHitTags )
			.WithoutTags( FlyingIgnoreTags )
			.IgnoreGameObjectHierarchy( GameObject );

		var helper = new CharacterControllerHelper
		{
			Trace = trace,
			Bounce = 0,
			Position = view.ViewPosition,
			MaxStandableAngle = 90,
			Velocity = Velocity
		};

		helper.TryMove( deltaTime );

		Velocity = helper.Velocity;

		view.ViewPosition = helper.Position;

		Friction:

		Velocity = Velocity.WithFriction( FlyingFriction, deltaTime );
	}
}
