namespace GameFish;

public static class Prefab
{
	/// <summary>
	/// Safely reference a prefab without breaking the entire project if it's missing.
	/// </summary>
	public static GameObject Get( string path )
	{
		path ??= "";

		var prefab = GameObject.GetPrefab( path );

		if ( prefab is null )
		{
			Log.Warning( "[Prefab] Couldn't find prefab with path: " + path );
			return null;
		}

		return prefab;
	}

	/// <summary>
	/// Safely reference a <see cref="PrefabFile"/> without breaking the entire project if it's missing.
	/// </summary>
	public static PrefabFile GetFile( string path )
	{
		path ??= "";

		var prefab = ResourceLibrary.Get<PrefabFile>( path );

		if ( prefab is null )
		{
			Log.Warning( "[Prefab] Couldn't find prefab file with path: " + path );
			return null;
		}

		return prefab;
	}

	/// <returns> If the prefab was valid and spawned a valid game object. </returns>
	public static bool TrySpawn( this PrefabFile prefabFile, out GameObject go )
	{
		if ( !prefabFile.IsValid() )
		{
			go = null;
			return false;
		}

		go = GameObject.Clone( prefabFile );

		if ( go.IsValid() )
		{
			go.Enabled = true;
			return true;
		}

		return false;
	}

	/// <returns> If the prefab was valid and spawned a valid game object at the position. </returns>
	public static bool TrySpawn( this PrefabFile prefabFile, Transform t, out GameObject go )
	{
		if ( prefabFile.TrySpawn( out var goSpawned ) )
		{
			go = goSpawned;
			go.WorldTransform = t;
			return true;
		}

		go = null;
		return false;
	}

	/// <returns> If the prefab was valid and spawned a valid game object at the position. </returns>
	public static bool TrySpawn( this PrefabFile prefabFile, Vector3 pos, out GameObject go )
	{
		if ( prefabFile.TrySpawn( out var goSpawned ) )
		{
			go = goSpawned;
			go.WorldPosition = pos;
			return true;
		}

		go = null;
		return false;
	}
}
