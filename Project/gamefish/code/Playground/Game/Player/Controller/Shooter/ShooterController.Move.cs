using GameFish;

namespace Fishbox;

partial class ShooterController
{
	/// <summary>
	/// Multiplier of gravity while holding jump.
	/// </summary>
	[Property]
	[InputAction]
	[Order( BADASS_ORDER )]
	[Title( "Gravity (float)" )]
	[Feature( BADASS ), Group( GRAVITY )]
	[Range( 0.2f, 1.0f, clamped: false )]
	public virtual float JumpGravityScale { get; set; } = 0.7f;

	/// <summary>
	/// Multiplier of gravity while holding duck.
	/// </summary>
	[Property]
	[InputAction]
	[Order( BADASS_ORDER )]
	[Title( "Gravity (sink)" )]
	[Feature( BADASS ), Group( GRAVITY )]
	[Range( 1.0f, 3.0f, clamped: false )]
	public virtual float DuckGravityScale { get; set; } = 2f;

	[Property]
	[Order( BADASS_ORDER )]
	[Title( "Gravity (wall run)" )]
	[Feature( BADASS ), Group( GRAVITY )]
	[Range( 0.2f, 1.0f, clamped: false )]
	public virtual float WallRunGravityScale { get; set; } = 0.8f;

	public override Vector3 Gravity => Down * base.Gravity.Length * GravityMultiplier();

	protected virtual float GravityMultiplier()
	{
		var mult = 1f;

		if ( JumpInput.IsHeld )
			mult *= JumpGravityScale;

		if ( Input.Down( DuckInput ) )
			mult *= DuckGravityScale;

		if ( IsWallRunning( out _ ) )
			mult *= WallRunGravityScale;

		return mult;
	}

	public override float GetMovementSpeed()
	{
		if ( IsWallRunning( out _ ) )
			return WallRunMoveSpeed;

		return base.GetMovementSpeed();
	}

	protected override bool IsWishingJump()
	{
		if ( IsWallRunning( out _ ) )
			return JumpInput.IsPressed;

		return base.IsWishingJump();
	}

	protected override bool ShouldJump()
	{
		if ( !IsJumpingAllowed() )
			return false;

		if ( !IsGrounded && !IsWallRunning( out _ ) )
			return false;

		return IsWishingJump();
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

		base.Jump( jumpVel );
	}

	protected override void PreMove( in float deltaTime )
	{
		base.PreMove( deltaTime );

		UpdateWallRunning( in deltaTime );
	}
}
