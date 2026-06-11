namespace GameFish;

/// <summary>
/// Deals damage to whatever filter-passsing object enters it.
/// <code> trigger_hurt </code>
/// </summary>
[Icon( "sentiment_very_dissatisfied" )]
public partial class DamageTrigger : FilterTrigger
{
	protected const int HEALTH_ORDER = DEFAULT_ORDER - 1000;

	protected const int HEALTH_TIMING_ORDER = HEALTH_ORDER + 10;
	protected const int HEALTH_SOUNDS_ORDER = HEALTH_ORDER + 50;

	/// <summary>
	/// The health points intended to be subtracted.
	/// <br /> <br />
	/// <b> NOTE: </b> If the value is negative it's possible it may heal instead.
	/// <br /> <br />
	/// <b> NOTE: </b> The target may affect this whenever receiving the damage.
	/// </summary>
	[Property]
	[Range( 0f, 100f, clamped: false )]
	[Feature( HEALTH ), Order( HEALTH_ORDER )]
	public float Amount { get; set; } = 10;

	/// <summary>
	/// The types of damage inflicted.
	/// <br /> <br />
	/// <b> TIP: </b> Try "burn", "bullet", "fall".
	/// </summary>
	[Property]
	[Feature( HEALTH )]
	[Order( HEALTH_ORDER )]
	public TagSet Types { get; set; } = [Damage.FIRE, Damage.BURN];

	/// <summary>
	/// If enabled: continuously deal damage between ticks.
	/// </summary>
	[Property]
	[Order( HEALTH_TIMING_ORDER )]
	[Feature( HEALTH ), Group( TIMING )]
	public bool IsContinuous { get; set; } = true;

	/// <summary>
	/// The delay between each tick of damage.
	/// </summary>
	[Property]
	[Order( HEALTH_TIMING_ORDER )]
	[Range( 0.1f, 2f, clamped: false )]
	[Feature( HEALTH ), Group( TIMING )]
	public float Interval { get; set; } = 0.5f;

	/// <summary>
	/// Enables specifying force along with damage.
	/// <br /> <br />
	/// <b> NOTE: </b> The target may affect
	/// this whenever receiving the damage.
	/// </summary>
	[Property]
	[Feature( HEALTH )]
	[ToggleGroup( nameof( HasImpulse ), Label = VELOCITY )]
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

	/// <summary>
	/// The sound to play on entities affected by the damage.
	/// </summary>
	[Property]
	[Title( "On Damage" )]
	[Order( HEALTH_SOUNDS_ORDER )]
	[Range( 0, 10, clamped: false )]
	[Feature( HEALTH ), Group( SOUNDS )]
	public SoundEvent OnDamageSound { get; set; }

	// TODO: Dictionary<Component, TimeSince> SinceDamaged { get; set; }
	[Sync]
	public TimeUntil NextDamage { get; set; }

	public override Color DefaultGizmoColor => Color.Parse( "#e95426" ) ?? Color.Red.Desaturate( 0.3f );

	protected override void OnTouchStart( GameObject obj )
	{
		base.OnTouchStart( obj );

		TryDamage( obj );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		DamagingUpdate( Time.Delta );
	}

	/// <summary>
	/// Ran ever update to damage what's inside of it(if enabled).
	/// </summary>
	/// <param name="deltaTime"></param>
	protected virtual void DamagingUpdate( in float deltaTime )
	{
		if ( !IsContinuous )
			return;

		if ( !NextDamage )
			return;

		if ( Touching is null )
			return;

		var isEffective = false;

		foreach ( var obj in Touching )
			if ( TryDamage( obj ) )
				isEffective = true;

		if ( !isEffective )
			return;

		NextDamage = Interval;
	}

	public virtual DamageData GetDamageData()
	{
		Vector3? impulse = HasImpulse ? Impulse : null;
		var dmgData = new DamageData( Amount, impulse, null, this, Types );

		return dmgData;
	}

	protected virtual void OnObjectDamaged( GameObject obj, object target = null )
	{
		if ( !OnDamageSound.IsValid() )
			return;

		if ( target is not IValid v )
			return;

		if ( v.IsValid<Entity>( out var ent ) )
		{
			var tWorld = ent.WorldTransform;
			var localPoint = tWorld.PointToLocal( ent.Center );

			ent.HostBroadcastSound( OnDamageSound, localPoint );
		}
	}

	public virtual bool TryDamage( GameObject obj )
	{
		if ( !obj.IsValid() )
			return false;

		const FindMode findMode = FindMode.Enabled | FindMode.InSelf | FindMode.InAncestors;

		if ( obj.Components.TryGet<IHealth>( out var hp, findMode ) )
		{
			if ( TryDamage( hp ) )
			{
				OnObjectDamaged( obj, hp );
				return true;
			}

			return false;
		}

		if ( obj.Components.TryGet<IDamageable>( out var dmg, findMode ) )
		{
			if ( TryDamage( dmg ) )
			{
				OnObjectDamaged( obj, dmg );
				return true;
			}

			return false;
		}

		return false;
	}

	public virtual bool TryDamage( IHealth hp )
	{
		if ( hp is null )
			return false;

		if ( hp is IValid v && !v.IsValid() )
			return false;

		return hp.TrySendDamage( GetDamageData() );
	}

	public virtual bool TryDamage( IDamageable dmg )
	{
		if ( dmg is IValid v && !v.IsValid() )
			return false;

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
