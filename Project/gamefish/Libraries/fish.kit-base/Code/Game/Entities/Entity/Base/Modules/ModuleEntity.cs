namespace GameFish;

/// <summary>
/// An entity that supports modules.
/// </summary>
public partial class ModuleEntity : BaseEntity
{
	[Property, ReadOnly]
	[Feature( ENTITY ), Group( MODULES )]
	public List<Module> Modules { get; set; }

	protected static bool IsModule<TMod>( Module m ) where TMod : Module
		=> m.IsValid() && m is TMod;

	/// <summary>
	/// Quickly checks the cache to see if we have this module.
	/// </summary>
	/// <typeparam name="TMod"> The specific module. </typeparam>
	/// <returns> If the module exists. </returns>
	public bool HasModule<TMod>() where TMod : Module
	{
		var mType = typeof( TMod );
		return GetModules().Any( IsModule<TMod> );
	}

	/// <summary>
	/// Always gives you a cached list of modules. Registers them if that hasn't been done yet.
	/// </summary>
	/// <returns> The cached list of modules. </returns>
	public IEnumerable<Module> GetModules()
	{
		if ( Modules is null )
			RegisterModules();

		return Modules ??= [];
	}

	/// <typeparam name="TMod"> The specific module. </typeparam>
	public TMod GetModule<TMod>() where TMod : Module
	{
		var mType = typeof( TMod );
		return GetModules().FirstOrDefault( IsModule<TMod> ) as TMod;
	}

	/// <typeparam name="TMod"> The specific module. </typeparam>
	/// <returns> Every module of this type(if any, never null). </returns>
	public IEnumerable<TMod> GetModules<TMod>() where TMod : Module
	{
		return GetModules()
			.Select( m => m as TMod )
			.Where( m => m.IsValid() );
	}

	/// <typeparam name="TMod"> The specific module. </typeparam>
	public bool TryGetModule<TMod>( out TMod m ) where TMod : Module
	{
		m = GetModule<TMod>();
		return m.IsValid();
	}

	/// <summary>
	/// Searches self and descendants for modules.
	/// </summary>
	public void RegisterModules()
	{
		Modules ??= [];

		foreach ( var m in Components.GetAll<Module>( FindMode.EverythingInSelfAndDescendants ) )
			TryRegisterModule( m );
	}

	public bool TryRegisterModule( Module m )
	{
		if ( !m.IsValid() || !m.IsParent( this ) )
			return false;

		if ( Modules is null )
		{
			Modules = [m];
		}
		else
		{
			if ( Modules.Contains( m ) )
				return false;

			Modules.Add( m );
		}

		m.OnRegistered( this );

		OnRegisterModule( m );

		return true;
	}

	public void RemoveModule( Module m )
	{
		// Not using IsValid because it might be destroyed.
		if ( m is null || Modules is null )
			return;

		Modules.RemoveAll( mod => !mod.IsValid() || mod == m );

		if ( m.IsValid() )
			m.OnRemoved( this );

		OnRemoveModule( m );

		return;
	}

	public virtual void OnRegisterModule( Module m )
	{
	}

	public virtual void OnRemoveModule( Module m )
	{
	}

	public override void UpdateNetworking( Connection cn )
	{
		base.UpdateNetworking( cn );

		UpdateModuleOwnership( cn );
	}

	/// <summary>
	/// Update the network ownership of currently registered modules. <br />
	/// Does not try to register modules if they aren't cached yet.
	/// </summary>
	protected void UpdateModuleOwnership( Connection cn = null )
	{
		if ( !this.IsValid() || Modules is null )
			return;

		cn ??= Network?.Owner;

		foreach ( var mod in Modules )
			mod?.UpdateNetworking( cn );
	}

	/// <summary>
	/// Creates and attaches a module from a prefab.
	/// </summary>
	/// <param name="prefab"></param>
	public virtual IEnumerable<Module> AddModule( PrefabFile prefab )
	{
		if ( !this.IsValid() )
			return [];

		if ( !prefab.IsValid() || !prefab.TrySpawn( WorldTransform, out var go ) )
		{
			this.Warn( $"Tried to spawn invalid module prefab:[{prefab}]!" );
			return [];
		}

		var modules = go.Components.GetAll<Module>( FindMode.EverythingInSelfAndDescendants ) ?? [];

		if ( !modules.Any() )
		{
			this.Warn( $"No modules found on prefab:[{prefab}]! Destroying." );
			go.Destroy();
			return [];
		}

		// Updated as its own object before parenting.
		foreach ( var mod in modules )
			mod.UpdateNetworking( Network?.Owner );

		go.SetParent( GameObject, keepWorldPosition: false );

		return modules;
	}
}
