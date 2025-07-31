namespace GameFish;

/// <summary>
/// A module for <typeparamref name="T"/> components. Registers itself. <br />
/// These are entities that can automatically network the object they're on. <br />
/// You should put them on child objects of the parent component.
/// </summary>
public abstract partial class Module<T> : BaseEntity, Component.ExecuteInEditor where T : Component, IModules<T>
{
	/// <summary>
	/// If true: the entire object is destroyed with this component. <br />
	/// You should probably always enable this if it's on its own object
	/// and in a networked environment to synchronize its removal.
	/// </summary>
	[Sync]
	[Property]
	[Feature( ENTITY ), Group( MODULE )]
	public bool DestroyObject { get; set; } = true;

	/// <summary>
	/// The <typeparamref name="T"/> this module should register with.
	/// </summary>
	[Property]
	[Feature( ENTITY ), Group( MODULE )]
	public T ModuleParent
	{
		get => _comp.IsValid() ? _comp
			: _comp = Components?.Get<T>( FindMode.EverythingInSelfAndAncestors );
	}

	protected T _comp;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		RegisterModule();
	}

	protected override void OnStart()
	{
		RegisterModule();

		if ( !ModuleParent.IsValid() )
			this.Warn( $"Failed to find parent component of type:[{typeof( T )}]" );

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
		var comp = ModuleParent;

		if ( comp.IsValid() )
			comp.RegisterModule( this );
	}

	public void RemoveModule()
	{
		var comp = ModuleParent;

		if ( comp.IsValid() )
			comp.RemoveModule( this );
	}

	/// <summary>
	/// Called when this has been successfully registered onto a parent component.
	/// </summary>
	public virtual void OnRegistered( T comp )
	{
	}

	/// <summary>
	/// Called when this has been removed from a parent component.
	/// </summary>
	public virtual void OnRemoved( T comp )
	{
	}
}
