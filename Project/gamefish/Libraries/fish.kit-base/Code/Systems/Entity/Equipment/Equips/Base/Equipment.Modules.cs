using System;

namespace GameFish;

partial class Equipment
{
	/// <summary> The cache of modules for equipment belonging to this. </summary>
	[SkipHotload]
	public IEnumerable<EquipModule> EquipModules
	{
		get => _equipModules ??= CacheEquipModules();
		set => _equipModules = value;
	}

	[SkipHotload]
	protected IEnumerable<EquipModule> _equipModules;

	/// <returns> Refreshes/initializes this equipment's module cache. </returns>
	protected IEnumerable<EquipModule> CacheEquipModules()
		=> EquipModules = GetModules<EquipModule>();


	/// <summary> The cache of function modules for this equipment. </summary>
	[SkipHotload]
	public IEnumerable<EquipFunction> Functions
	{
		get => _functions ??= CacheFunctions();
		set => _functions = value;
	}

	[SkipHotload]
	protected IEnumerable<EquipFunction> _functions;

	/// <returns> Refreshes/initializes this equipment's function modules. </returns>
	protected IEnumerable<EquipFunction> CacheFunctions()
		=> Functions = GetModules<EquipFunction>();


	protected override void OnModulesRefreshed()
	{
		base.OnModulesRefreshed();

		CacheEquipModules();
		CacheFunctions();
	}

	/// <summary>
	/// Called by the owner to allow attacks, reloading etc.
	/// </summary>
	protected virtual void SimulateModules( in float deltaTime )
	{
		foreach ( var module in EquipModules )
			module.Simulate( deltaTime );
	}

	/// <summary>
	/// Safely indicates to all equipment modules that something happened.
	/// </summary>
	protected void OnModuleEvent( in Action<EquipModule> action )
		=> OnModuleEvent<EquipModule>( action );

	/// <summary>
	/// Safely indicates to all <typeparamref name="TModule"/>s that something happened.
	/// </summary>
	protected virtual void OnModuleEvent<TModule>( in Action<TModule> action ) where TModule : EquipModule
	{
		if ( action is null )
			return;

		foreach ( var m in EquipModules )
		{
			if ( m is not TModule module )
				continue;

			try
			{
				action.Invoke( module );
			}
			catch ( Exception e )
			{
				this.Warn( $"Module event exception: {e}" );
			}
		}
	}

	/// <summary>
	/// Allows this equipment to alter the delay before a function can be used again.
	/// </summary>
	/// <returns> The delay between each use(in seconds). </returns>
	public virtual float GetCooldownDuration( in float baseCooldown, EquipFunction func = null )
		=> baseCooldown;

	/// <summary>
	/// Allows this equipment to alter the time it takes to reload ammo.
	/// </summary>
	/// <returns> The duration of reloading(in seconds). </returns>
	public virtual float GetReloadDuration( in float baseDuration, EquipFunction func = null )
		=> baseDuration;

	/// <summary>
	/// Allows this equipment to alter magazine size.
	/// </summary>
	/// <returns> The magazine size for that ammo module. </returns>
	public virtual float GetMagazineSize( in float baseCapacity, AmmoEquipModule ammo )
		=> baseCapacity;

	/*
	/// <summary>
	/// Allows this equipment to alter how much an ammo module has in reserve(if enabled).
	/// </summary>
	/// <returns> The amount of ammo that module has left to refill from. </returns>
	public virtual float GetReserveCount( in float baseCount, AmmoEquipModule ammo )
		=> baseCount;
	*/

	/// <summary>
	/// Allows this equipment to alter the limit of ammo it can have in reserve.
	/// </summary>
	/// <returns> The limit of reserve ammo for that ammo module. </returns>
	public virtual float GetReserveLimit( in float baseCapacity, AmmoEquipModule ammo )
		=> baseCapacity;
}
