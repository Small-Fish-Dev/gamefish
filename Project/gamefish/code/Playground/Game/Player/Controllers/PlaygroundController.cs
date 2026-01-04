using GameFish;
using ShrimpleCharacterController;
using SCC = ShrimpleCharacterController.ShrimpleCharacterController;

namespace Playground;

public partial class PlaygroundController : ShooterController
{
	public const int DEFAULTS_ORDER = 1000;

	public const string SLIDING = "🏄 Sliding";
	public const int SLIDING_ORDER = 4000;

	public const string SURFACE_WATER = "water";
	public const string TAG_SLIPPERY = "slippery";

	/// <summary>
	/// Small Fish's character controller.
	/// </summary>
	[Property]
	[Feature( PAWN )]
	public SCC ShrimpleController
	{
		get => _c.IsValid() ? _c
			: _c = _c.GetCached( GameObject, FindMode.EverythingInSelf );

		set { _c = value; }
	}

	protected SCC _c;

	[Property]
	[Feature( PLAYER ), Group( PHYSICS )]
	[Range( 0f, 1f, clamped: false ), Step( 0.01f )]
	public float GravityFloating { get; set; } = 0.5f;

	/// <summary>
	/// Multiplies airborne gravity while duck is held.
	/// </summary>
	[Property]
	[Feature( PLAYER ), Group( PHYSICS )]
	[Range( 0f, 5f, clamped: false ), Step( 0.01f )]
	public float GravitySinking { get; set; } = 2f;

	/// <summary>
	/// The angle where a surface is a slope(when not sliding).
	/// </summary>
	[Property]
	[Feature( PLAYER ), Group( DEFAULTS ), Order( DEFAULTS_ORDER )]
	[Range( 0f, 90f, clamped: false ), Step( 1f )]
	public float SlopeAngle { get; set; } = 35f;

	/// <summary>
	/// How fast the player moves in the air(capped by movement speed).
	/// </summary>
	[Property]
	[Feature( PLAYER ), Group( MOVEMENT ), Order( DEFAULTS_ORDER )]
	[Range( 0f, 10000f, clamped: false ), Step( 1f )]
	public float AirAcceleration { get; set; } = 6000f;

	[Property]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	[ToggleGroup( nameof( AllowSliding ), Label = SLIDING )]
	public bool AllowSliding { get; set; } = true;

	/// <summary>
	/// Movement speed while sliding. Can't accelerate beyond the current velocity.
	/// </summary>
	[Property]
	[Title( "Move Speed (Sliding)" )]
	[Range( 0f, 1000f, clamped: false )]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	public float SlideMoveSpeed { get; set; } = 700f;

	/// <summary>
	/// Must be going this speed to start sliding while ducked on the ground.
	/// </summary>
	[Property]
	[Title( "Minimum Speed" )]
	[Range( 0f, 1000f, clamped: false )]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	public float SlideMinSpeed { get; set; } = 400f;

	/// <summary>
	/// Stop an active slide while under this speed.
	/// </summary>
	[Property]
	[Title( "Stop Speed" )]
	[Range( 0f, 500f, clamped: false )]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	public float SlideStopSpeed { get; set; } = 50f;

	/// <summary>
	/// Limit of friction while on a tall slope or slippery surface.
	/// </summary>
	[Property]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	[Range( 0f, 1f, clamped: false ), Step( 0.01f )]
	public float SlippingFriction { get; set; } = 0.15f;

	[Property]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	public Curve SlopeSpeed { get; set; } = new( new( 0f, 0f ), new( 1f, 1f ) )
	{
		TimeRange = new( 0f, 90f ),
		ValueRange = new( 0f, 5000f )
	};

	[Property]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	public Curve SlopeFriction { get; set; } = new( new( 0f, 1f ), new( 1f, 0f ) )
	{
		TimeRange = new( 0f, 90f ),
		ValueRange = new( 0f, 350f )
	};

	[Sync]
	[Property]
	[ShowIf( nameof( InGame ), true )]
	[ToggleGroup( nameof( AllowSliding ) )]
	[Feature( PLAYER ), Order( SLIDING_ORDER )]
	public bool IsSliding { get; set; }

	// Jump while held on the ground but only if pressed while airborne.
	public override bool ShouldJump => AllowJumping && HasJumpButton && ShrimpleController.IsValid()
		&& (ShrimpleController.IsOnGround ? Input.Down( JumpButton ) : Input.Pressed( JumpButton ));

	/// <summary>
	/// Is the player on too steep a slope?
	/// </summary>
	[Sync] public bool IsSlipping { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		if ( !ShrimpleController.IsValid() )
			this.Warn( $"needs a {typeof( SCC )} to function!" );
	}

