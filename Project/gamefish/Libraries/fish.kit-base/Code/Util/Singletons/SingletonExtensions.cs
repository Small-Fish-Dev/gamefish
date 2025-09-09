namespace GameFish;

public static partial class GameFish
{
	/// <summary>
	/// Retrieves the first valid instance of a component in the scene and caches it. <br />
	/// Call this on the private static instance in your public property's getter to auto-assign it.
	/// </summary>
	/// <param name="instance"></param>
	/// <param name="isOwned"> If true: only consider network-owned components. </param>
	/// <param name="allowEditor"> If true: editor scenes can find/use singletons. </param>
	/// <returns> Null if in the editor or no instance was found. </returns>
	public static T GetSingleton<T>( this T instance, bool isOwned = false, bool allowEditor = false ) where T : Component
	{
		if ( instance.IsValid() )
			return instance;

		var activeScene = Game.ActiveScene;

		if ( !activeScene.IsValid() || (activeScene.InEditor() && !allowEditor) )
			return null;

		instance = isOwned
			? activeScene.GetAllComponents<T>().FirstOrDefault( c => c.IsOwner() )
			: activeScene.GetAllComponents<T>().FirstOrDefault();

		// Should always be valid or null, never destroyed.
		return instance.IsValid() ? instance : null;
	}
}
