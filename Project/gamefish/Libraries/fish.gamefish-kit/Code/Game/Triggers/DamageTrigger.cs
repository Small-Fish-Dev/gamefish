namespace GameFish;

/// <summary>
/// Deals damage to whatever filter-passsing object enters it.
/// <code> trigger_hurt </code>
/// </summary>
[Icon( "sentiment_very_dissatisfied" )]
public partial class DamageTrigger : FilterTrigger
{
	protected const int DAMAGE_ORDER = DEFAULT_ORDER - 100;

	/// <summary>
	/// The health points intended to be subtracted.
	/// <br /> <br />
	/// <b> NOTE: </b> The target may affect
	/// this whenever receiving the damage.
	/// </summary>
	[Property]
	[Range( 0f, 100f, clamped: false )]
	[Feature( HEALTH ), Order( DAMAGE_ORDER )]
	public float Damage { get; set; } = 10;

	/// <summary>
	/// The types of damage inflicted.
	/// <b> TIP: </b> Try "burn", "bullet", "fall".
	/// </summary>
	[Property]
	[Feature( HEALTH ), Order( DAMAGE_ORDER )]
	public TagSet Types { get; set; } = [DamageTypes.BURN];

	/*
	/// <summary>
	/// Continuously deal damage between ticks?
	/// </summary>
	[Property]
	[Feature( HEALTH ), Group( TIMING )]
	public bool IsContinuous { get; set; } = true;

	/// <summary>
	/// The delay between each tick of damage.
	/// </summary>
	[Property]
	[Range( 0f, 1f, clamped: false )]
	[Feature( HEALTH ), Group( TIMING )]
	public float Interval { get; set; } = 0.5f;
	*/

	/// <summary>
	/// Enables specifying force along with damage.
	/// <br /> <br />
	/// <b> NOTE: </b> The target may affect
	/// this whenever receiving the damage.
	/// </summary>
	[Property]
	[Feature( HEALTH )]
	[ToggleGroup( nameof( HasImpulse ), Label = PHYSICS )]
	public bool HasImpulse { get; set; }

	/// <summary>
	/// The force to apply with each tick of damage.
	/// <br /> <br />
	/// <b> NOTE: </b> The target may affect
	/// this whenever receiving the damage.
	/// </summary>
	[Property]
	[Feature( HEALTH )]
	[ToggleGroup( nameof( HasImpulse ) )]
	public Vector3 Impulse { get; set; }

	protected override void OnTouchStart( GameObject obj )
	{
		base.OnTouchStart( obj );

		TryDamage( obj );
	}

	public DamageData GetDamageData()
	{
		Vector3? impulse = HasImpulse ? Impulse : null;
		var dmgData = new DamageData( Damage, impulse, null, this, Types );

		return dmgData;
	}

	public virtual bool TryDamage( GameObject obj )
	{
		if ( !obj.IsValid() )
			return false;

		const FindMode findMode = FindMode.Enabled | FindMode.InSelf | FindMode.InAncestors;

		if ( obj.Components.TryGet<IHealth>( out var hp, findMode ) )
			return TryDamage( hp );

		if ( obj.Components.TryGet<IDamageable>( out var dmg, findMode ) )
			return TryDamage( dmg );

		return false;
	}

	protected virtual bool TryDamage( IHealth hp )
	{
		if ( hp is null )
			return false;

		this.Log( hp );

		return hp.TrySendDamage( GetDamageData() );
	}

	protected virtual bool TryDamage( IDamageable dmg )
	{
		var dmgInfo = new DamageInfo()
		{
			Attacker = GameObject,
			Weapon = GameObject,
			Origin = Center,
			Tags = Types ?? []
		};

		dmg.OnDamage( dmgInfo );

		return true;
	}
}
