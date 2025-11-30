namespace GameFish;

partial class SpectatorPawn
{
	public const string AIMING = "Aiming";
	public const int AIMING_ORDER = 1000;

	/// <summary>
	/// Should the owner's look input rotate their eye angles?
	/// </summary>
	[Property]
	[Feature( SPECTATOR ), Group( VIEW ), Order( AIMING_ORDER )]
	[ToggleGroup( value: nameof( AllowAiming ), Label = "Aiming" )]
	public virtual bool AllowAiming { get; set; } = true;

	[Property]
	[ToggleGroup( nameof( AllowAiming ) )]
	[Feature( SPECTATOR ), Group( VIEW ), Order( AIMING_ORDER )]
	public virtual bool PitchClamping { get; set; } = true;

	[Property]
	[Range( 0, 180 )]
	[ToggleGroup( nameof( AllowAiming ) )]
	[Feature( SPECTATOR ), Group( VIEW ), Order( AIMING_ORDER )]
	[ShowIf( nameof( PitchClamping ), true )]
	public virtual FloatRange PitchRange { get; set; } = new( -89.9f, 89.9f );

	public override Vector3 EyePosition
	{
		get => View?.ViewPosition ?? WorldPosition;
		set
		{
			if ( View is var view && view.IsValid() )
				view.ViewPosition = value;
		}
	}

	public override Rotation EyeRotation
	{
		get => View?.ViewRotation ?? WorldRotation;
		set
		{
			if ( View is var view && view.IsValid() )
				view.ViewRotation = value;
		}
	}

	protected override void DoAiming( in float deltaTime )
	{
		if ( View is not SpectatorView view || !view.IsValid() )
			return;

		if ( !view.TryAiming( in deltaTime ) )
			return;

		var angLook = Input.AnalogLook;

		if ( PitchClamping )
		{
			Angles angAim = EyeRotation;

			angAim.pitch = (angAim.pitch + angLook.pitch).Clamp( PitchRange );
			angAim.yaw += angLook.yaw;

			angAim.roll = angAim.roll.LerpDegreesTo( 0f, Time.Delta * 10f );

			EyeRotation = angAim;
		}
		else
		{
			var rAim = EyeRotation;
			var rInverse = rAim.Inverse;

			rAim *= Rotation.FromAxis( rInverse.Up, angLook.yaw );
			rAim *= Rotation.FromPitch( angLook.pitch );

			rAim *= Rotation.FromRoll( -rAim.Roll() * deltaTime * 10f );

			EyeRotation = rAim;
		}
	}
}
