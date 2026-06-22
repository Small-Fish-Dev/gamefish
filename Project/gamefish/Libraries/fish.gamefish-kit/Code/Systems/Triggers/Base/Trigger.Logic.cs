namespace GameFish;

partial class Trigger : IToggle
{
	/// <summary>
	/// Print debug logs related to logic?
	/// </summary>
	[Property]
	[Title( "Logging (logic)" )]
	[Order( TRIGGER_DEBUG_ORDER - 1 )]
	[Feature( TRIGGER ), Group( DEBUG )]
	public bool DebugLogicLogging { get; set; } = false;

	/// <summary>
	/// If enabled: this trigger is active.
	/// </summary>
	[Sync]
	[Property]
	[EnumButtonGroup]
	[Title( "State" )]
	[Feature( TRIGGER )]
	[Order( TRIGGER_ORDER )]
	[WideMode( HasLabel = true )]
	public ToggleState ToggleState
	{
		get => _toggleState;
		set
		{
			if ( _toggleState == value )
				return;

			_toggleState = value;
			OnSetToggleState( value );
		}
	}

	protected ToggleState _toggleState = ToggleState.Enabled;

	public bool IsOn
	{
		get => ToggleState is ToggleState.Enabled;
		set => ToggleState = value.ToState();
	}

	protected virtual void OnSetToggleState( in ToggleState state )
	{
		if ( DebugLogicLogging )
			this.Log( $"Toggled to: {state}" );

		UpdateColliders();
	}

	public virtual bool CanToggle( in bool isOn )
	{
		if ( GameObject.IsDestroyed() )
			return false;

		return IsOn != isOn;
	}

	public virtual void Toggle( in bool isOn )
	{
		if ( !CanToggle( in isOn ) )
			return;

		IsOn = isOn;
	}
}
