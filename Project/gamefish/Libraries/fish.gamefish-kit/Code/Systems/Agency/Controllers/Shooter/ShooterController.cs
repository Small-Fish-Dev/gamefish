namespace GameFish;

/// <summary>
/// Rootin', tootin' an' shootin'.
/// </summary>
public partial class ShooterController : BaseController
{
	/// <summary>
	/// The button to run. <br />
	/// Set this to blank/null to disable it.
	/// </summary>
	[Property]
	[InputAction]
	[Feature( INPUT ), Order( SPRINT_ORDER )]
	public virtual string SprintButton { get; set; } = "run";
	public virtual bool HasSprintButton => !string.IsNullOrWhiteSpace( SprintButton );

	protected virtual bool ShouldSprint => AllowSprinting && HasSprintButton
		&& (Input.Down( SprintButton ) == !SprintDefault);

	/// <summary>
	/// The button to crouch. <br />
	/// Set this to blank/null to disable it.
	/// </summary>
	[Property]
	[InputAction]
	[Feature( INPUT ), Order( DUCKING_ORDER )]
	public virtual string DuckButton { get; set; } = "duck";
	public virtual bool HasDuckButton => !string.IsNullOrWhiteSpace( DuckButton );

	public virtual bool ShouldDuck => AllowDucking && HasDuckButton
		&& Input.Down( DuckButton );

	/// <summary>
	/// The button to let you jump. <br />
	/// Set this to blank/null to disable it.
	/// </summary>
	[Property]
	[InputAction]
	[Feature( INPUT ), Order( JUMPING_ORDER )]
	public virtual string JumpButton { get; set; } = "jump";
	public virtual bool HasJumpButton => !string.IsNullOrWhiteSpace( JumpButton );

	public virtual bool ShouldJump => AllowJumping && HasJumpButton
		&& Input.Down( JumpButton );

	/// <summary>
	/// Should the owner be able to increase their speed?
	/// </summary>
	[Property]
	[Feature( PAWN ), Order( SPRINT_ORDER )]
	[ToggleGroup( nameof( AllowSprinting ), Label = SPRINTING )]
	public virtual bool AllowSprinting { get; set; } = true;

	/// <summary>
	/// If true: sprinting is on when not held and is toggled off instead.
	/// </summary>
	[Property]
	[Title( "Starts Enabled" )]
	[Feature( PAWN ), Order( SPRINT_ORDER )]
	[ToggleGroup( nameof( AllowSprinting ), Label = SPRINTING )]
	public virtual bool SprintDefault { get; set; } = false;

	[Property]
	[ToggleGroup( nameof( AllowSprinting ) )]
	[Feature( PAWN ), Order( SPRINT_ORDER )]
	[Range( 0f, 3f, clamped: false ), Step( 0.01f )]
	public virtual float SprintMultiplier { get; set; } = 1.5f;

	/// <summary>
	/// Should the owner be able to crouch?
	/// </summary>
	[Property]
	[Feature( PAWN ), Order( DUCKING_ORDER )]
	[ToggleGroup( nameof( AllowDucking ), Label = DUCKING )]
	public virtual bool AllowDucking { get; set; } = true;

	[Property]
	[Title( "Move Speed (Ducked)" )]
	[Range( 0f, 1000f, clamped: false )]
	[ToggleGroup( nameof( AllowDucking ) )]
	[Feature( PAWN ), Order( DUCKING_ORDER )]
	public virtual float MoveSpeedDucked { get; set; } = 120f;

	/// <summary>
	/// Should the owner be able to jump?
	/// </summary>
	[Property]
	[Feature( PAWN ), Order( JUMPING_ORDER )]
	[ToggleGroup( nameof( AllowJumping ), Label = JUMPING )]
	public virtual bool AllowJumping { get; set; } = true;

	[Property]
	[ToggleGroup( nameof( AllowJumping ) )]
	[ShowIf( nameof( HasJumpButton ), true )]
	[Feature( PAWN ), Order( JUMPING_ORDER )]
	public virtual float JumpSpeed { get; set; } = 400f;

	public virtual Vector3 GetJumpVelocity()
		=> WorldRotation.Up * JumpSpeed;

	[Property]
	[Title( "Standing Height" )]
	[Feature( VIEW ), Group( EYE_POS ), Order( EYEPOS_ORDER )]
	public virtual float EyeHeightStand { get; set; } = 64f;

	[Property]
	[Title( "Ducked Height" )]
	[Feature( VIEW ), Group( EYE_POS ), Order( EYEPOS_ORDER )]
	public virtual float EyeHeightDuck { get; set; } = 32f;

	[Sync]
	public bool IsDucking { get; set; }

	[Sync]
	public bool IsSprinting { get; set; }

	public override Vector3 GetLocalEyeTargetPosition()
		=> Vector3.Up * (IsDucking ? EyeHeightDuck : EyeHeightStand);

	protected override void Move( in float deltaTime )
	{
		PreMove( in deltaTime );
		PostMove( in deltaTime );
	}

	protected override void PreMove( in float deltaTime ) { }

	protected override void PostMove( in float deltaTime ) { }

	public virtual float GetSprintSpeed( in float? baseSpeed = null )
		=> (baseSpeed ?? MoveSpeed) * SprintMultiplier;

	public override float GetMovementSpeed()
	{
		if ( !AllowMovement || Pawn?.IsAlive is not true )
			return 0f;

		var moveSpeed = MoveSpeed;

		if ( ShouldSprint )
			moveSpeed = GetSprintSpeed( moveSpeed );

		if ( !HasDuckButton )
			return moveSpeed;

		return LocalEyePosition.z.Remap( EyeHeightDuck, EyeHeightStand, MoveSpeedDucked, MoveSpeed );
	}
}
