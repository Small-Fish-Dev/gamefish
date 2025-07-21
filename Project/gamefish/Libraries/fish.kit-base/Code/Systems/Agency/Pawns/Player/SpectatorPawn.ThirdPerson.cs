namespace GameFish;

public partial class SpectatorPawn
{
	protected const string GROUP_THIRD_PERSON = "Third Person";

	[Property]
	[Feature( FEATURE_SPECTATOR )]
	[ToggleGroup( nameof( HasThirdPersonMode ), Label = GROUP_THIRD_PERSON )]
	protected bool HasThirdPersonMode { get; set; }

	[Property]
	[Feature( FEATURE_SPECTATOR )]
	[ToggleGroup( nameof( HasThirdPersonMode ) )]
	public FloatRange MaxDistance { get; set; } = new( 50f, 500f );
}
