using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// 🔫 Shoots bullets.
/// </summary>
[Icon( "clear_all" )]
public partial class BulletEquipFunction : EquipFunction
{
	/// <summary>
	/// The maximum distance the bullet trace will travel.
	/// </summary>
	[Title( "Max Distance" )]
	[Property, Feature( BULLET )]
	public float Distance { get; set; } = 4096f;

	/// <summary>
	/// The base damage of the weapon.
	/// </summary>
	[Title( "Base Damage" )]
	[Property, Feature( BULLET )]
	public float Damage { get; set; } = 25f;

	/// <summary>
	/// Multiplies weapon damage over distance.
	/// </summary>
	[Title( "Damage Range" )]
	[Property, Feature( BULLET )]
	public Curve DamageFalloff { get; set; } = new Curve( new( 0, 1 ), new( 0.3f, 1 ), new( 1, 0 ) )
	{
		TimeRange = new( 0f, 5000f ),
		ValueRange = new( 0f, 1f )
	};

	/// <summary>
	/// Scales damage based on any specified <see cref="Hitbox"/> tag.
	/// </summary>
	[Title( "Hitbox Damage" )]
	[Property, Feature( BULLET )]
	public Dictionary<string, float> DamageScaling { get; set; } = new()
	{
		["head"] = 2f,
	};

	/// <summary>
	/// If it's a cone or box or whatever.
	/// </summary>
	[Title( "Shape" )]
	[Property, Feature( BULLET ), Group( SPREAD )]
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
	protected Vector2 GetSpread()
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

	public virtual SceneTrace GetBulletTrace( in Vector3 origin, in Vector3 dir )
	{
		if ( !Pawn.IsValid() )
			return default;

		return Pawn.GetEyeTrace( origin, origin + (dir * Distance) );
	}

	public SceneTrace GetBulletTrace( in Transform t )
		=> GetBulletTrace( t.Position, t.Rotation.Forward );

	protected override void Activate()
	{
		// TODO: Bullet shooting with tracer effects.
	}
}
