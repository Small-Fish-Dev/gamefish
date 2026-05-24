namespace GameFish;

public static class Prefab
{
	/// <summary>
	/// Safely reference a prefab without breaking the entire project if it's missing.
	/// </summary>
	/// <param name="path"> The file path to the prefab. </param>
	/// <param name="warn"> Print a warning log upon failure? </param>
	/// <returns> The object of the prefab(or null). </returns>
	public static GameObject Get( string path, in bool warn = true )
	{
		path ??= "";

		var prefab = GameObject.GetPrefab( path );

		if ( prefab is null )
		{
			if ( warn )
				Log.Warning( $"[{nameof( Prefab )}] Couldn't find prefab with path: " + path );

			return null;
		}

		return prefab;
	}

	/// <summary>
	/// Safely reference a prefab without breaking the entire project if it's missing.
	/// </summary>
	/// <param name="path"> The file path to the prefab. </param>
	/// <param name="warn"> Print a warning log upon failure? </param>
	/// <returns> The resource of the prefab(or null). </returns>
	public static PrefabFile GetFile( string path, in bool warn = true )
	{
		path ??= "";

		var prefab = ResourceLibrary.Get<PrefabFile>( path );

		if ( prefab is null )
		{
			if ( warn )
				Log.Warning( $"[{nameof( Prefab )}] Couldn't find prefab file with path: " + path );

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
	public static bool TrySpawn( this PrefabFile prefabFile, in Transform t, out GameObject go )
	{
		if ( prefabFile.TrySpawn( out go ) )
		{
			go.WorldTransform = t;
			return true;
		}

		go = null;
		return false;
	}

	/// <returns> If the prefab was valid and spawned a valid game object at the position. </returns>
	public static bool TrySpawn( this PrefabFile prefabFile, in Vector3 pos, out GameObject go )
	{
		if ( prefabFile.TrySpawn( out go ) )
		{
			go.WorldPosition = pos;
			return true;
		}

		go = null;
		return false;
	}

	/// <returns> If the prefab was valid and spawned a valid game object at the position/rotation. </returns>
	public static bool TrySpawn( this PrefabFile prefabFile, in Vector3 pos, in Rotation r, out GameObject go )
	{
		if ( prefabFile.TrySpawn( out go ) )
		{
			go.WorldPosition = pos;
			go.WorldRotation = r;
			return true;
		}

		go = null;
		return false;
	}
}
