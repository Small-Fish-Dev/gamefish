using GameFish;
using ShrimpleCharacterController;
using SCC = ShrimpleCharacterController.ShrimpleCharacterController;

namespace Playground;

partial class FishboxController
{
	/// <summary>
	/// Small Fish's character controller.
	/// </summary>
	[Property]
	[Feature( PAWN )]
	[Title( "Controller" )]
	public SCC ShrimpleController
	{
		get => _c.IsValid() ? _c
			: _c = _c.GetCached( GameObject, FindMode.EverythingInSelf );

		set { _c = value; }
	}

	protected SCC _c;

	public override Vector3 Velocity
	{
		get => ShrimpleController?.Velocity ?? default;
		set
		{
			if ( ShrimpleController.IsValid() )
				ShrimpleController.Velocity = value;
		}
	}

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
	/// How fast the player moves in the air(capped by movement speed).
	/// </summary>
	[Property]
	[Feature( PLAYER ), Group( MOVEMENT ), Order( DEFAULTS_ORDER )]
	[Range( 0f, 10000f, clamped: false ), Step( 1f )]
	public float AirAcceleration { get; set; } = 2000f;

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
			// DoStrafing( in deltaTime );

			DoWallRunning( in deltaTime );
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
			return SlideAcceleration;

		var speed = base.GetWishSpeed();

		if ( IsSprinting )
			speed = GetSprintSpeed( speed );

		return speed;
	}

	public override Vector3 GetWishVelocity( in Vector3? inputDir = null )
	{
		var wishVel = base.GetWishVelocity( inputDir );

		if ( !ShrimpleController.IsValid() )
			return wishVel;

		// Hack fix for SCC's air aim momentum cancel bug.
		if ( wishVel.AlmostEqual( 0f ) )
			return _c.Velocity.ClampLength( float.Epsilon );

		return wishVel;
	}

	protected virtual void DoAirMovement( in float deltaTime )
	{
		if ( !ShrimpleController.IsValid() )
			return;

		if ( _c.IsOnGround )
			return;

		_c.AirAcceleration = 0f;
		_c.AirDeceleration = 0f;

		var wishDir = WishVelocity.Normal;

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

		var wishDir = WishVelocity.Normal;

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

		if ( IsWallRunning )
		{
			if ( PressedJump )
				DoWallRunJump();

			return;
		}

		if ( !_c.IsOnGround )
			return;

		if ( IsSlipping && Up.Angle( _c.GroundNormal ) >= SlopeAngle )
			return;

		IsSliding = false;

		// Jump relative to the ground/slope.
		// var rForward = Vector3.VectorPlaneProject( EyeForward, _c.GroundNormal );
		// var jumpDir = Rotation.LookAt( rForward, _c.GroundNormal ).Forward;

		// Maintain jump height.
		_c.GroundNormal.Separate( Up, out var _, out var jumpSide );
		var jumpVel = (jumpSide + Up) * JumpSpeed;

		// Cancel previous vertical velocity.
		_c.Velocity = _c.Velocity.Horizontal( Up );
		_c.Punch( jumpVel );
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
}
