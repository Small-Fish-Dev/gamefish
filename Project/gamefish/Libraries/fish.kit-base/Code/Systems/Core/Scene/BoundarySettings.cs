namespace GameFish;

public partial struct BoundarySettings
{
	/// <summary>
	/// How players are treated.
	/// </summary>
	[Group( PLAYERS ), Order( 1 )]
	public BoundaryResponse Players { get; set; } = BoundaryResponse.Teleport;

	/// <summary>
	/// Non-player character response.
	/// </summary>
	[Group( ACTORS ), Order( 2 )]
	public BoundaryResponse Actors { get; set; } = BoundaryResponse.Destroy;

	/// <summary>
	/// Do this to physics objects.
	/// </summary>
	[Group( PHYSICS ), Order( 3 )]
	public BoundaryResponse Physics { get; set; } = BoundaryResponse.Destroy;

	public BoundarySettings() { }

	public BoundarySettings( BoundaryResponse pl, BoundaryResponse actor, BoundaryResponse phys )
	{
		Players = pl;
		Actors = actor;
		Physics = phys;
	}
}
