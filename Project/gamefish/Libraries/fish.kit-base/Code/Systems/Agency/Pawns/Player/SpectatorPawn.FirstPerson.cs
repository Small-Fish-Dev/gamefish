namespace GameFish;

public partial class SpectatorPawn
{
	protected const string GROUP_FIRST_PERSON = "First Person";

	[Property]
	[Feature( FEATURE_SPECTATOR )]
	[ToggleGroup( nameof( HasFirstPersonMode ), Label = GROUP_FIRST_PERSON )]
	protected bool HasFirstPersonMode { get; set; } = true;

	[Property]
	[Feature( FEATURE_SPECTATOR )]
	[ToggleGroup( nameof( HasFirstPersonMode ) )]
	public bool ShowViewModel { get; set; } = true;

	protected void FirstPersonUpdate()
	{
		if ( !Target.IsValid() ) return;

		if ( Scene?.Camera is not { } camera ) return;

		camera.WorldPosition = Target.EyePosition;
		camera.WorldRotation = Target.EyeRotation;
		DebugOverlay.Line( Target.EyePosition, Target.EyeForward * 100 );
	}
}
