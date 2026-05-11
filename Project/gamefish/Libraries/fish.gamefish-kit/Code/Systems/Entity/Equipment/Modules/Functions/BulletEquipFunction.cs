using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// 🔫 Shoots bullets.
/// </summary>
[Icon( "clear_all" )]
[Title( "Bullet Shooter" )]
public partial class BulletEquipFunction : EquipFunction
{
	protected const int BULLET_ORDER = MODULE_ORDER - 1000;

	protected const int BULLET_DEBUG_ORDER = BULLET_ORDER - 10;

	protected const int BULLET_SOUNDS_ORDER = BULLET_ORDER + 10;
	protected const int BULLET_SPREAD_ORDER = BULLET_ORDER + 20;
	protected const int BULLET_TRACING_ORDER = BULLET_ORDER + 30;
	protected const int BULLET_HEALTH_ORDER = BULLET_ORDER + 40;

	[Property]
	[Title( "Render Traces" )]
	[Order( BULLET_DEBUG_ORDER )]
	[Feature( BULLET ), Group( DEBUG )]
	public bool DebugRenderTraces { get; set; } = false;

	/// <summary>
	/// The current angle(in degrees) of total, active bullet spread.
	/// </summary>
	[Property]
	[Title( "Current" )]
	[ReadOnly, JsonIgnore]
	[Order( BULLET_DEBUG_ORDER )]
	[ShowIf( nameof( InGame ), true )]
	[Feature( BULLET ), Group( DEBUG )]
	protected virtual Vector3 InspectorSpread => GetSpread();

	[Property]
	[Title( "Fire" )]
	[Order( BULLET_SOUNDS_ORDER )]
	[Feature( BULLET ), Group( SOUNDS )]
	public virtual SoundEvent FireSound { get; set; }

	[Property]
	[Title( "Empty" )]
	[Order( BULLET_SOUNDS_ORDER )]
	[Feature( BULLET ), Group( SOUNDS )]
	public virtual SoundEvent EmptySound { get; set; }

	/// <summary>
	/// The maximum distance the bullet trace will travel.
	/// </summary>
	[Property]
	[Title( "Distance" )]
	[Order( BULLET_TRACING_ORDER )]
	[Feature( BULLET ), Group( TRACING )]
	public virtual float TraceDistance { get; set; } = 4096f;

	/// <summary>
	/// The base damage of the weapon.
	/// </summary>
	[Property]
	[Title( "Damage" )]
	[InlineEditor, WideMode]
	[Order( BULLET_HEALTH_ORDER )]
	[Feature( BULLET ), Group( HEALTH )]
	public DamageSettings DamageSettings { get; protected set; } = new()
	{
		BaseDamage = 25f,

		Impulse = 1f,
		ScaleForces = true,
		EnableForces = true,

		EnableHitboxes = true,
		HitboxMultipliers = { ["head"] = 2f },
	};

	/// <summary>
	/// If it's a cone or box or whatever.
	/// </summary>
	[Property]
	[Title( "Shape" )]
	[Order( BULLET_SPREAD_ORDER )]
	[Feature( BULLET ), Group( SPREAD )]
	public virtual SpreadShape SpreadShape { get; set; }

	/// <summary>
	/// The base angle(in degrees) of bullet cone spread.
	/// </summary>
	[Title( "Base" )]
	[Order( BULLET_SPREAD_ORDER )]
	[Range( 0f, 90f, clamped: false )]
	[Property, Feature( BULLET ), Group( SPREAD )]
	public virtual float SpreadCone { get; set; } = 1f;

	/// <summary>
	/// The current angle(in degrees) of active bullet spread.
	/// </summary>
	protected virtual Vector2 GetSpread()
		=> Equip?.GetCurrentSpread( SpreadCone, this ) ?? SpreadCone;

	/// <returns> The start position of the bullet. </returns>
	public virtual Vector3 GetBulletOrigin()
		=> AimPosition;

	/// <returns> The direction of the bullet as a rotation. </returns>
	public virtual Rotation GetBulletDirection( in Vector2? spread = null )
	{
		var bulletSpread = spread ?? GetSpread();

		return bulletSpread != default
			? AimRotation * GetSpreadConeRotation( bulletSpread.Length )
			: AimRotation;
	}

	public SceneTrace GetBulletTrace( in Transform t )
		=> GetBulletTrace( t.Position, t.Rotation.Forward );

	public virtual SceneTrace GetBulletTrace( in Vector3 origin, in Vector3 dir )
	{
		if ( !Pawn.IsValid() )
			return default;

		return Pawn.GetEyeTrace( origin, origin + (dir * TraceDistance) );
	}

	protected virtual DamageData GetDamage( in SceneTraceResult tr )
		=> DamageData.FromBullet( DamageSettings, in tr, Equip );

	protected override void PlayActivationEffect( in Transform tOrigin )
	{
		if ( !FireSound.IsValid() )
			return;

		BroadcastSound( FireSound, tOrigin.Position );
	}

	protected override void Activate()
	{
		var tAim = AimTransform;

		PlayActivationEffect( in tAim );

		var tr = GetBulletTrace( in tAim ).Run();

		if ( DebugRenderTraces )
			DebugOverlay.Trace( tr, duration: 1f );

		// TODO: Tracer effects.
		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return;

		tr.GameObject.TryDamage( GetDamage( in tr ) );
	}

	protected override void ActivateEmpty()
	{
		base.ActivateEmpty();

		if ( !EmptySound.IsValid() )
			return;

		BroadcastSound( EmptySound, AimPosition );
	}
}
