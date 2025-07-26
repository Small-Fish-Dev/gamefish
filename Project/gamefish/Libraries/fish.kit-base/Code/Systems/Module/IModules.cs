using System;

namespace GameFish;

/// <summary>
/// Allows a component to have modules.
/// </summary>
public interface IModules<T> where T : Component, IModules<T>
{
	/// <summary>
	/// The component we're adding modules to. <br />
	/// Implementation:
	/// <code> public (T) Component => this; </code>
	/// </summary>
	public T Component { get; }

	/// <summary>
	/// A way to access this interface. <br />
	/// Implementation:
	/// <code> public IModules(T) Modules => this; </code>
	/// </summary>
	public IModules<T> Modules { get; }

	/// <summary>
	/// The cached list of registered modules. <br />
	/// Use <see cref="GetModules"/> to also auto-register them if necessary.
	/// </summary>
	public List<Module<T>> ModuleList { get; set; }

	protected bool IsModule<TMod>( Module<T> m )
		=> m.IsValid() && m is TMod;

	/// <summary>
	/// Quickly checks the cache to see if we have this module.
	/// </summary>
	/// <typeparam name="TMod"> The specific type module for <typeparamref name="T"/>. </typeparam>
	/// <returns> If the module exists. </returns>
	public bool HasModule<TMod>() where TMod : Module<T>
	{
		var mType = typeof( TMod );
		return GetModules().Any( IsModule<TMod> );
	}

	/// <summary>
	/// Always gives you a cached list of modules. Registers them if that hasn't been done yet.
	/// </summary>
	/// <returns> The cached list of modules. </returns>
	public IEnumerable<Module<T>> GetModules()
	{
		if ( ModuleList is null )
			RegisterModules();

		return ModuleList ??= [];
	}

	/// <typeparam name="TMod"> The specific type module for <typeparamref name="T"/>. </typeparam>
	public TMod GetModule<TMod>() where TMod : Module<T>
	{
		var mType = typeof( TMod );
		return GetModules().FirstOrDefault( IsModule<TMod> ) as TMod;
	}

	/// <typeparam name="TMod"> The specific type module for <typeparamref name="T"/>. </typeparam>
	/// <returns> Every module of this type(if any, never null). </returns>
	public IEnumerable<TMod> GetModules<TMod>() where TMod : Module<T>
	{
		return GetModules()
			.Select( m => m as TMod )
			.Where( m => m.IsValid() );
	}

	/// <typeparam name="TMod"> The specific type module for <typeparamref name="T"/>. </typeparam>
	public bool TryGetModule<TMod>( out TMod m ) where TMod : Module<T>
	{
		m = GetModule<TMod>();
		return m.IsValid();
	}

	public void RegisterModules()
	{
		ModuleList ??= [];

		if ( !Component.IsValid() )
		{
			GameFish.Warn( this, $"Unimplemented Component:[{typeof( T )}]" );
			return;
		}

		foreach ( var m in Component.Components.GetAll<Module<T>>( FindMode.EverythingInSelfAndDescendants ) )
			RegisterModule( m );
	}

	public bool RegisterModule( Module<T> m )
	{
		if ( !m.IsValid() )
			return false;

		if ( ModuleList is null )
		{
			ModuleList = [m];
		}
		else
		{
			if ( ModuleList.Contains( m ) )
				return false;

			ModuleList.Add( m );
		}

		m.OnRegistered( Component );

		OnRegisterModule( m );

		return true;
	}

	public void RemoveModule( Module<T> m )
	{
		// Not using IsValid because it might be destroyed.
		if ( m is null || ModuleList is null )
			return;

		ModuleList.RemoveAll( mod => !mod.IsValid() || mod == m );

		if ( m.IsValid() )
			m.OnRemoved( Component );

		OnRemoveModule( m );

		return;
	}

	public void OnRegisterModule( Module<T> m )
	{
	}

	public void OnRemoveModule( Module<T> m )
	{
	}
}
