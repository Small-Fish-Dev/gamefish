namespace GameFish;

/// <summary>
/// An entity that supports health and physics.
/// </summary>
public partial class DestructibleEntity : PhysicsEntity, IHealth
{
	public const string VALUES = "Values";

	[Sync]
	[Property, Feature( HEALTH )]
	public bool IsAlive { get; set; } = true;

	/// <summary> Is this capable of ever taking damage? </summary>
	[Property, Feature( HEALTH )]
	public virtual bool IsDestructible { get; set; } = true;

	[Sync]
	[Property, Title( "Initial" )]
	[Group( VALUES ), Feature( HEALTH )]
	[ShowIf( nameof( IsDestructible ), true )]
	public float Health { get; set; } = 100f;

	[Sync]
	[Property, Title( "Max" )]
	[Group( VALUES ), Feature( HEALTH )]
	[ShowIf( nameof( IsDestructible ), true )]
	public float MaxHealth { get; set; } = 100f;

	[Property]
	[Feature( HEALTH ), Group( DEBUG )]
	[ShowIf( nameof( IsDestructible ), true )]
	public float DebugDamage { get; set; } = 25f;

	public IEnumerable<IHealthEvent> HealthEvents
		=> Components?.GetAll<IHealthEvent>( FindMode.EnabledInSelfAndDescendants ) ?? [];

	[Button]
	[Title( "Take Damage" )]
	[Feature( HEALTH ), Group( DEBUG )]
	[ShowIf( nameof( IsDestructible ), true )]
	protected void DebugTakeDamage()
		=> TryDamage( new() { Damage = DebugDamage } );

	public void OnDamage( in DamageInfo dmgInfo )
		=> TryDamage( dmgInfo );

	public virtual void SetHealth( in float hp )
	{
		Health = hp.Clamp( 0f, MaxHealth );

		foreach ( var e in HealthEvents )
			e.OnSetHealth( hp );

		if ( Health > 0 )
			Revive();
		else if ( Health <= 0 )
			Die();
	}

	public virtual void ModifyHealth( in float hp )
		=> SetHealth( Health + hp );

	public virtual void Die()
	{
		if ( !IsAlive )
			return;

		if ( Health > 0f )
			Health = 0f;

		IsAlive = false;
		OnDeath();
	}

	public virtual void Revive( bool restoreHealth = false )
	{
		if ( IsAlive )
			return;

		IsAlive = true;
		Health = Health.Max( restoreHealth ? MaxHealth : 1 );

		OnRevival();
	}

	public virtual void OnDeath()
	{
		foreach ( var e in HealthEvents )
			e.OnDeath();
	}

	public virtual void OnRevival()
	{
		foreach ( var e in HealthEvents )
			e.OnRevival();
	}

	public virtual bool CanDamage( in DamageInfo dmgInfo )
		=> IsDestructible && dmgInfo.Damage > 0;

	public virtual bool TryDamage( DamageInfo dmgInfo )
	{
		if ( !CanDamage( in dmgInfo ) )
			return false;

		foreach ( var e in HealthEvents )
			if ( !e.TryDamage( ref dmgInfo ) )
				return false;

		ApplyDamage( dmgInfo );
		return true;
	}

	public virtual void ApplyDamage( DamageInfo dmgInfo )
	{
		foreach ( var e in HealthEvents )
			e.OnApplyDamage( ref dmgInfo );

		ModifyHealth( -dmgInfo.Damage );
	}
}
