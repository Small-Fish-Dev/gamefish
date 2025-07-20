namespace GameFish;

public static partial class GameFish
{
	/// <returns> If this component is valid and loaded in an editor scene(not in game/play mode). </returns>
	public static bool InEditor( this Component c )
	{
		return c.IsValid() && c.Scene.IsValid() && c.Scene.IsEditor;
	}

	/// <returns> If this component is valid and loaded in a play mode scene(not scene/prefab editor). </returns>
	public static bool InGame( this Component c )
	{
		return c.IsValid() && c.Scene.IsValid() && !c.Scene.IsLoading && !c.Scene.IsEditor;
	}
}
