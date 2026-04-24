namespace GameFish;

partial class Library
{
	/// <summary>
	/// Lets you quickly use a property's get/set to auto-cache a component.
	/// </summary>
	/// <param name="obj"> The object to find the component on. </param>
	/// <param name="field"> The <typeparamref name="TComp"/> field. </param>
	/// <param name="findMode"> If it's enabled, on self or a parent and so on. </param>
	/// <returns> The cached <typeparamref name="TComp"/>(if found). </returns>
	public static TComp GetCached<TComp>( this GameObject obj, ref TComp @field, FindMode findMode = FindMode.EnabledInSelf )
		where TComp : Component
	{
		if ( !obj.IsValid() )
			return null;

		if ( @field.IsValid() )
			return @field;

		@field = obj.Components?.Get<TComp>( findMode );

		return @field.AsValid();
	}

	/// <summary>
	/// Lets you quickly use a property's get/set to auto-cache an entity's module.
	/// </summary>
	/// <param name="ent"> The entity that <typeparamref name="TModule"/> is targeting. </param>
	/// <param name="field"> The <typeparamref name="TModule"/> field. </param>
	/// <returns> The cached <typeparamref name="TModule"/>(if found). </returns>
	public static TModule GetCached<TModule>( this ModuleEntity ent, ref TModule @field )
		where TModule : Module
	{
		if ( !ent.IsValid() )
			return null;

		if ( @field.IsValid() && @field.Parent == ent )
			return @field;

		@field = ent.GetModule<TModule>();

		return @field.AsValid();
	}
}