	public override void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
		base.Simulate( deltaTime, isFixedUpdate );

		UpdateView( in deltaTime );
	}

	public override Vector3 GetLocalEyeTargetPosition()
	{
		if ( IsSliding )
			return base.GetLocalEyeTargetPosition() * .75f;

		return base.GetLocalEyeTargetPosition();
	}

	protected override void OnSetWishVelocity( in Vector3 wishVel )
	{
		base.OnSetWishVelocity( wishVel );

		if ( !ShrimpleController.IsValid() )
			return;

		_c.ManuallyUpdate = true;
		_c.WishVelocity = WishVelocity;
	}

	protected override void Move( in float deltaTime )
	{
		PreMove( in deltaTime );

		if ( Pawn.IsValid() && Pawn.IsAlive )
		{
			DoSliding( in deltaTime );

			DoJumping( in deltaTime );
			DoGravity( in deltaTime );

			DoAirMovement( in deltaTime );
			DoStrafing( in deltaTime );
		}

		PostMove( in deltaTime );
	}

	protected override void PreMove( in float deltaTime )
	{
		var isAlive = Pawn?.IsAlive is true;

		IsDucking = isAlive && ShouldDuck;
		IsSprinting = isAlive && ShouldSprint;

		DoGroundTrace();
	}

	protected override void PostMove( in float deltaTime )
	{
		if ( !ShrimpleController.IsValid() )
			return;

		var result = _c.Move( deltaTime, manualUpdate: false );

		// _c.WorldPosition = result.Position;
	}

	public override float GetWishSpeed()
	{
		if ( IsSliding )
			return SlideMoveSpeed;

		var speed = base.GetWishSpeed();

		if ( IsSprinting )
			speed = GetSprintSpeed( speed );

		return speed;
	}

	public override Vector3 GetWishVelocity( in Vector3? inputDir = null )
	{
		var wishVel = base.GetWishVelocity( inputDir );

		// Hack fix for SCC's air aim momentum cancel bug.
		if ( ShrimpleController.IsValid() && !_c.IsOnGround )
			if ( wishVel.AlmostEqual( 0f ) )
				return _c.Velocity.ClampLength( float.Epsilon );

		return wishVel;
	}

	protected virtual void DoAirMovement( in float deltaTime )
	{
		if ( !ShrimpleController.IsValid() || _c.IsOnGround )
			return;

		_c.AirAcceleration = 0f;
		_c.AirDeceleration = 0f;

		var wishDir = GetWishDirection();

		if ( wishDir.AlmostEqual( 0f ) )
			return;

		// Split the horizontal and vertical speeds.
		_c.Velocity.Separate( WorldRotation.Up, out var upVel, out var sideVel );

		// Respect their existing speed relative to the direction we're trying to move.
		var speedLimit = sideVel.Length.Max( MoveSpeed );

		var airMove = wishDir * AirAcceleration * deltaTime;
		sideVel = (sideVel + airMove).ClampLength( speedLimit );

		_c.Velocity = sideVel + upVel;
	}

	/// <summary>
	/// Air and slope strafing.
	/// </summary>
	protected virtual void DoStrafing( in float deltaTime )
	{
		if ( !ShrimpleController.IsValid() )
			return;

		// If you're on the ground you don't need this.
		if ( _c.IsOnGround && !IsSliding )
			return;

		var wishDir = GetWishDirection();

		if ( wishDir.AlmostEqual( 0f ) )
			return;

		// Split the horizontal and vertical speeds.
		_c.Velocity.Separate( WorldRotation.Up, out var vVel, out var hVel );

		// Poor man's air strafe.
		var velDir = hVel.Normal;

		// var speed = hVel.Length;
		// var curve = _c.IsOnGround && IsSliding ? SlideStrafing : AirStrafing;
		// var turnDot = velDir.Dot( wishDir ).Positive().Remap( 1f, 0f );

		var turn = (velDir + wishDir).Normal;

		hVel = (hVel + turn * deltaTime).Normal * hVel.Length;

		_c.Velocity = hVel + vVel;
	}

	protected virtual void DoJumping( in float deltaTime )
	{
		if ( !ShrimpleController.IsValid() )
			return;

		if ( !AllowJumping || !ShouldJump )
			return;

		if ( _c.IsOnGround )
		{
			var up = WorldRotation.Up;

			if ( IsSlipping && up.Angle( _c.GroundNormal ) >= SlopeAngle )
				return;

			IsSliding = false;

			// Jump relative to the ground/slope.
			// var rForward = Vector3.VectorPlaneProject( EyeForward, _c.GroundNormal );
			// var jumpDir = Rotation.LookAt( rForward, _c.GroundNormal ).Forward;

			// Maintain jump height.
			_c.GroundNormal.Separate( up, out var _, out var jumpSide );
			var jumpVel = (jumpSide + up) * JumpSpeed;

			// Cancel previous vertical velocity.
			_c.Velocity = _c.Velocity.Horizontal( up );
			_c.Punch( jumpVel );
		}
	}

	protected virtual void DoGravity( in float deltaTime )
	{
		if ( !ShrimpleController.IsValid() )
			return;

		var grav = Scene?.PhysicsWorld?.Gravity ?? default;

		if ( !_c.IsOnGround )
		{
			if ( ShouldJump )
				grav *= GravityFloating;

			if ( !IsSliding && ShouldDuck )
				grav *= GravitySinking;
		}

		_c.VectorGravity = grav;
	}

	public SceneTraceResult GroundTrace { get; set; }
	public Surface GroundSurface { get; set; }
	public float SurfaceFriction { get; set; }

	public virtual void DoGroundTrace()
	{
		if ( !ShrimpleController.IsValid() || !_c.IsOnGround )
			return;

		var startPos = WorldPosition;
		var down = WorldRotation.Down;

		GroundTrace = _c.BuildTrace( _c.Bounds, startPos, startPos + down * 2f );
		GroundSurface = GroundTrace.Surface;
		SurfaceFriction = GroundTrace.Surface?.Friction ?? 1f;
	}

	protected virtual bool IsSlippery( in SceneTraceResult tr )
	{
		if ( !tr.Hit || tr.Surface is not Surface surface )
			return false;

		const float slipFriction = 0.2f;

		return surface.Friction <= slipFriction
			|| surface.ResourceName == SURFACE_WATER
			|| (surface.Tags?.Contains( TAG_SLIPPERY ) ?? false);
	}

	protected virtual void DoSliding( in float deltaTime )
	{
		if ( !ShrimpleController.IsValid() )
			return;

		if ( !AllowSliding )
			goto NotSliding;

		var up = WorldRotation.Up;
		var slopeAngle = up.Angle( _c.GroundNormal );

		IsSlipping = _c.IsOnGround && (slopeAngle > SlopeAngle || IsSlippery( GroundTrace ));

		if ( !IsSliding )
		{
			if ( IsSlipping )
			{
				// Always slide when slipping on a slope.
				IsSliding = true;
			}
			else if ( _c.IsOnGround )
			{
				if ( IsDucking && _c.Velocity.Length > SlideMinSpeed )
				{
					// Start a slide with enough ground speed while crouching.
					IsSliding = true;
				}
			}
		}

		if ( IsSliding )
		{
			_c.MaxGroundAngle = 180f;
			_c.StickToPlatforms = false;
			_c.GroundStickEnabled = false;

			_c.GroundAcceleration = 0f;
			_c.GroundDeceleration = 0f;

			if ( _c.IsOnGround && _c.GroundNormal != Vector3.Zero )
			{
				var slideSpeed = SlopeSpeed.Evaluate( slopeAngle );
				var slideDir = Vector3.VectorPlaneProject( -up, _c.GroundNormal );

				_c.Velocity += slideDir * slideSpeed * deltaTime;

				// Apply friction(mainly for low slopes) if not slipping.
				var frictionScale = SurfaceFriction;

				if ( IsSlipping )
					frictionScale = frictionScale.Min( SlippingFriction );

				var slideFriction = SlopeFriction.Evaluate( slopeAngle ) * frictionScale;

				_c.Velocity -= (_c.Velocity.Normal * slideFriction * deltaTime)
					.ClampLength( _c.Velocity.Length );

				var velLen = _c.Velocity.Length;

				// Stop sliding if going too slow.
				if ( !IsSlipping && slopeAngle <= SlopeAngle )
					if ( velLen <= SlideStopSpeed )
						goto NotSliding;

				// Slower slide movement.
				var slideMove = WishVelocity.ProjectAndScale( _c.GroundNormal ) * deltaTime;
				var maxSlide = velLen;

				if ( slopeAngle < SlopeAngle )
				{
					// Don't move faster than we can walk while slipping.
					var slopeMoveSpeed = (SlideMoveSpeed * slopeAngle.Remap( SlopeAngle, 0f ))
						.Min( MoveSpeed );

					maxSlide = maxSlide.Max( slopeMoveSpeed );
				}

				_c.Velocity = (_c.Velocity + slideMove)
					.ClampLength( maxSlide );
			}

			return;
		}

		NotSliding:

		if ( IsSliding )
			IsSliding = false;

		_c.MaxGroundAngle = SlopeAngle;
		_c.StickToPlatforms = true;
		_c.GroundStickEnabled = true;

		_c.GroundAcceleration = MoveSpeed * 10f;
		_c.GroundDeceleration = Friction.Value * 500f;
	}
}
