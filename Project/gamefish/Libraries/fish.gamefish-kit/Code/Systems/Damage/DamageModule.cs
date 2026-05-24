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

	public bool IsAlive => ParentEntity?.IsAlive is true;
	public bool IsDestructible => ParentEntity?.IsDestructible is true;

	public float Health => ParentEntity?.Health ?? 0f;
	public float MaxHealth => ParentEntity?.MaxHealth ?? 0f;

	public override Vector3 Center => Parent?.Center ?? WorldPosition;

	/// <returns> If false: prevents damage. </returns>
	public virtual bool CanDamage( in DamageData data )
		=> true;

	/// <summary>
	/// Modifies damage before it is dealt.
	/// </summary>
	public virtual void ModifyDamage( ref DamageData data ) { }

	/// <summary>
	/// Responds to damage after it is dealt.
	/// </summary>
	public virtual void OnDamaged( in DamageData data ) { }

	/// <summary>
	/// The parent entity has revived.
	/// </summary>
	public virtual void OnAlive() { }

	/// <summary>
	/// The parent entity has died.
	/// </summary>
	public virtual void OnDeath()
	{
		if ( DestroyUponDeath )
			SelfDestruct();
	}

	/// <summary>
	/// If true: destroy the parent's object upon death.
	/// </summary>
	[Property]
	[Order( HEALTH_ORDER )]
	[Title( "Self-Destruct" )]
	[Feature( HEALTH ), Group( DEATH )]
	public bool DestroyUponDeath { get; set; } = false;

	protected virtual void SelfDestruct()
	{
		if ( IsProxy )
			return;

		var obj = Parent?.GameObject;

		if ( obj.IsValid() )
			obj.Destroy();
	}
}
