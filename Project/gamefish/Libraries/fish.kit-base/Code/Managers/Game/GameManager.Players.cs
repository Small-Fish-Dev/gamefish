namespace GameFish;

partial class GameManager
{
	public virtual Transform FallbackSpawnPoint { get; set; } = global::Transform.Zero;

	/// <param name="agent"> Probably a <see cref="Client"/>. </param>
	/// <returns> Where they should spawn. </returns>
	public virtual Transform FindSpawnPoint( Agent agent )
	{
		// Allow the state to specify where we spawn.
		if ( State.IsValid() && State.FindSpawnPoint( agent ) is Transform tState )
			return tState;

		// Fall back to the built-in spawn points.
		var allSpawnPoints = Scene?.GetAll<SpawnPoint>();

		if ( allSpawnPoints is null || !allSpawnPoints.Any() )
			return FallbackSpawnPoint;

		return allSpawnPoints.PickRandom()?.WorldTransform ?? FallbackSpawnPoint;
	}
}
