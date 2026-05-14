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
	[Title( "Limit" )]
	[Order( BADASS_ORDER )]
	[Range( 1, 5, clamped: false )]
	[Feature( BADASS ), Group( AIRJUMP )]
	[ToggleGroup( nameof( AirJumpEnabled ) )]
	public virtual int AirJumpsLimit { get; set; } = 1;

	[Property]
	[Title( "Jump (up)" )]
	[Order( BADASS_ORDER )]
	[Feature( BADASS ), Group( AIRJUMP )]
	[Range( 100f, 500f, clamped: false )]
	[ToggleGroup( nameof( AirJumpEnabled ) )]
	public virtual float AirJumpUp { get; set; } = 400f;

	[Property]
	[Order( BADASS_ORDER )]
	[Title( "Recharge Delay" )]
	[Range( 1f, 5f, clamped: false )]
	[Feature( BADASS ), Group( AIRJUMP )]
	[ToggleGroup( nameof( AirJumpEnabled ) )]
	public virtual float AirJumpRechargeDelay { get; set; } = 3.0f;

	[Sync]
	public int AirJumpsRemaining { get; set; } = 0;

	[Sync]
	public TimeUntil NextAirJumpRecharge { get; set; }

	public virtual bool IsAirJumpingAllowed()
	{
		if ( !AirJumpEnabled )
			return false;

		return AirJumpsRemaining > 0;
	}

	protected virtual void DoAirJump()
	{
		AirJumpsRemaining--;

		var upDir = Up;

		Velocity.Separate( in upDir, out var upVel, out var hVel );

		// Cancel out downwards momentum.
		var upSpeed = upVel.Dot( upDir );
		upSpeed = (upSpeed + AirJumpUp).Max( AirJumpUp );

		upVel = upDir * upSpeed;

		OnPreJump();

		Velocity = hVel + upVel;
	}

	public virtual void RechargeAirJumps( in int? count = null )
	{
		NextAirJumpRecharge = AirJumpRechargeDelay;

		if ( count is null )
		{
			AirJumpsRemaining = AirJumpsLimit;
			return;
		}

		int add = count ?? 0;

		var jumps = AirJumpsRemaining.Max( 0 );
		jumps = (jumps + add).Clamp( 1, AirJumpsLimit );

		AirJumpsRemaining = jumps;
	}

	public override void OnSetIsGrounded( in bool isGrounded )
	{
		base.OnSetIsGrounded( isGrounded );

		if ( isGrounded )
			RechargeAirJumps();
	}

	protected virtual void UpdateAirJumping( in float deltaTime )
	{
		if ( !AirJumpEnabled )
			return;

		if ( AirJumpsRemaining >= AirJumpsLimit )
			return;

		if ( !NextAirJumpRecharge )
			return;

		RechargeAirJumps( count: 1 );
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

		NextAirJumpRecharge = AirJumpRechargeDelay;
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
