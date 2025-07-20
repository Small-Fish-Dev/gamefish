namespace GameFish;

partial class BaseEntity
{
	/// <summary>
	/// Does this entity have a valid <see cref="global::GameFish.HealthComponent"/>?
	/// </summary>
	public bool HasHealth => HealthComponent.IsValid();

	[Title( "Component" )]
	[Property, Feature( IHealth.FEATURE )]
	public HealthComponent HealthComponent
	{
		get => _hp.IsValid() ? _hp : Components?.Get<HealthComponent>( FindMode.EverythingInSelfAndAncestors );
		set { _hp = value; }
	}
	private HealthComponent _hp;

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
