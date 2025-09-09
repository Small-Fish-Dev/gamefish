namespace GameFish;

public static partial class GameFish
{
    /// <returns> If this scene is valid and in editor mode(not a game/playing scene). </returns>
    public static bool InEditor( this Scene sc )
    {
        return sc.IsValid() && (sc.IsEditor || sc is PrefabScene);
    }

    /// <returns> If this component is valid and loaded in an editor scene(not in game/play mode). </returns>
    public static bool InEditor( this Component c )
    {
        return c.IsValid() && c.Scene.InEditor();
    }

    /// <returns> If this scene is valid and loaded in play mode(not a prefab/editor scene). </returns>
    public static bool InGame( this Scene sc )
    {
        return sc.IsValid() && !sc.IsLoading && !sc.IsEditor && sc is not PrefabScene;
    }

    /// <returns> If this component is valid and loaded in a play mode scene(not scene/prefab editor). </returns>
    public static bool InGame( this Component c )
    {
        return c.IsValid() && c.Scene.InGame();
    }
}
