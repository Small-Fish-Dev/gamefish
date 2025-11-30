namespace GameFish;

partial class Library
{
    /// <returns> If this scene is valid and in editor mode(not a game/playing scene). </returns>
    public static bool InEditor( this Scene sc )
    {
        return sc.IsValid() && (sc.IsEditor || sc is PrefabScene);
    }

    /// <returns> If this object is valid and loaded in an editor scene(not in game/play mode). </returns>
    public static bool InEditor( this GameObject obj )
    {
        return obj.IsValid() && obj.Scene.InEditor();
    }

    /// <returns> If this component is valid and loaded in an editor scene(not in game/play mode). </returns>
    public static bool InEditor( this Component c )
    {
        return c.IsValid() && c.Scene.InEditor();
    }
}
