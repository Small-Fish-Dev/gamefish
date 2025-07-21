namespace GameFish;

public partial class SpectatorPawn
{
	protected const string GROUP_FREE_ROAM = "Free Roam";

	[Property]
	[Feature( FEATURE_SPECTATOR )]
	[ToggleGroup( nameof( HasFreeRoamMode ), Label = GROUP_FREE_ROAM )]
	protected bool HasFreeRoamMode { get; set; } = true;

	[Property]
	[Feature( FEATURE_SPECTATOR )]
	[ToggleGroup( nameof( HasFreeRoamMode ) )]
	[Range( 1, 1000, clamped: false )]
	public float Speed { get; set; } = 10f;

	[Property]
	[Feature( FEATURE_SPECTATOR )]
	[ToggleGroup( nameof( HasFreeRoamMode ) )]
	[Range( 1, 1000, clamped: false )]
	public float? SprintSpeed { get; set; } = 20f;

	/// <summary>
	/// Should the free roam controller limit itself to the <see cref="Bounds"/>.
	/// </summary>
	[Property]
	[Feature( FEATURE_SPECTATOR )]
	[ToggleGroup( nameof( HasFreeRoamMode ) )]
	public bool HasBounds { get; set; } = false;

	/// <summary>
	/// Hard bounds for the free roam mode. Uses global transform.
	/// </summary>
	[Property]
	[Feature( FEATURE_SPECTATOR )]
	[ShowIf( nameof( HasBounds ), true )]
	[ToggleGroup( nameof( HasFreeRoamMode ) )]
	public BBox Bounds { get; set; }
}
