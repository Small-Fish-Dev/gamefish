namespace GameFish;

partial class PawnView
{
	public const string AIMING = "Aiming";
	public const string COLLISION = "Collision";

	/// <summary>
	/// If true: you can press buttons to cycle through perspectives.
	/// </summary>
	[Property]
	[Title( "Allow" )]
	[Feature( INPUT ), Group( CYCLING )]
	public virtual bool AllowCyclingMode { get; set; } = false;

	/// <summary>
	/// The button to press to select the next mode.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Forward" )]
	[Feature( INPUT ), Group( CYCLING )]
	[ShowIf( nameof( AllowCyclingMode ), true )]
	public virtual string CycleModeForwardAction { get; set; } = "View";

	/// <summary>
	/// The button to press to select the next mode.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Backward" )]
	[Feature( INPUT ), Group( CYCLING )]
	[ShowIf( nameof( AllowCyclingMode ), true )]
	public virtual string CycleModeBackwardAction { get; set; }

	[Property]
	[Feature( INPUT ), Group( AIMING )]
	public virtual bool PitchClamping { get; set; } = true;

	[Property]
	[Range( 0, 180 )]
	[Feature( INPUT ), Group( AIMING )]
	[ShowIf( nameof( PitchClamping ), true )]
	public virtual FloatRange PitchRange { get; set; } = new( -89.9f, 89.9f );

	/// <summary>
	/// An Euler angle representing where this view is looking.
	/// </summary>
	[Property]
	[Feature( INPUT ), Group( AIMING )]
	public virtual Angles EyeAngles
	{
		get => ViewRotation;
		set => ViewRotation = PitchClamping
			? value.WithPitch( value.pitch.Clamp( PitchRange ) )
			: value;
	}

	/// <summary>
	/// Allows cycling of perspective modes.
	/// </summary>
	protected virtual void HandleInput()
	{
		DoAiming();
		DoModeCycling();
	}

	protected virtual void DoAiming()
	{
		var angLook = Input.AnalogLook;

		if ( PitchClamping )
		{
			Angles angAim = EyeAngles;

			angAim.pitch = (angAim.pitch + angLook.pitch).Clamp( PitchRange );
			angAim.yaw += angLook.yaw;

			angAim.roll = angAim.roll.LerpDegreesTo( 0f, Time.Delta * 10f );

			EyeAngles = angAim;
		}
		else
		{
			var rAim = ViewRotation;
			var rInverse = rAim.Inverse;

			rAim *= Rotation.FromAxis( rInverse.Up, angLook.yaw );
			rAim *= Rotation.FromPitch( angLook.pitch );

			rAim *= Rotation.FromRoll( -rAim.Roll() * Time.Delta * 10f );

			EyeAngles = rAim;
		}
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
