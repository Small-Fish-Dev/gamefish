namespace GameFish;

/// <summary>
/// Takes damage and calls health-related events/hooks.
/// Add this to the root of the prefab/object you want to have health.
/// </summary>
[Icon( "monitor_heart" )]
public partial class HealthComponent : Component, IHealth
{
	/// <summary>
	/// This is what you use to call <see cref="IHealth.TryDamage"/> and such.
	/// </summary>
	public IHealth Interface => this;

	public IEnumerable<IHealthEvent> HealthEvents
		=> Components?.GetAll<IHealthEvent>( FindMode.EnabledInSelfAndDescendants ) ?? [];

	[Sync]
	[Property, Feature( IHealth.FEATURE )]
	public bool IsAlive { get; set; } = true;

	/// <summary> Is this capable of ever taking damage? </summary>
	[Property, Feature( IHealth.FEATURE )]
	public virtual bool IsDestructible { get; set; } = true;

	[Sync]
	[Property, Title( "Initial" )]
	[ShowIf( nameof( IsDestructible ), true )]
	[Group( IHealth.GROUP_VALUES ), Feature( IHealth.FEATURE )]
	public float Health { get; set; } = 100f;

	[Sync]
	[Property, Title( "Max" )]
	[ShowIf( nameof( IsDestructible ), true )]
	[Group( IHealth.GROUP_VALUES ), Feature( IHealth.FEATURE )]
	public float MaxHealth { get; set; } = 100f;

	[Property]
	[ShowIf( nameof( IsDestructible ), true )]
	[Feature( IHealth.FEATURE ), Group( BaseEntity.DEBUG )]
	public float DebugDamage { get; set; } = 25f;

	[Button]
	[Title( "Take Damage" )]
	[ShowIf( nameof( IsDestructible ), true )]
	[Feature( IHealth.FEATURE ), Group( BaseEntity.DEBUG )]
	protected void DebugTakeDamage()
	{
		Interface?.TryDamage( new() { Damage = DebugDamage } );
	}
}
