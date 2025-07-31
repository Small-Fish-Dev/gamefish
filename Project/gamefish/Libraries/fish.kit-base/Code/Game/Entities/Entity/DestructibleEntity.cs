namespace GameFish;

/// <summary>
/// An entity that supports health and physics.
/// </summary>
public partial class DestructibleEntity : PhysicsEntity
{
	/// <summary>
	/// Does this entity have a valid <see cref="global::GameFish.HealthComponent"/>?
	/// </summary>
	public bool HasHealth => HealthComponent.IsValid();

	/// <summary>
	/// The <see cref="global::GameFish.HealthComponent"/> on this object or its children(if any).
	/// Add one to allow taking damage, healing, dying etc.
	/// </summary>
	[Title( "Component" )]
	[Property, Feature( IHealth.FEATURE )]
	public HealthComponent HealthComponent
	{
		get => _hp.IsValid() ? _hp
			: _hp = Components?.Get<HealthComponent>( FindMode.EverythingInSelf | FindMode.InDescendants | FindMode.InAncestors );

		set { _hp = value; }
	}

	protected HealthComponent _hp;

	[ShowIf( nameof( HasHealth ), true )]
	[Property, Feature( IHealth.FEATURE )]
	public bool IsAlive => HealthComponent?.IsAlive ?? false;

	[ShowIf( nameof( HasHealth ), true )]
	[Property, Feature( IHealth.FEATURE )]
	public bool IsDestructible => HealthComponent?.IsDestructible ?? false;

	[Title( "Initial" )]
	[Group( IHealth.GROUP_VALUES )]
	[ShowIf( nameof( HasHealth ), true )]
	[Property, Feature( IHealth.FEATURE )]
	public float Health => HealthComponent?.Health ?? 0f;

	[Title( "Max" )]
	[Group( IHealth.GROUP_VALUES )]
	[ShowIf( nameof( HasHealth ), true )]
	[Property, Feature( IHealth.FEATURE )]
	public float MaxHealth => HealthComponent?.MaxHealth ?? 0f;
}
