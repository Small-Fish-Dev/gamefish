namespace GameFish;

/// <summary>
/// A module for components. Registers itself. <br />
/// These are entities that can automatically network the object they're on. <br />
/// You should put them on child objects of the parent component.
/// </summary>
public abstract partial class Module : BaseEntity, Component.ExecuteInEditor
{
	public const string MODULE = "🧩 Module";
	public const int MODULE_ORDER = NETWORK_ORDER - 1;

	/// <summary>
	/// If true: the entire object is destroyed with this component. <br />
	/// You should probably always enable this if it's on its own object
	/// and in a networked environment to synchronize its removal.
	/// </summary>
	[Sync]
	[Property]
	[Feature( ENTITY ), Group( MODULE ), Order( MODULE_ORDER )]
	public bool DestroyObject { get; set; } = true;

	/// <summary>
	/// The entity this is registered to.
	/// </summary>
	[Property]
	[Feature( ENTITY ), Group( MODULE ), Order( MODULE_ORDER )]
	public ModuleEntity Parent
	{
		get => _parent.IsValid() ? _parent
			: _parent = Components?.GetAll<ModuleEntity>( FindMode.EverythingInSelfAndAncestors )
				.FirstOrDefault( p => p.IsValid() && IsParent( p ) );
	}

	protected ModuleEntity _parent;

	/// <returns> If this module is meant to target a module-supporting entity. </returns>
	public abstract bool IsParent( ModuleEntity comp );

	protected override void OnEnabled()
	{
		base.OnEnabled();

		RegisterModule();
	}

	protected override void OnStart()
	{
		RegisterModule();

		if ( !Parent.IsValid() )
			this.Warn( $"Failed to find parent component." );

		base.OnStart();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		RemoveModule();

		if ( DestroyObject )
			GameObject?.Destroy();
	}

	public void RegisterModule()
	{
		var comp = Parent;

		if ( comp.IsValid() )
			comp.RegisterModule( this );
	}

	public void RemoveModule()
	{
		var comp = Parent;

		if ( comp.IsValid() )
			comp.RemoveModule( this );
	}

	/// <summary>
	/// Called when this has been successfully registered onto a parent component.
	/// </summary>
	public virtual void OnRegistered( ModuleEntity comp )
	{
	}

	/// <summary>
	/// Called when this has been removed from a parent component.
	/// </summary>
	public virtual void OnRemoved( ModuleEntity comp )
	{
	}
}
