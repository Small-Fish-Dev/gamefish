namespace GameFish;

public partial class SpectatorPawn : BasePawn
{
	public const string TAG_SPECTATOR = "spectator";

	public const string FLYING = "🦅 Flying";

	[Sync]
	[Property]
	[Feature( SPECTATING ), Group( DEBUG )]
	public BasePawn Spectating
	{
		get => _spectating;

		protected set
		{
			_spectating = value;

			if ( this.IsValid() && !IsProxy )
				OnSpectatingSet( value );
		}
	}

	protected BasePawn _spectating;

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
	/// The button that spectates a target or stops if already spectating them.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Spectate Target" )]
	[Feature( SPECTATING ), Group( INPUT )]
	public virtual string SpectateTargetAction { get; set; } = "Use";

	/// <summary>
	/// The button that toggles first/third person(if any).
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Toggle Perspective" )]
	[Feature( SPECTATING ), Group( INPUT )]
	public virtual string TogglePerspectiveAction { get; set; } = "Jump";

	/// <summary>
	/// Allow flying around while not spectating someone.
	/// </summary>
	[Property]
	[Title( "Enabled" )]
	[Feature( SPECTATING ), Group( FLYING )]
	public bool FlyingEnabled { get; set; } = true;

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
	public float FlyingCollisionRadius { get; set; } = 64f;

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

	/// <summary> Spectators can never be spectated. </summary>
	public override bool AllowSpectators => false;

	/// <summary> Spectators can never be spectated. </summary>
	public override bool CanSpectate( BasePawn spec )
		=> false;

	/// <summary> How fast the spectator is moving. </summary>
	[Property]
	[Feature( DEBUG ), Group( PHYSICS )]
	public override Vector3 Velocity { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		Tags?.Add( TAG_SPECTATOR );
	}

	/// <summary>
	/// Called whenever <see cref="Spectating"/> has been set.
	/// </summary>
	protected virtual void OnSpectatingSet( BasePawn target )
	{
		if ( !View.IsValid() )
			return;

		if ( !target.IsValid() )
		{
			View.Mode = PawnView.Perspective.FirstPerson;
			return;
		}

		WorldPosition = View.WorldPosition;
		WorldRotation = View.WorldRotation;

		View.Mode = target.View.IsValid()
			? target.View.Mode
			: PawnView.Perspective.ThirdPerson;
	}

	public override bool TrySpectate( BasePawn target )
	{
		if ( !target.IsValid() || !target.CanSpectate( this ) )
			return false;

		Spectating = target;

		return true;
	}

	public override void StopSpectating()
	{
		base.StopSpectating();

		Spectating = null;
	}

	public override void FrameSimulate( in float deltaTime )
	{
		base.FrameSimulate( deltaTime );

		DoFlying( in deltaTime );
	}

	public virtual bool ShouldCollide()
	{
		return FlyingCollision;
	}

	public virtual void DoFlying( in float deltaTime )
	{
		if ( !FlyingEnabled || Spectating.IsValid() )
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

		if ( !FlyingCollision )
		{
			WorldPosition += Velocity * deltaTime;
			goto Friction;
		}

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
