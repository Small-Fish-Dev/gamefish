namespace GameFish;

public interface IHealth : Component.IDamageable
{
	public bool IsAlive { get; }
	public float Health { get; }

	/// <summary>
	/// The collection of <see cref="IHealthEvent"/>s relevant to this object. <br />
	/// Example: retrieved from a <see cref="ComponentList"/>.
	/// </summary>
	public IEnumerable<IHealthEvent> HealthEvents { get; }

	/// <summary>
	/// Lets clients and owners know if this damage is allowed.
	/// </summary>
	public bool CanDamage( in DamageInfo dmgInfo );

	void Component.IDamageable.OnDamage( in DamageInfo dmgInfo )
		=> TryDamage( dmgInfo );

	public bool TryDamage( in DamageInfo dmgInfo );

	public void SetHealth( in float hp );
	public void ModifyHealth( in float hp );
}
