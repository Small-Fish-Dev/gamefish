using Boxfish.Utility;
using GameFish;
using ShrimpleCharacterController;

namespace Fishbox;

partial class FishboxController
{
	[Sync]
	public override Vector3 Velocity
	{
		get => Rigidbody?.Velocity ?? default;
		set
		{
			if ( Rigidbody.IsValid() )
				Rigidbody.Velocity = value;

			OnSetVelocity( in value );
		}
	}

	/// <summary>
	/// How fast the player moves in the air(capped by movement speed).
	/// </summary>
	[Property]
	[Feature( PLAYER ), Group( MOVEMENT ), Order( DEFAULTS_ORDER )]
	[Range( 0f, 10000f, clamped: false ), Step( 1f )]
	public float AirAcceleration { get; set; } = 2000f;

	public MoveHelper MoveHelper { get; set; }

	[Sync]
	public TimeUntil NextGroundStick { get; set; }

	protected virtual void OnSetVelocity( in Vector3 vel )
	{
	}

	protected override void Move( in float deltaTime )
	{
		PreMove( in deltaTime );

		DoAbilities( in deltaTime );

		PostMove( in deltaTime );
	}

	protected virtual void DoAbilities( in float deltaTime )
	{
		if ( !Pawn.IsValid() && !Pawn.IsAlive )
			return;

		DoJumping( in deltaTime );
		DoGravity( in deltaTime );

		DoGroundMovement( in deltaTime );
		DoAirMovement( in deltaTime );
		// DoStrafing( in deltaTime );

		// DoSliding( in deltaTime );

		DoWallRunning( in deltaTime );
	}

	protected override void PreMove( in float deltaTime )
	{
		var isAlive = Pawn?.IsAlive is true;

		IsDucking = isAlive && ShouldDuck;
		IsSprinting = isAlive && ShouldSprint;

		UpdateGround();
	}

	protected override void PostMove( in float deltaTime )
	{
		// Stick to the ground.
		if ( IsGrounded )
		{
			var vDown = -Up;
			var trStickToGround = TraceColliders( WorldPosition, vDown * GroundStickDistance );
			TryStickToSurface( trStickToGround );
		}
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

	public override Vector3 GetWishDirection( in Vector3? inputDir = null )
	{
		if ( inputDir is not Vector3 moveInput )
			return default;

		var up = -GravityDirection;

		var flatAim = Vector3.VectorPlaneProject( EyeForward, up );
		var rMove = Rotation.LookAt( flatAim, up );

		return rMove * moveInput;
	}

	public override Vector3 GetWishVelocity( in Vector3? inputDir = null )
	{
		var wishVel = base.GetWishVelocity( inputDir );

		return wishVel;
	}

	public override Vector3 GetJumpVelocity()
		=> GroundNormal * JumpSpeed;

	protected virtual void DoJumping( in float deltaTime )
	{
		if ( !AllowJumping || !ShouldJump )
			return;

		if ( IsWallRunning )
		{
			if ( PressedJump )
				DoWallRunJump();

			return;
		}

		if ( !IsGrounded || GroundNormal.AlmostEqual( 0f ) )
			return;

		if ( IsSlipping )
			return;

		IsSliding = false;
		IsGrounded = false;

		// Negate downwards velocity.
		var jumpVel = GetJumpVelocity();
		var jumpDir = jumpVel.Normal;

		Velocity.Separate( jumpDir, out var upVel, out var hVel );

		var vSpeed = jumpDir.Dot( upVel )
			.Max( jumpVel.Length )
			.Positive();

		upVel = jumpDir * vSpeed;

		Velocity = hVel + upVel;
	}

	public virtual void UpdateGround()
	{
		if ( !NextGroundStick )
			return;

		var origin = WorldPosition;

		var vGround = Down * WorldScale.z * GroundCheckDistance;

		GroundTrace = TraceColliders( origin, vGround );

		if ( !GroundTrace.Hit )
		{
			IsGrounded = false;
			return;
		}

		var upVel = Velocity.Forward( GroundNormal );
		var upSpeed = GroundTrace.Normal.Dot( upVel );
		var isRamping = upSpeed >= 300f;

		IsGrounded = !isRamping && IsValidGround( GroundTrace );

		if ( !IsGrounded )
			return;

		GroundNormal = GroundTrace.Normal;
		GroundCollider = GroundTrace.Collider;
		GroundObject = GroundTrace.GameObject;

		TryStickToSurface( GroundTrace );
	}

	protected virtual void DoGroundMovement( in float deltaTime )
	{
		if ( !IsGrounded )
			return;

		ApplyFriction( in deltaTime );

		var wishDir = WishVelocity.Normal;

		if ( wishDir.AlmostEqual( 0f ) )
			return;

		Velocity.Separate( Up, out var upVel, out var sideVel );

		var wishSpeed = GetWishSpeed();
		var speedLimit = sideVel.Length.Max( wishSpeed );

		var speed = Acceleration * wishSpeed;
		var vMove = wishDir * speed * deltaTime;

		sideVel = (sideVel + vMove).ClampLength( speedLimit );

		Velocity = sideVel + upVel;
	}

	protected virtual void DoAirMovement( in float deltaTime )
	{
		if ( IsGrounded )
			return;

		var wishDir = WishVelocity.Normal;

		if ( wishDir.AlmostEqual( 0f ) )
			return;

		// Split the horizontal and vertical speeds.
		Velocity.Separate( Up, out var upVel, out var sideVel );

		// Respect their existing speed relative to the direction we're trying to move.
		var speedLimit = sideVel.Length.Max( MoveSpeed );

		var airMove = wishDir * AirAcceleration * deltaTime;
		sideVel = (sideVel + airMove).ClampLength( speedLimit );

		Velocity = sideVel + upVel;
	}

	/// <summary>
	/// Air and slope strafing.
	/// </summary>
	protected virtual void DoStrafing( in float deltaTime )
	{
		// If you're on the ground you don't need this.
		if ( IsGrounded && !IsSliding )
			return;

		var wishDir = WishVelocity.Normal;

		if ( wishDir.AlmostEqual( 0f ) )
			return;

		// Split the horizontal and vertical speeds.
		Velocity.Separate( Up, out var vVel, out var hVel );

		// Poor man's air strafe.
		var velDir = hVel.Normal;

		// var speed = hVel.Length;
		// var curve = IsOnGround && IsSliding ? SlideStrafing : AirStrafing;
		// var turnDot = velDir.Dot( wishDir ).Positive().Remap( 1f, 0f );

		var turn = (velDir + wishDir).Normal;

		hVel = (hVel + turn * deltaTime).Normal * hVel.Length;

		Velocity = hVel + vVel;
	}
}
