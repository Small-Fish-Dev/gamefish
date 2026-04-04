namespace GameFish;

/// <summary>
/// Listens to damage events from <see cref="DynamicEntity"/>
/// to respond to, block or modify damage being dealt.
/// </summary>
[Group( Library.NAME )]
[Icon( "sports_martial_arts" )]
public abstract class DamageModule : Module
{
	protected const int HEALTH_ORDER = DEFAULT_ORDER - 1000;

	public override bool IsParent( ModuleEntity comp )
		=> comp is DynamicEntity;

	public DynamicEntity ParentEntity => Parent as DynamicEntity;

	/// <returns> If false: prevents damage. </returns>
	public virtual bool CanDamage( in DamageData data )
		=> true;

	/// <summary>
	/// Called before damage is dealt to modify it.
	/// </summary>
	public virtual void ModifyDamage( ref DamageData data ) { }

	/// <summary>
	/// Called after damage is dealt to respond to it.
	/// </summary>
	public virtual void OnDamaged( in DamageData data ) { }

	public virtual void OnDeath() { }
	public virtual void OnAlive() { }
}
