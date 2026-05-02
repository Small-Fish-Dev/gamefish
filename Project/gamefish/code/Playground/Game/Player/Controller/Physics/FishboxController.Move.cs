using Boxfish.Utility;
using GameFish;
using ShrimpleCharacterController;

namespace Fishbox;

partial class FishboxController
{
	/// <summary>
	/// How fast the player moves in the air(capped by movement speed).
	/// </summary>
	[Property]
	[Feature( PLAYER ), Group( MOVEMENT ), Order( DEFAULTS_ORDER )]
	[Range( 0f, 10000f, clamped: false ), Step( 1f )]
	public float AirAcceleration { get; set; } = 2000f;

	public MoveHelper MoveHelper { get; set; }

	[Sync]
	public TimeUntil NextJump { get; set; }

	protected override void Move( in float deltaTime )
	{
		PreMove( in deltaTime );
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
		DoAbilities( in deltaTime );
	}

	protected override void PostMove( in float deltaTime )
	{
		FollowParent();

		UpdateUpDirection( in deltaTime );
	}

	protected virtual void UpdateUpDirection( in float deltaTime )
	{
		// Ya gotta get up.
		var vUp = Up;
		var upDirDest = -GravityDirection.Normal;

		if ( upDirDest.AlmostEqual( 0f ) )
			return;

		if ( vUp.AlmostEqual( upDirDest ) )
		{
			if ( vUp != upDirDest )
				SetUpDirection( upDirDest );

			return;
		}

		// SetUpDirection( vUp.SlerpTo( upDirDest, deltaTime ) );
		SetUpDirection( upDirDest );
	}

	public override float GetMovementSpeed()
	{
		if ( IsSliding )
			return SlideAcceleration;

		var speed = base.GetMovementSpeed();

		if ( IsSprinting )
			speed = GetSprintSpeed( speed );

		return speed;
	}

	public override Vector3 CalculateWishDirection( in Vector3? inputDir = null )
	{
		if ( inputDir is not Vector3 moveInput )
			return default;

		var up = -GravityDirection;

		var flatAim = Vector3.VectorPlaneProject( EyeForward, up );
		var rMove = Rotation.LookAt( flatAim, up );

		return rMove * moveInput;
	}

	public override Vector3 CalculateWishVelocity( in Vector3? inputDir = null )
	{
		var wishVel = base.CalculateWishVelocity( inputDir );

		return wishVel;
	}

	protected override bool ShouldJump()
	{
		if ( !JumpingEnabled )
			return false;

		if ( !IsGrounded && !IsWallRunning )
			return false;

		return IsWishingJump();
	}

	public override Vector3 GetJumpVelocity()
		=> GroundNormal * JumpImpulse;

	protected virtual void DoJumping( in float deltaTime )
	{
		if ( !ShouldJump() )
			return;

		if ( IsWallRunning )
		{
			if ( JumpInput.IsPressed )
				DoWallRunJump();

			return;
		}

		if ( !IsGrounded || GroundNormal.AlmostEqual( 0f ) )
			return;

		if ( IsSlipping )
			return;

		IsSliding = false;
		IsGrounded = false;

		NextGround = 0.1f;

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
