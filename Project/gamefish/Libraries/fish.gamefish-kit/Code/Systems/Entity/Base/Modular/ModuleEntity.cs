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
	[Property]
	[Title( "Modules" )]
	[ReadOnly, JsonIgnore]
	[Feature( ENTITY ), Group( MODULES )]
	protected List<Module> InspectorModules => Modules;

	public List<Module> Modules { get; protected set; }

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
		if ( GameObject.IsDestroyed() )
			return;

		if ( !Network.Active || IsProxy )
			return;

		cn ??= Network?.Owner;

		foreach ( var mod in GetActiveModules() )
		{
			if ( !mod.IsValid() )
				continue;

			mod.TryNetwork( cn, allowProxy: true );
		}
	}

	public override bool TryNetwork( Connection cn, bool allowProxy = false )
	{
		// this.Log( $"{nameof( TrySetNetworkOwner )} cn:[{cn}] allowProxy:[{allowProxy}]" );

		if ( !base.TryNetwork( cn, allowProxy ) )
			return false;

		UpdateModuleOwnership( cn );

		return true;
	}

	/// <summary>
	/// Ensures this entity can safely parent a module.
	/// </summary>
	/// <param name="m"> The module that may want this entity as a parent. </param>
	/// <returns> If this entity accepts the responsibility of parenting that module. </returns>
	public virtual bool IsValidModule( Module m )
	{
		if ( m == this )
			return false;

		if ( !m.IsValid() || m.GameObject.IsDestroyed() )
			return false;

		return m.IsParent( this );
	}

	/// <returns> If this is the type of module we're looking for. </returns>
	protected static bool IsModule<TMod>( Module m ) where TMod : class
		=> m.IsValid() && m is TMod;

	/// <summary>
	/// Quickly checks the cache to see if we have any module of this type.
	/// </summary>
	/// <typeparam name="TMod"> The type of module. </typeparam>
	/// <returns> If the module exists. </returns>
	public bool HasModule<TMod>() where TMod : class
		=> GetActiveModules().Any( IsModule<TMod> );

	/// <summary>
	/// Always gives you a cached list of active and valid modules.
	/// Registers them if that hasn't been done yet.
	/// </summary>
	/// <returns> The cached list of modules. </returns>
	public IEnumerable<Module> GetActiveModules()
	{
		var list = Modules ?? RegisterModules();

		foreach ( var m in list )
		{
			if ( !m.IsValid() || !m.Active )
				continue;

			yield return m;
		}
	}

	/// <typeparam name="TMod"> The type of module. </typeparam>
	/// <returns> The first(if any) <typeparamref name="TMod"/>. </returns>
	public TMod GetModule<TMod>() where TMod : class
	{
		foreach ( var m in GetActiveModules() )
			if ( m is TMod tm )
				return tm;

		return null;
	}

	/// <typeparam name="TMod"> The type of module. </typeparam>
	public bool TryGetModule<TMod>( out TMod m ) where TMod : class
	{
		m = GetModule<TMod>();
		return m != default;
	}

	/// <typeparam name="TMod"> The type of modules to find. </typeparam>
	/// <returns> Every module of this type(if any, never null). </returns>
	public IEnumerable<TMod> GetModules<TMod>() where TMod : class
	{
		foreach ( var m in GetActiveModules() )
		{
			if ( !m.IsValid() || m.GameObject.IsDestroyed() )
				continue;

			if ( m is TMod tm )
				yield return tm;
		}

		yield break;
	}

	/// <typeparam name="TMod"> The type of the modules to find. </typeparam>
	public bool TryGetModules<TMod>( out IEnumerable<TMod> modules ) where TMod : class
	{
		modules = GetModules<TMod>();
		return modules?.Any() is true;
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
	protected List<Module> RegisterModules()
	{
		Modules ??= [];

		foreach ( var m in Components.GetAll<Module>( FindMode.EnabledInSelfAndDescendants ) )
			TryRegisterModule( m, allowRefresh: false );

		OnModulesRefreshed();

		return Modules;
	}

	/// <summary>
	/// Attempts to register a module.
	/// </summary>
	/// <param name="m"> The module. </param>
	/// <param name="allowRefresh"> Should <see cref="OnModulesRefreshed"/> be called after? </param>
	/// <returns> If the module could be registered. </returns>
	public bool TryRegisterModule( Module m, bool allowRefresh = true )
	{
		if ( GameObject.IsDestroyed() )
			return false;

		if ( !IsValidModule( m ) )
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

		if ( !GameObject.IsDestroyed() )
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
