using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// An entity that supports <see cref="Module"/>s.
/// </summary>
public partial class ModuleEntity : BaseEntity, Component.INetworkSpawn
{
	/// <summary>
	/// The cached list of modules belonging to this entity.
	/// </summary>
	[Title( "Modules" )]
	[Property, ReadOnly, JsonIgnore]
	[Feature( ENTITY ), Group( MODULES )]
	public List<Module> InspectorModules => Modules;

	public List<Module> Modules { get; protected set; }

	/// <returns> If this is the module we're looking for. </returns>
	protected virtual bool IsModule<TMod>( Module m ) where TMod : Module
		=> m.IsValid() && m is TMod;

	public override bool TrySetNetworkOwner( Connection cn, bool allowProxy = false )
	{
		// this.Log( $"{nameof( TrySetNetworkOwner )} cn:[{cn}] allowProxy:[{allowProxy}]" );

		if ( !base.TrySetNetworkOwner( cn, allowProxy ) )
			return false;

		UpdateModuleOwnership( cn, allowProxy );
		return true;
	}

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
	/// <returns> The first(if any) <typeparamref name="TMod"/>. </returns>
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
			.Where( IsModule<TMod> )
			.Select( m => m as TMod );
	}

	/// <typeparam name="TMod"> The specific module. </typeparam>
	public bool TryGetModule<TMod>( out TMod m ) where TMod : Module
	{
		m = GetModule<TMod>();
		return m.IsValid();
	}

	/// <summary>
	/// Called to let you update your module cache and such.
	/// </summary>
	protected virtual void OnModulesRefreshed()
	{
	}

	/// <summary>
	/// Searches self and descendants for modules.
	/// </summary>
	protected void RegisterModules()
	{
		Modules ??= [];

		foreach ( var m in Components.GetAll<Module>( FindMode.EverythingInSelfAndDescendants ) )
			TryRegisterModule( m, allowRefresh: false );

		OnModulesRefreshed();
	}

	/// <summary>
	/// Attempts to register a module.
	/// </summary>
	/// <param name="m"> The module. </param>
	/// <param name="allowRefresh"> Should <see cref="OnModulesRefreshed"/> be called? </param>
	/// <returns> If the module could be registered. </returns>
	public bool TryRegisterModule( Module m, bool allowRefresh = true )
	{
		if ( !GameObject.IsValid() || GameObject.IsDestroyed )
			return false;

		if ( !m.IsValid() || !m.IsParent( this ) )
			return false;

		// Add it to the list.
		if ( Modules is null )
			Modules = [m];
		else if ( !Modules.Contains( m ) )
			Modules.Add( m );

		m.OnRegistered( this );

		OnModuleRegistered( m );

		if ( allowRefresh )
			OnModulesRefreshed();

		return true;
	}

	/// <summary>
	/// Removes a module.
	/// </summary>
	/// <param name="m"> The module. </param>
	/// <param name="allowRefresh"> Should <see cref="OnModulesRefreshed"/> be called? </param>
	public void RemoveModule( Module m, bool allowRefresh = true )
	{
		// Not using IsValid right here because it might be destroyed.
		if ( m is null || Modules is null )
			return;

		Modules.RemoveAll( mod => !mod.IsValid() || mod == m );

		if ( m.IsValid() && m.Parent == this )
			m.OnRemoved( this );

		if ( GameObject.IsValid() && !GameObject.IsDestroyed )
		{
			OnModuleRemoved( m );

			if ( allowRefresh )
				OnModulesRefreshed();
		}

		return;
	}

	/// <summary>
	/// Called when a module has been successfully registered.
	/// </summary>
	protected virtual void OnModuleRegistered( Module m )
	{
	}

	/// <summary>
	/// Called when a valid module has been removed from a valid entity.
	/// </summary>
	protected virtual void OnModuleRemoved( Module m )
	{
	}

	/// <summary>
	/// Auto-updates child module ownership.
	/// </summary>
	void INetworkSpawn.OnNetworkSpawn( Connection cn )
		=> UpdateModuleOwnership( cn );

	/// <summary>
	/// Update the network ownership of currently registered modules. <br />
	/// Does not try to register modules if they aren't cached yet.
	/// </summary>
	protected void UpdateModuleOwnership( Connection cn = null, bool allowProxy = false )
	{
		if ( !GameObject.IsValid() )
			return;

		if ( !allowProxy && IsProxy )
			return;

		cn ??= Network?.Owner;

		foreach ( var mod in GetModules() )
			if ( mod.IsValid() )
				mod.TrySetNetworkOwner( cn );
	}

	/// <summary>
	/// Creates and attaches a module from a prefab.
	/// </summary>
	public virtual IEnumerable<Module> AddModule( PrefabFile prefab )
	{
		if ( !this.IsValid() )
			return [];

		if ( !prefab.IsValid() || !prefab.TrySpawn( WorldTransform, out var go ) )
		{
			this.Warn( $"Tried to spawn invalid module prefab:[{prefab}]!" );
			return [];
		}

		var modules = go.Components?.GetAll<Module>( FindMode.EverythingInSelfAndDescendants ) ?? [];

		if ( !modules.Any() )
		{
			this.Warn( $"No modules found on prefab:[{prefab}]! Destroying." );
			go.Destroy();
			return [];
		}

		go.SetParent( GameObject, keepWorldPosition: false );

		return modules;
	}
}
