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

	[Property]
	[Title( "Render Traces" )]
	[Order( BULLET_DEBUG_ORDER )]
	[Feature( BULLET ), Group( DEBUG )]
	public bool DebugRenderTraces { get; set; } = false;

	/// <summary>
	/// The maximum distance the bullet trace will travel.
	/// </summary>
	[Property]
	[Order( BULLET_ORDER )]
	[Feature( BULLET ), Group( TRACING )]
	public virtual float TraceDistance { get; set; } = 4096f;

	/// <summary>
	/// The base damage of the weapon.
	/// </summary>
	[Property]
	[Title( "Damage" )]
	[Order( BULLET_ORDER )]
	[InlineEditor, WideMode]
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
	[Feature( BULLET ), Group( SPREAD )]
	public virtual SpreadShape SpreadShape { get; set; }

	/// <summary>
	/// The base angle(in degrees) of bullet cone spread.
	/// </summary>
	[Title( "Base" )]
	[Range( 0f, 90f, clamped: false )]
	[Property, Feature( BULLET ), Group( SPREAD )]
	public virtual float SpreadCone { get; set; } = 1f;

	/// <summary>
	/// The current angle(in degrees) of total, active bullet spread.
	/// </summary>
	[Title( "Current" )]
	[Header( "Debug" )]
	[Property, ReadOnly, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( BULLET ), Group( SPREAD )]
	protected virtual Vector3 InspectorSpread => GetSpread();

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

	protected override void Activate()
	{
		var tr = GetBulletTrace( AimTransform ).Run();

		if ( DebugRenderTraces )
			DebugOverlay.Trace( tr, duration: 1f );

		// TODO: Tracer effects.
		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return;

		tr.GameObject.TryDamage( GetDamage( in tr ) );
	}
}
