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
	public override float AirAcceleration { get; set; } = 2000f;

	public MoveHelper MoveHelper { get; set; }

	[Sync]
	public TimeUntil NextJump { get; set; }

	protected override void PreMove( in float deltaTime )
	{
		base.PreMove( in deltaTime );

		DoAbilities( in deltaTime );
	}

	protected override void PostMove( in float deltaTime )
	{
		base.PostMove( in deltaTime );

		FollowParent();

		UpdateUpDirection( in deltaTime );
	}

	protected virtual void DoAbilities( in float deltaTime )
	{
		if ( !Pawn.IsValid() && !Pawn.IsAlive )
			return;

		// DoGravity( in deltaTime );

		// DoGroundMovement( in deltaTime );
		// DoStrafing( in deltaTime );

		// DoSliding( in deltaTime );

		// DoWallRunning( in deltaTime );
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
			// if ( vUp != upDirDest )
				// SetUpDirection( upDirDest );

			return;
		}

		// SetUpDirection( vUp.SlerpTo( upDirDest, deltaTime ) );
		// SetUpDirection( upDirDest );
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

	protected override bool ShouldJump()
	{
		if ( !JumpingEnabled )
			return false;

		if ( !IsGrounded && !IsWallRunning )
			return false;

		return IsWishingJump();
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
