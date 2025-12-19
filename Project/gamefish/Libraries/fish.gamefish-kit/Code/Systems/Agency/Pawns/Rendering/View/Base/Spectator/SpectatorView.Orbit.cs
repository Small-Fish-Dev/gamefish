namespace GameFish;

partial class SpectatorView
{
	public const string ORBITING = "Orbiting";

	[Property]
	[Title( "Enabled" )]
	[Feature( INPUT ), Group( ORBITING )]
	public virtual bool AllowOrbiting { get; set; } = true;

	/// <summary>
	/// The period in which the camera rotation stays where you moved it to.
	/// </summary>
	[Property]
	[Title( "Reset Delay" )]
	[Range( 0, 10, clamped: false )]
	[Feature( INPUT ), Group( ORBITING )]
	[ShowIf( nameof( AllowOrbiting ), true )]
	public virtual float OrbitingResetDelay { get; set; } = 3f;

	public RealTimeUntil? OrbitingReset { get; set; }

	public virtual bool IsOrbiting => AllowOrbiting && OrbitingReset.HasValue && !OrbitingReset.Value;

	public override Rotation PawnEyeRotation => IsOrbiting ? ViewRotation : base.PawnEyeRotation;

	public void StopOrbiting()
	{
		OrbitingReset = null;
		StartTransition();
	}

	public override void FrameSimulate( in float deltaTime )
	{
		base.FrameSimulate( deltaTime );

		if ( !IsOrbiting && OrbitingReset.HasValue )
			StopOrbiting();
	}

	public override bool TryAiming( in float deltaTime )
	{
		if ( IsSpectating && Mode?.InFirstPerson() is true )
		{
			if ( IsOrbiting )
				StopOrbiting();

			return false;
		}

		// Shit gets wonky if you try to aim during a transition.
		// TODO: Fix that or scale input depending on "velocity".
		if ( PreviousOffset.HasValue && TransitionFraction < 0.9f )
			return false;

		if ( AllowOrbiting && Client.TryGetLocalAim( out var aim ) )
			if ( aim.ToRotation().Angle().AlmostEqual( 0f ) )
				OrbitingReset = OrbitingResetDelay;

		return true;
	}

	/*
	protected override void DoAiming()
	{
		if ( IsSpectating && Mode is Perspective.FirstPerson )
		{
			if ( OrbitingReset.HasValue )
				StopOrbiting();

			return;
		}

		// Shit gets wonky if you try to aim during a transition.
		// TODO: Fix that or scale input depending on "velocity".
		if ( PreviousOffset.HasValue && TransitionFraction < 0.9f )
			return;

		if ( AllowOrbiting && !Client.TryGetLocalAim.AsVector3().AlmostEqual( Vector3.Zero ) )
			OrbitingReset = OrbitingResetDelay;

		base.DoAiming();
	}
	*/
}
