using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// An entity that supports <see cref="Module"/>s.
/// </summary>
public partial class ModuleEntity : Entity, Component.INetworkSpawn
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
	protected virtual bool IsModule<TMod>( Module m ) where TMod : class
		=> m.IsValid() && m is TMod;

	public override bool TryNetwork( Connection cn, bool allowProxy = false )
	{
		// this.Log( $"{nameof( TrySetNetworkOwner )} cn:[{cn}] allowProxy:[{allowProxy}]" );

		if ( !base.TryNetwork( cn, allowProxy ) )
			return false;

		UpdateModuleOwnership( cn );

		return true;
	}

	/// <summary>
	/// Quickly checks the cache to see if we have any module of this type.
	/// </summary>
	/// <typeparam name="TMod"> The specific module. </typeparam>
	/// <returns> If the module exists. </returns>
	public bool HasModule<TMod>() where TMod : class
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
	public TMod GetModule<TMod>() where TMod : class
	{
		var mType = typeof( TMod );
		return GetModules().FirstOrDefault( IsModule<TMod> ) as TMod;
	}

	/// <typeparam name="TMod"> The specific module. </typeparam>
	/// <returns> Every module of this type(if any, never null). </returns>
	public IEnumerable<TMod> GetModules<TMod>() where TMod : class
	{
		return GetModules()
			.Where( IsModule<TMod> )
			.Select( m => m as TMod );
	}

	/// <typeparam name="TMod"> The specific module. </typeparam>
	public bool TryGetModule<TMod>( out TMod m ) where TMod : class
	{
		m = GetModule<TMod>();
		return m != default;
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

		if ( !m.IsValid() || m == this || !m.IsParent( this ) )
			return false;

		// Prevent recursive module parenting.
		if ( m.Parent == this || m.Modules?.Contains( this ) is true )
		{
			this.Warn( $"Tried to register module:[{m}] when it was our parent!" );
			return false;
		}

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
	/// Removes a module from this entity.
	/// </summary>
	/// <param name="m"> The module to be removed. </param>
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
	protected void UpdateModuleOwnership( Connection cn = null )
	{
		if ( !GameObject.IsValid() )
			return;

		if ( !Network.Active || IsProxy )
			return;

		cn ??= Network?.Owner;

		foreach ( var mod in GetModules() )
		{
			if ( !mod.IsValid() )
				continue;

			mod.TryNetwork( cn, allowProxy: true );
		}
	}

	/// <summary>
	/// Creates and attaches a <typeparamref name="TMod"/> from a prefab.
	/// </summary>
	public virtual bool TryAddModule<TMod>( PrefabFile prefab, out TMod m )
		where TMod : Module
	{
		m = null;

		if ( !this.IsValid() )
			return false;

		if ( !prefab.IsValid() || !prefab.TrySpawn( WorldTransform, out var go ) )
		{
			this.Warn( $"Tried to spawn invalid module prefab:[{prefab}]!" );
			return false;
		}

		m = go.Components?.Get<TMod>( FindMode.EverythingInSelfAndDescendants );

		if ( !m.IsValid() )
		{
			this.Warn( $"No {typeof( TMod )} found on prefab:[{prefab}]! Destroying." );
			go.DestroyImmediate();
			return false;
		}

		m.SetupNetworking( force: true );

		go.SetParent( GameObject );

		return true;
	}

	/// <summary>
	/// Creates and attaches modules from a prefab.
	/// </summary>
	public virtual bool TryAddModules( PrefabFile prefab, out IEnumerable<Module> m )
	{
		m = [];

		if ( !this.IsValid() )
			return false;

		if ( !prefab.IsValid() || !prefab.TrySpawn( WorldTransform, out var go ) )
		{
			this.Warn( $"Tried to spawn invalid module prefab:[{prefab}]!" );
			return false;
		}

		m = go.Components?.GetAll<Module>( FindMode.EverythingInSelfAndDescendants ) ?? [];

		if ( !m.Any() )
		{
			this.Warn( $"No modules found on prefab:[{prefab}]! Destroying." );
			go.Destroy();
			return false;
		}

		go.LocalPosition = Vector3.Zero;
		go.SetParent( GameObject );

		return true;
	}
}
