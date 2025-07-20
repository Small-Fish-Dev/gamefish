namespace GameFish;

/// <summary>
/// A module for <typeparamref name="T"/> components. Registers itself.
/// </summary>
public abstract partial class Module<T> : Component, Component.ExecuteInEditor where T : Component, IModules<T>
{
	public const string FEATURE_MODULES = "🧩 Modules";
	public const string GROUP_MODULE = "🧩 Module";

	/// <summary>
	/// Is this component currently loaded in the scene editor? <br />
	/// Otherwise it is assumed to be in play mode. <br />
	/// You can use this with <see cref="HideIfAttribute"/> or <see cref="ShowIfAttribute"/>.
	/// </summary>
	public bool EditingScene => Scene?.IsEditor ?? true;

	/// <summary>
	/// The <typeparamref name="T"/> this module should register with.
	/// </summary>
	[Property]
	[Feature( BaseEntity.DEBUG )]
	[Group( GROUP_MODULE )]
	public T ParentComponent
	{
		get => _comp.IsValid() ? _comp : _comp = Components?.Get<T>( FindMode.EverythingInSelfAndAncestors );
		set => _comp = value;
	}
	protected T _comp;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		RegisterModule();
	}

	protected override void OnStart()
	{
		base.OnStart();

		RegisterModule();

		if ( !ParentComponent.IsValid() )
			this.Warn( $"Failed to find parent component of type:[{typeof( T )}]" );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		RemoveModule();
	}

	public void RegisterModule()
	{
		var comp = ParentComponent;

		if ( comp.IsValid() )
			comp.RegisterModule( this );
	}

	public void RemoveModule()
	{
		var comp = ParentComponent;

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
