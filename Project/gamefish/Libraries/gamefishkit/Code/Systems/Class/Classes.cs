namespace GameFish;

/// <summary>
/// Allows you to quickly find prefabs from their class
/// Looks up and caches a table of prefabs with <see cref="Class"/> derived components on their root.
/// </summary>
public static partial class Classes
{
	public static Dictionary<string, ClassData> All => _all ??= Find<Class>();
	private static Dictionary<string, ClassData> _all;

	/// <summary>
	/// Registers fresh copies of classes.
	/// </summary>
	public static void Refresh()
	{
		_all?.Clear();
		_all ??= [];

		foreach ( var (id, data) in Find<Class>() )
			_all[id] = data;

		if ( Game.IsEditor )
			Print.InfoFrom( typeof( Classes ), $"Refreshing. Found {_all.Count} classes." );
	}

	public static Dictionary<string, ClassData> Find<TClass>()
		where TClass : Class
	{
		var dict = new Dictionary<string, ClassData>();

		var prefabClasses = PrefabLibrary.FindPrefabComponents<Class>() ?? [];

		foreach ( var (Prefab, Class) in prefabClasses )
		{
			if ( !Class.IsValid() || !Class.IsClass )
				continue;

			var id = Class?.ClassId;

			if ( !id.IsBlank() )
				dict[id] = new ClassData( id, Prefab, Class );
		}

		return dict;
	}
}
