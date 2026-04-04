namespace GameFish;

public interface IClientSpawnedEvent : ISceneEvent<IClientSpawnedEvent>
{
	void OnClientSpawned( Connection connection, Client client );
}
