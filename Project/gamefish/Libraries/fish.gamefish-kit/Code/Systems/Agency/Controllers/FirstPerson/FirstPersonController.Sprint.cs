namespace GameFish;

partial class FirstPersonController
{
	/// <summary>
	/// Should this be able to toggle increasing its speed?
	/// </summary>
	[Property]
	[Feature( CONTROLLER ), Order( SPRINT_ORDER )]
	[ToggleGroup( nameof( SprintingEnabled ), Label = SPRINTING )]
	public virtual bool SprintingEnabled { get; set; } = true;

	/// <summary>
	/// The input to hold for sprinting.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Input" )]
	[Feature( CONTROLLER ), Order( SPRINT_ORDER )]
	[ToggleGroup( nameof( SprintingEnabled ) )]
	public string SprintInput { get; set; } = "Run";

	/// <summary>
	/// The move speed multiplier applied while sprinting.
	/// </summary>
	[Property]
	[Feature( CONTROLLER ), Order( SPRINT_ORDER )]
	[ToggleGroup( nameof( SprintingEnabled ) )]
	[Range( 0f, 3f, clamped: false ), Step( 0.01f )]
	public virtual float SprintMultiplier { get; set; } = 1.5f;

	/// <summary>
	/// If true: sprinting is on when not held and is toggled off instead.
	/// </summary>
	[Property]
	[Title( "Starts Enabled" )]
	[Feature( CONTROLLER ), Order( SPRINT_ORDER )]
	[ToggleGroup( nameof( SprintingEnabled ) )]
	public virtual bool IsSprintDefault { get; set; } = false;

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

	protected virtual void OnSetIsSprinting( in bool isSprinting )
	{
	}

	/// <returns> If sprinting is currently intended. </returns>
	protected virtual bool IsWishingSprint()
		=> Input.Down( SprintInput ) == !IsSprintDefault;

	/// <returns> If sprinting should be active. </returns>
	protected virtual bool ShouldSprint()
	{
		if ( !SprintingEnabled )
			return false;

		if ( !IsGrounded )
			return false;

		return IsWishingSprint();
	}

	public virtual float GetSprintSpeed( in float? speed = null )
		=> (speed ?? MoveSpeed) * SprintMultiplier;
}
