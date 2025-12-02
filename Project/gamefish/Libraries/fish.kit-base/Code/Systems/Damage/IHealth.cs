namespace GameFish;

public interface IHealth : Component.IDamageable
{
	public bool IsAlive { get; }
	public float Health { get; }

	/// <summary>
	/// Indicates if this damage is allowed.
	/// </summary>
	public bool CanDamage( in DamageInfo dmgInfo );

	public bool TryDamage( in DamageInfo dmgInfo );

	public void SetHealth( in float hp );
	public void ModifyHealth( in float hp );

	void Component.IDamageable.OnDamage( in DamageInfo dmgInfo )
		=> TryDamage( dmgInfo );
}
