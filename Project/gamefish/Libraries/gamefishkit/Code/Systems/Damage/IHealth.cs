namespace GameFish;

public interface IHealth : Component.IDamageable
{
	public bool IsAlive { get; }
	public float Health { get; }

	/// <summary>
	/// Indicates if this damage is allowed.
	/// </summary>
	public bool CanDamage( in DamageData data );

	/// <summary>
	/// Checks if the damage is allowed before trying to network it.
	/// </summary>
	/// <returns> If the damage was sent(with probable success). </returns>
	public bool TrySendDamage( in DamageData data );

	void Component.IDamageable.OnDamage( in DamageInfo info )
		=> TrySendDamage( new DamageData( info ) );
}
