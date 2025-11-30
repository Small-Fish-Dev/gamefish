using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Information for how a source of damage should be applied.
/// </summary>
[Group( Library.NAME )]
public partial struct DamageSettings
{
	const int RANGE_ORDER = 5;
	const int FORCE_ORDER = 10;
	const int HITBOXES_ORDER = 15;

	/// <summary>
	/// The baseline/maximum damage dealt.
	/// </summary>
	[KeyProperty]
	[Title( "Base" )]
	public float BaseDamage { get; set; } = 20f;


	/// <summary>
	/// Damage type tags.
	/// </summary>
	[KeyProperty]
	public TagSet Types { get; set; }

	/// <summary>
	/// Allows applying forces on hit.
	/// </summary>
	[KeyProperty]
	[Order( FORCE_ORDER )]
	[ToggleGroup( nameof( EnableForces ), Label = FORCE )]
	public bool EnableForces { get; set; }

	/// <summary>
	/// Should force be scaled by damage dealt?
	/// </summary>
	[KeyProperty]
	[Title( "Scaling" )]
	[Order( FORCE_ORDER )]
	[ToggleGroup( nameof( EnableForces ) )]
	public bool ScaleForces { get; set; }

	/// <summary>
	/// Push them back this much.
	/// </summary>
	[Order( FORCE_ORDER )]
	[KeyProperty]
	[ToggleGroup( nameof( EnableForces ) )]
	public float Impulse { get; set; }


	/// <summary>
	/// Allows scaling damage by distance.
	/// </summary>
	[Order( RANGE_ORDER )]
	[ToggleGroup( nameof( EnableRange ), Label = "🏹 Range" )]
	public bool EnableRange { get; set; }

	/// <summary>
	/// Multiplier of damage by distance. <br />
	/// <c>x</c> = distance <br />
	/// <c>y</c> = multiplier
	/// </summary>
	[WideMode]
	[Order( RANGE_ORDER )]
	[ToggleGroup( nameof( EnableRange ) )]
	public Curve Range { get; set; } = new Curve( new( 0f, 1f ), new( 1f, 0f ) )
	{
		TimeRange = new( 0f, 2000f )
	};


	/// <summary>
	/// Allows scaling damage per hitbox.
	/// </summary>
	[Order( HITBOXES_ORDER )]
	[ToggleGroup( nameof( EnableHitboxes ), Label = "🥊 Hitboxes" )]
	public bool EnableHitboxes { get; set; }

	/// <summary>
	/// Multiplies damage dealt for each hitbox found.
	/// </summary>
	[WideMode]
	[Order( HITBOXES_ORDER )]
	[ToggleGroup( nameof( EnableHitboxes ) )]
	public Dictionary<string, float> HitboxMultipliers { get; set; } = new()
	{
		["head"] = 2f,
	};


	public DamageSettings() { }

	public DamageSettings( TagSet tags, in bool useRange = false, in bool useForces = false, in bool useHitboxes = false )
	{
		Types = tags ?? [];

		EnableRange = useRange;
		EnableForces = useForces;
		EnableHitboxes = useHitboxes;
	}

	public DamageSettings( params IEnumerable<string> tags )
		=> Types = [.. tags?.Where( tag => tag.IsValidTag() ) ?? []];

	/// <summary>
	/// Tells you how much force to apply with respect to its force settings.
	/// </summary>
	/// <param name="dir"> The direction to apply the force. </param>
	/// <param name="dmg"> The damage that was dealt. </param>
	/// <returns> The amount of force to apply in the attack direction. </returns>
	public readonly Vector3 GetForce( in Vector3 dir, in float dmg )
	{
		if ( !EnableForces )
			return Vector3.Zero;

		var force = dir * Impulse;

		if ( ScaleForces )
			force *= dmg;

		return force;
	}

	/// <returns> The damage that should be dealt with respect to range. </returns>
	public readonly float GetRangeDamage( in Vector3 from, in Vector3 to )
	{
		if ( !EnableRange )
			return BaseDamage;

		return GetDamageFromDistance( from.Distance( in to ) );
	}

	/// <returns> The damage that should be dealt(assuming range is enabled). </returns>
	public readonly float GetDamageFromDistance( in float distance )
		=> BaseDamage * Range.Evaluate( distance );
}
