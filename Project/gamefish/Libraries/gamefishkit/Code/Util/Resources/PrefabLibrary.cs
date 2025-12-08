namespace GameFish;

/// <summary>
/// The prefab library made by ceitine at Small Fish. <br />
/// Copied and altered from the Blocks 'n Bullets repo(currently MIT).
/// </summary>
public static class PrefabLibrary
{
	public static IReadOnlyDictionary<PrefabFile, GameObject> All => _all ?? Refresh();
	private static Dictionary<PrefabFile, GameObject> _all;

	public static IReadOnlyDictionary<PrefabFile, GameObject> Refresh()
	{
		var prefabs = ResourceLibrary.GetAll<PrefabFile>().ToArray();

		_all = [];

		foreach ( var prefab in prefabs )
		{
			if ( !prefab.IsValid() )
				continue;

			var prefabObject = GameObject.GetPrefab( prefab.ResourcePath );

			if ( prefabObject is not null )
				_all[prefab] = prefabObject;
		}

		return _all;
	}

	/// <summary>
	/// Finds all components of a type in all prefabs.
	/// </summary>
	public static IEnumerable<T> FindComponents<T>()
		where T : Component
	{
		if ( All is null )
			yield break;

		foreach ( var (prefab, obj) in All )
		{
			var components = obj?.Components?.GetAll();

			var found = components?.FirstOrDefault( c => c is T ) as T;

			if ( found is not null )
				yield return found;
		}
	}

	/// <summary>
	/// Finds all prefabs that contain a component.
	/// This is probably slow so avoid using it too much.
	/// </summary>
	public static IEnumerable<(GameObject PrefabScene, PrefabFile Prefab)> FindByComponent<T>()
		where T : Component
	{
		if ( All == null )
			yield break;

		foreach ( var (prefab, obj) in All )
		{
			if ( !prefab.IsValid() )
				continue;

			var components = obj?.Components?.GetAll();

			if ( components?.Any( c => c is T ) is true )
				yield return (obj, prefab);
		}
	}

	/// <summary>
	/// Finds all prefab/<typeparamref name="T"/> pairs.
	/// This is probably slow so avoid using it too much.
	/// </summary>
	public static IEnumerable<(PrefabFile Prefab, T Component)> FindPrefabComponents<T>()
		where T : Component
	{
		if ( All == null )
			yield break;

		foreach ( var (prefab, obj) in All )
		{
			if ( !prefab.IsValid() )
				continue;

			var components = obj?.Components?.GetAll();
			var found = components?.FirstOrDefault( c => c is T ) as T;

			if ( found is not null )
				yield return (prefab, found);
		}
	}

	/// <summary>
	/// Tries to find a <see cref="GameObject"/> by <see cref="PrefabFile"/> path.
	/// </summary>
	public static bool TryGetByPath( string path, out GameObject prefab )
	{
		prefab = GameObject.GetPrefab( path );
		return prefab.IsValid();
	}
}
