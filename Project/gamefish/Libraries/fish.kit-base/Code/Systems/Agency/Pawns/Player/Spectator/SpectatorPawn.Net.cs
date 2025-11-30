namespace GameFish;

partial class SpectatorPawn
{
	/// <summary>
	/// Always destroy spectator pawns upon losing them.
	/// </summary>
	protected override NetworkOrphaned NetworkOrphanedModeDefault => NetworkOrphaned.Destroy;
}
