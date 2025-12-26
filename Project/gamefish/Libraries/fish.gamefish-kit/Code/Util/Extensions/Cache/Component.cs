namespace GameFish;

partial class Library
{
	/// <summary>
	/// Lets you quickly use a property's get/set to auto-cache a component.
	/// </summary>
	/// <param name="c"> The <typeparamref name="TComp"/> field. </param>
	/// <param name="obj"> The object to find the component on. </param>
	/// <param name="findMode"> If this is enabled, on self or a parent and so on. </param>
	/// <returns> The cached <typeparamref name="TComp"/>(if found). </returns>
	public static TComp GetCached<TComp>( this TComp c, GameObject obj, FindMode findMode = FindMode.EnabledInSelf )
		where TComp : Component
	{
		if ( !obj.IsValid() )
			return null;

		if ( c.IsValid() )
			return c;

		c = obj.Components?.Get<TComp>( findMode );

		return c;
	}
}
