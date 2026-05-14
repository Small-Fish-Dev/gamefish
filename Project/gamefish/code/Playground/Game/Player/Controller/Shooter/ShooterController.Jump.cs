using GameFish;

namespace Fishbox;

partial class ShooterController
{
	[Property]
	[Order( BADASS_ORDER )]
	[Feature( BADASS ), Group( AIRJUMP )]
	[ToggleGroup( nameof( AirJumpEnabled ), Label = AIRJUMP )]
	public bool AirJumpEnabled { get; set; } = true;

	[Property]
	[Title( "Jump (up)" )]
	[Order( BADASS_ORDER )]
	[Feature( BADASS ), Group( AIRJUMP )]
	[Range( 100f, 500f, clamped: false )]
	[ToggleGroup( nameof( AirJumpEnabled ) )]
	public float AirJumpUp { get; set; } = 400f;

	[Sync]
	public int AirJumpsRemaining { get; set; } = 0;

	public virtual bool IsAirJumpingAllowed()
	{
		if ( !AirJumpEnabled )
			return false;

		if ( Velocity.Dot( Up ) >= AirJumpUp )
			return false;

		return AirJumpsRemaining > 0;
	}

	protected virtual void DoAirJump()
	{
		AirJumpsRemaining--;

		OnPreJump();

		var upDir = Up;

		Velocity.Separate( in upDir, out var upVel, out var hVel );

		// Cancel out downwards momentum.
		var upSpeed = upVel.Dot( upDir );
		upSpeed = (upSpeed + AirJumpUp).Max( AirJumpUp );

		upVel = upDir * upSpeed;

		Velocity = hVel + upVel;
	}

	public override void OnSetIsGrounded( in bool isGrounded )
	{
		base.OnSetIsGrounded( isGrounded );

		if ( isGrounded )
			ResetAirJumps();
	}

	protected virtual void UpdateAirJumping( in float deltaTime )
	{
	}

	public virtual void ResetAirJumps()
	{
		AirJumpsRemaining = 1;
	}

	protected override bool IsWishingJump()
	{
		if ( IsWallRunning( out _ ) )
			return JumpInput.IsPressed;

		if ( !IsGrounded )
			return JumpInput.IsPressed;

		return base.IsWishingJump();
	}

	protected override bool ShouldJump()
	{
		if ( !IsWishingJump() )
			return false;

		if ( !IsJumpingAllowed() )
			return false;

		if ( IsGrounded || IsWallRunning( out _ ) )
			return true;

		if ( IsAirJumpingAllowed() )
			return true;

		return false;
	}

	protected override void OnPreJump()
	{
		base.OnPreJump();

		StopWallRunning();
	}

	public override void Jump( in Vector3? jumpVel = null )
	{
		// Jumping off the wall you're running on.
		if ( IsWallRunning( out var normal ) )
		{
			DoWallRunJump( in normal );
			return;
		}

		if ( !IsGrounded )
		{
			DoAirJump();
			return;
		}

		base.Jump( jumpVel );
	}
}
