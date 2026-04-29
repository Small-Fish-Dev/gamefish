namespace GameFish;

/// <summary>
/// A very basic controller with sprinting and ducking.
/// Ideal for use with first-person shooters.
/// </summary>
public abstract class FirstPersonController : BaseController
{
	/// <summary>
	/// Should this be able to toggle increasing its speed?
	/// </summary>
	[Property]
	[Feature( PAWN ), Order( SPRINT_ORDER )]
	[ToggleGroup( nameof( SprintingEnabled ), Label = SPRINTING )]
	public virtual bool SprintingEnabled { get; set; } = true;

	[Property]
	[InputAction]
	[Title( "Input" )]
	[Feature( PAWN ), Order( SPRINT_ORDER )]
	[ToggleGroup( nameof( SprintingEnabled ) )]
	public string SprintInput { get; set; } = "Run";

	[Property]
	[Feature( PAWN ), Order( SPRINT_ORDER )]
	[ToggleGroup( nameof( SprintingEnabled ) )]
	[Range( 0f, 3f, clamped: false ), Step( 0.01f )]
	public virtual float SprintMultiplier { get; set; } = 1.5f;

	/// <summary>
	/// If true: sprinting is on when not held and is toggled off instead.
	/// </summary>
	[Property]
	[Title( "Starts Enabled" )]
	[Feature( PAWN ), Order( SPRINT_ORDER )]
	[ToggleGroup( nameof( SprintingEnabled ) )]
	public virtual bool IsSprintDefault { get; set; } = false;

	/// <summary>
	/// Should this be able to crouch?
	/// </summary>
	[Property]
	[Feature( PAWN ), Order( DUCKING_ORDER )]
	[ToggleGroup( nameof( DuckingEnabled ), Label = DUCKING )]
	public bool DuckingEnabled { get; set; } = true;

	[Property]
	[InlineEditor]
	[Title( "Input" )]
	[Feature( PAWN ), Order( DUCKING_ORDER )]
	[ToggleGroup( nameof( DuckingEnabled ) )]
	public InputSetting DuckInput { get; set; } = new( "Duck", InputMode.Held );

	[Property]
	[Title( "Move Speed" )]
	[Range( 0f, 1000f, clamped: false )]
	[Feature( PAWN ), Order( DUCKING_ORDER )]
	[ToggleGroup( nameof( DuckingEnabled ) )]
	public virtual float MoveSpeedDucked { get; set; } = 120f;

	/// <summary>
	/// Should this be able to jump?
	/// </summary>
	[Property]
	[Feature( PAWN ), Order( JUMPING_ORDER )]
	[ToggleGroup( nameof( JumpingEnabled ), Label = JUMPING )]
	public bool JumpingEnabled { get; set; } = true;

	/// <summary>
	/// The button to let you jump. <br />
	/// Set this to blank/null to disable it.
	/// </summary>
	[Property]
	[InlineEditor]
	[Title( "Input" )]
	[Feature( PAWN ), Order( JUMPING_ORDER )]
	[ToggleGroup( nameof( JumpingEnabled ) )]
	public InputSetting JumpInput { get; set; } = new( "Jump", InputMode.Pressed );

	/// <summary>
	/// The sudden force from jumping.
	/// </summary>
	[Property]
	[Title( "Impulse" )]
	[Feature( PAWN ), Order( JUMPING_ORDER )]
	[ToggleGroup( nameof( JumpingEnabled ) )]
	public virtual float JumpImpulse { get; set; } = 400f;

	[Property]
	[Title( "Standing Height" )]
	[Feature( VIEW ), Group( EYE_POS ), Order( EYEPOS_ORDER )]
	public virtual float EyeHeightStand { get; set; } = 64f;

	[Property]
	[Title( "Ducked Height" )]
	[Feature( VIEW ), Group( EYE_POS ), Order( EYEPOS_ORDER )]
	public virtual float EyeHeightDuck { get; set; } = 32f;

	[Sync]
	public bool IsDucking
	{
		get => _isDucking;
		set
		{
			if ( _isDucking == value )
				return;

			_isDucking = value;
			OnSetIsDucking( value );
		}
	}

	protected bool _isDucking = false;

	[Sync]
	public bool IsSprinting
	{
		get => _isSprinting;
		set
		{
			if ( _isSprinting == value )
				return;

			_isSprinting = value;
			OnSetIsSprinting( value );
		}
	}

	protected bool _isSprinting = false;

	protected virtual void OnSetIsDucking( in bool isDucking )
	{
	}

	protected virtual void OnSetIsSprinting( in bool isSprinting )
	{
	}

	protected override void UpdateInput( in float deltaTime )
	{
		base.UpdateInput( deltaTime );

		var isAlive = Pawn?.IsAlive is true;

		IsDucking = isAlive && ShouldDuck();
		IsSprinting = isAlive && ShouldSprint();
	}

	/// <returns> If ducking is currently intended. </returns>
	protected virtual bool IsWishingDuck()
		=> DuckInput.IsActive;

	/// <returns> If sprinting is currently intended. </returns>
	protected virtual bool IsWishingSprint()
		=> Input.Down( SprintInput ) == !IsSprintDefault;

	/// <returns> If jumping is currently intended. </returns>
	protected virtual bool IsWishingJump()
		=> JumpInput.IsActive;

	/// <returns> If ducking should be active. </returns>
	protected virtual bool ShouldDuck()
	{
		if ( !DuckingEnabled )
			return false;

		return IsWishingDuck();
	}

	/// <returns> If sprinting should be active. </returns>
	protected virtual bool ShouldSprint()
	{
		if ( !SprintingEnabled )
			return false;

		return IsWishingSprint();
	}

	public virtual float GetSprintSpeed( in float? baseSpeed = null )
		=> (baseSpeed ?? MoveSpeed) * SprintMultiplier;

	public override float GetMovementSpeed()
	{
		var moveSpeed = MoveSpeed;

		// Affect move speed smoothly between stances.
		if ( DuckingEnabled )
			moveSpeed = LocalEyePosition.z.Remap( EyeHeightDuck, EyeHeightStand, MoveSpeedDucked, MoveSpeed );

		if ( IsWishingSprint() )
			moveSpeed = GetSprintSpeed( moveSpeed );

		return moveSpeed;
	}

	/// <returns> If jumping should be performed this frame. </returns>
	protected virtual bool ShouldJump()
	{
		if ( !JumpingEnabled )
			return false;

		if ( !IsGrounded )
			return false;

		return IsWishingJump();
	}

	public virtual Vector3 GetJumpVelocity()
		=> WorldRotation.Up * JumpImpulse;

	public override Vector3 GetLocalEyeTargetPosition()
		=> Vector3.Up * (IsDucking ? EyeHeightDuck : EyeHeightStand);
}
