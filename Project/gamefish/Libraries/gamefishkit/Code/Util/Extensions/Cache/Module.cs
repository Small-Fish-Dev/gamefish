namespace GameFish;

partial class Library
{
	/// <summary>
	/// Lets you quickly use a property's get/set to auto-cache an entity's module.
	/// </summary>
	/// <param name="m"> The <typeparamref name="TModule"/> field. </param>
	/// <param name="ent"> The entity that <typeparamref name="TModule"/> is targeting. </param>
	/// <returns> The cached <typeparamref name="TModule"/>(if found). </returns>
	public static TModule GetCached<TModule>( this TModule m, ModuleEntity ent )
		where TModule : Module
	{
		if ( !ent.IsValid() )
			return null;

		if ( m.IsValid() && m.Parent == ent )
			return m;

		return m = ent.GetModule<TModule>();
	}
}
