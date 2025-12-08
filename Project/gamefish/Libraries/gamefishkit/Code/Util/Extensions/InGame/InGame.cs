namespace GameFish;

partial class Library
{
	/// <returns> If this scene is valid and loaded in play mode(not a prefab/editor scene). </returns>
	public static bool InGame( this Scene sc )
	{
		return sc.IsValid() && !sc.IsEditor && sc is not PrefabScene;
	}

	/// <returns> If this object is valid and loaded in a play mode scene(not scene/prefab editor). </returns>
	public static bool InGame( this GameObject obj )
	{
		return obj.IsValid() && obj.Scene.InGame();
	}

	/// <returns> If this component is valid and loaded in a play mode scene(not scene/prefab editor). </returns>
	public static bool InGame( this Component c )
	{
		return c.IsValid() && c.Scene.InGame();
	}
}
