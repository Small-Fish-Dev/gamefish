namespace GameFish;

partial class SpectatorPawn
{
	[Property]
	[InputAction]
	[Title( "Run" )]
	[Feature( SPECTATING ), Group( INPUT )]
	public virtual string RunAction { get; set; } = "Run";
	public virtual bool AllowRunning => !string.IsNullOrWhiteSpace( RunAction );

	[Property]
	[InputAction]
	[Title( "Ascend" )]
	[Feature( SPECTATING ), Group( INPUT )]
	public virtual string AscendAction { get; set; } = "Jump";
	public virtual bool AllowAscend => !string.IsNullOrWhiteSpace( AscendAction );

	[Property]
	[InputAction]
	[Title( "Descend" )]
	[Feature( SPECTATING ), Group( INPUT )]
	public virtual string DescendAction { get; set; } = "Duck";
	public virtual bool AllowDescend => !string.IsNullOrWhiteSpace( DescendAction );

	/// <summary>
	/// Allow flying around while not spectating someone.
	/// </summary>
	[Property]
	[Title( "Enabled" )]
	[Feature( SPECTATING ), Group( FLYING )]
	public virtual bool FlyingEnabled { get; set; } = true;

	/// <summary>
	/// The speed to move while not spectating someone.
	/// </summary>
	[Property]
	[Title( "Speed" )]
	[Range( 0f, 5000f, clamped: false )]
	[Feature( SPECTATING ), Group( FLYING )]
	public virtual float FlyingSpeed { get; set; } = 1000f;

	/// <summary>
	/// The speed to move while not spectating someone.
	/// </summary>
	[Property]
	[Title( "Run Speed" )]
	[Range( 0f, 5000f, clamped: false )]
	[Feature( SPECTATING ), Group( FLYING )]
	public virtual float FlyingRunSpeed { get; set; } = 2000f;

	/// <summary>
	/// The speed to move while not spectating someone.
	/// </summary>
	[Property]
	[Title( "Friction" )]
	[Feature( SPECTATING ), Group( FLYING )]
	public virtual Friction FlyingFriction { get; set; }

	/// <summary>
	/// Should we collide while moving when not spectating someone?
	/// </summary>
	[Property]
	[Title( "Collision" )]
	[Feature( SPECTATING ), Group( FLYING )]
	public bool FlyingCollision { get; set; } = false;

	/// <summary>
	/// Should we collide while moving when not spectating someone?
	/// </summary>
	[Property]
	[Title( "Collision Radius" )]
	[Feature( SPECTATING ), Group( FLYING )]
	[ShowIf( nameof( FlyingCollision ), true )]
	public float FlyingCollisionRadius { get; set; } = 16f;

	/// <summary>
	/// Collide with objects using these tags.
	/// </summary>
	[Property]
	[Title( "Hit Tags" )]
	[Feature( SPECTATING ), Group( FLYING )]
	[ShowIf( nameof( FlyingCollision ), true )]
	public TagSet FlyingHitTags { get; set; } = ["solid"];

	/// <summary>
	/// Go through objects with these tags.
	/// </summary>
	[Property]
	[Title( "Ignore Tags" )]
	[Feature( SPECTATING ), Group( FLYING )]
	[ShowIf( nameof( FlyingCollision ), true )]
	public TagSet FlyingIgnoreTags { get; set; } = ["pawn"];

	public virtual bool ShouldCollide()
		=> FlyingCollision;

	protected virtual void DoFlying( in float deltaTime )
	{
		if ( Spectating.IsValid() || !FlyingEnabled )
			return;

		var speed = AllowRunning && Input.Down( RunAction )
			? FlyingRunSpeed
			: FlyingSpeed;

		WishVelocity = Input.AnalogMove * speed;

		var view = View;

		var rAim = view.IsValid() ? view.EyeRotation : WorldRotation;

		if ( AllowAscend && Input.Down( AscendAction ) )
			WishVelocity += Vector3.Up * speed;

		if ( AllowDescend && Input.Down( DescendAction ) )
			WishVelocity += Vector3.Down * speed;

		Velocity += rAim * WishVelocity * deltaTime;

		// No need to trace if not colliding.
		if ( !ShouldCollide() )
		{
			WorldPosition += Velocity * deltaTime;
			goto Friction;
		}

		// Use a collision helper.
		var trace = Scene.Trace
			.Radius( FlyingCollisionRadius )
			.UsePhysicsWorld()
			.WithAnyTags( FlyingHitTags )
			.WithoutTags( FlyingIgnoreTags )
			.IgnoreGameObjectHierarchy( GameObject );

		var helper = new CharacterControllerHelper
		{
			Trace = trace,
			Bounce = 0,
			Position = WorldPosition,
			MaxStandableAngle = 90,
			Velocity = Velocity
		};

		helper.TryMove( deltaTime );

		Velocity = helper.Velocity;
		WorldPosition = helper.Position;

		Friction:

		Velocity = Velocity.WithFriction( FlyingFriction, deltaTime );
	}
}
