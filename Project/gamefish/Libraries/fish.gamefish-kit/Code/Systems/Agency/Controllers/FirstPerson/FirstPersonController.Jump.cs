namespace GameFish;

partial class FirstPersonController
{
	/// <summary>
	/// Should this be able to jump?
	/// </summary>
	[Property]
	[Feature( CONTROLLER ), Order( JUMPING_ORDER )]
	[ToggleGroup( nameof( JumpingEnabled ), Label = JUMPING )]
	public bool JumpingEnabled { get; set; } = true;

	/// <summary>
	/// The button to let you jump. <br />
	/// Set this to blank/null to disable it.
	/// </summary>
	[Property]
	[InlineEditor]
	[Title( "Input" )]
	[Feature( CONTROLLER ), Order( JUMPING_ORDER )]
	[ToggleGroup( nameof( JumpingEnabled ) )]
	public InputSetting JumpInput { get; set; } = new( "Jump", InputMode.Pressed );

	/// <summary>
	/// The sudden force from jumping.
	/// </summary>
	[Property]
	[Title( "Cooldown" )]
	[Range( 0f, 0.5f, clamped: false )]
	[Feature( CONTROLLER ), Order( JUMPING_ORDER )]
	[ToggleGroup( nameof( JumpingEnabled ) )]
	public virtual float JumpCooldown { get; set; } = 0.1f;

	/// <summary>
	/// The sudden force from jumping.
	/// </summary>
	[Property]
	[Title( "Impulse" )]
	[Range( 0f, 1000f, clamped: false )]
	[Feature( CONTROLLER ), Order( JUMPING_ORDER )]
	[ToggleGroup( nameof( JumpingEnabled ) )]
	public virtual float JumpImpulse { get; set; } = 400f;

	/// <summary>
	/// Extra horizontal speed from movement direction upon jumping.
	/// </summary>
	[Property]
	[Title( "Leap Speed" )]
	[Range( 0f, 250f, clamped: false )]
	[Feature( CONTROLLER ), Order( JUMPING_ORDER )]
	[ToggleGroup( nameof( JumpingEnabled ) )]
	public virtual float JumpLeap { get; set; } = 0f;

	/// <summary>
	/// The incline of a slope adds horizontal velocity to jumps by this much.
	/// </summary>
	[Property]
	[Title( "Slope Factor" )]
	[Range( 0f, 2f, clamped: false )]
	[Feature( CONTROLLER ), Order( JUMPING_ORDER )]
	[ToggleGroup( nameof( JumpingEnabled ) )]
	public virtual float JumpSlopeFactor { get; set; } = 1f;

	[Sync]
	public TimeSince? LastJumped { get; set; }

	/// <returns> If the ability to jump is not blocked somehow(such as a cooldown). </returns>
	public virtual bool IsJumpingAllowed()
	{
		if ( !JumpingEnabled )
			return false;

		if ( LastJumped is not TimeSince sinceJump )
			return true;

		return sinceJump >= JumpCooldown;
	}

	/// <returns> If jumping is currently intended. </returns>
	protected virtual bool IsWishingJump()
		=> JumpInput.IsActive;

	protected virtual void UpdateJumping( in float deltaTime )
	{
		if ( ShouldJump() )
			Jump();
	}

	/// <returns> If we should jump this frame(such as if pressed). </returns>
	protected virtual bool ShouldJump()
	{
		if ( !IsGrounded )
			return false;

		if ( !IsJumpingAllowed() )
			return false;

		return IsWishingJump();
	}

	/// <returns> The velocity to add from jumping. </returns>
	public virtual Vector3 GetJumpVelocity()
	{
		var vel = Up * JumpImpulse;

		if ( JumpLeap != 0f )
		{
			var wishDir = WishVelocity.Normal.Horizontal( Up );
			vel += wishDir * JumpLeap;
		}

		if ( !IsGrounded || GroundNormal == default )
			return vel;

		var hSlope = (GroundNormal * JumpImpulse).Horizontal( Up );
		vel += hSlope * JumpSlopeFactor;

		return vel;
	}

	/// <summary>
	/// Performs a jump with optional velocity override.
	/// </summary>
	public virtual void Jump( in Vector3? jumpVel = null )
	{
		var impulse = jumpVel ?? GetJumpVelocity();

		if ( impulse.AlmostEqual( 0f ) )
			return;

		Velocity.Separate( Up, out var upVel, out var hVel );

		// Prevent staggered jumps by negating downward velocity.
		var upDot = upVel.Dot( Up );
		upVel = Up * upDot.Positive();

		OnPreJump();

		Velocity = hVel + upVel + impulse;
	}

	/// <summary>
	/// Prepares us for leaving the ground before applying jump velocity.
	/// </summary>
	protected virtual void OnPreJump()
	{
		LastJumped = 0f;
		IsGrounded = false;
	}
}
