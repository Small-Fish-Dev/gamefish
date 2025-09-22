using System;

namespace GameFish;

public interface IHealth : Component.IDamageable
{
	abstract bool IsAlive { get; set; }

	/// <summary> Is this capable of ever taking damage? </summary>
	abstract bool IsDestructible { get; set; }

	abstract float Health { get; set; }
	abstract float MaxHealth { get; set; }

	/// <summary>
	/// The collection of <see cref="IHealthEvent"/>s relevant to this object. <br />
	/// Example: retrieved from a <see cref="ComponentList"/>.
	/// </summary>
	public IEnumerable<IHealthEvent> HealthEvents { get; }

	/// <summary>
	/// The engine's method for dealing damage that does not return success.
	/// </summary>
	void Component.IDamageable.OnDamage( in DamageInfo dmgInfo )
		=> TryDamage( dmgInfo );

	public void SetHealth( in float hp );
	public void ModifyHealth( in float hp );

	public void Die();
	public void Revive( bool restoreHealth = false );

	public void OnDeath();
	public void OnRevival();

	public bool CanDamage( in DamageInfo dmgInfo );
	public bool TryDamage( DamageInfo dmgInfo );
	public void ApplyDamage( DamageInfo dmgInfo );
}
