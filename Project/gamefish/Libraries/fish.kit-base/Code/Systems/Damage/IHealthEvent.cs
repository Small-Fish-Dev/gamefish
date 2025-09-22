namespace GameFish;

public interface IHealthEvent
{
	public void OnSetHealth( in float hp );

	public void OnRevival();
	public void OnDeath();

	public bool TryDamage( ref DamageInfo dmgInfo ) => true;
	public void OnApplyDamage( ref DamageInfo dmgInfo );
}
