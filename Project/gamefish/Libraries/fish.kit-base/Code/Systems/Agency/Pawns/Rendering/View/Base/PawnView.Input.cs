namespace GameFish;

partial class PawnView
{
	/// <summary>
	/// If true: you can press buttons to cycle through perspectives.
	/// </summary>
	[Property]
	[Feature( MODES ), Order( MODES_ORDER )]
	[ToggleGroup( nameof( AllowCyclingMode ), Label = CYCLING )]
	public virtual bool AllowCyclingMode { get; set; } = false;

	/// <summary>
	/// The button that selects the next mode.
	/// </summary>
	[InputAction]
	[Property, Title( "Forward" )]
	[Feature( MODES ), Order( MODES_ORDER )]
	[ToggleGroup( nameof( AllowCyclingMode ) )]
	public virtual string CycleModeForwardAction { get; set; } = "View";

	/// <summary>
	/// The button that selects the previous mode.
	/// </summary>
	[InputAction]
	[Property, Title( "Backward" )]
	[Feature( MODES ), Order( MODES_ORDER )]
	[ToggleGroup( nameof( AllowCyclingMode ) )]
	public virtual string CycleModeBackwardAction { get; set; }

	public virtual bool TryAiming( in float deltaTime )
		=> true;

	/// <summary>
	/// Allows cycling of perspective modes.
	/// </summary>
	protected virtual void HandleInput()
	{
		DoModeCycling();
	}

	protected virtual void DoModeCycling()
	{
		if ( !AllowCyclingMode )
			return;

		if ( !string.IsNullOrEmpty( CycleModeForwardAction ) )
			if ( Input.Pressed( CycleModeForwardAction ) )
				CycleMode( 1 );

		if ( !string.IsNullOrEmpty( CycleModeBackwardAction ) )
			if ( Input.Pressed( CycleModeBackwardAction ) )
				CycleMode( -1 );
	}
}
