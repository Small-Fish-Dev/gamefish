namespace GameFish;

/// <summary>
/// Information about damage being dealt.
/// <br /> <br />
/// <b> NOTE: </b> Has more useful data than <see cref="DamageInfo"/>.
/// </summary>
[Group( Library.NAME )]
public partial struct DamageData
{
	public float Damage { get; set; }
	public Vector3? Impulse { get; set; }

	public TagSet Types { get; set; } = [];

	public Vector3? Origin { get; set; }
	public Vector3? HitPosition { get; set; }

	public Hitbox Hitbox { get; set; }
	public PhysicsShape Shape { get; set; }

	public Pawn Attacker { get; set; }
	public Entity Inflictor { get; set; }

	/// <summary>
	/// An identifying source string.
	/// Could be an entity class name.
	/// </summary>
	public string Source { get; set; }

	public static implicit operator DamageData( in DamageInfo info )
		=> new( in info );

	public DamageData() { }

	/// <summary>
	/// Auto-converts <see cref="DamageInfo"/> to <see cref="DamageData"/>.
	/// </summary>
	public DamageData( in DamageInfo info )
	{
		Damage = info.Damage;
		Impulse = null;

		Types = info.Tags;

		Origin = info.Origin;
		HitPosition = info.Position;

		Hitbox = info.Hitbox;
		Shape = info.Shape;

		if ( info.Attacker.IsValid() )
			if ( Pawn.TryGet( info.Attacker, out var pawn ) )
				Attacker = pawn;

		if ( info.Weapon.IsValid() )
			if ( info.Weapon.Components.TryGet<Equipment>( out var equip, FindMode.EnabledInSelf | FindMode.InAncestors ) )
				Inflictor = equip;

		if ( Inflictor.IsValid() && Inflictor.IsClass )
			Source = Inflictor.ClassId;
	}

	public static DamageData FromBullet( in DamageSettings s, in SceneTraceResult tr, Equipment equip = null )
	{
		var data = new DamageData();

		var startPos = tr.StartPosition;
		var hitPos = tr.HitPosition;

		data.Origin = startPos;
		data.HitPosition = hitPos;

		data.Damage = s.GetRangeDamage( in startPos, in hitPos );
		data.Impulse = s.GetImpulse( tr.Direction, data.Damage );

		data.Types = s.Types;

		data.Hitbox = tr.Hitbox;
		data.Shape = tr.Shape;

		if ( equip.IsValid() )
		{
			data.Attacker = equip.Pawn;
			data.Inflictor = equip;

			if ( equip.IsClass )
				data.Source = equip.ClassId;
		}

		if ( data.Source.IsBlank() )
			data.Source = DamageTypes.BULLET;

		return data;
	}

	public static DamageData FromImpact( in DamageSettings s, in ImpactData impact, DynamicEntity phys, Pawn atkr = null )
	{
		var data = new DamageData();

		// Use physics entity mass center as origin.
		var origin = impact.EndPosition
			?? phys.AsValid()?.MassCenter;

		// Direct impacts shouldn't be ranged based.
		data.Damage = s.BaseDamage;

		// Apply impulse settings.
		var vel = phys?.Velocity.Normal ?? Vector3.Zero;
		data.Impulse = s.GetImpulse( vel, data.Damage );

		// Always use impact damage type.
		data.Types = (s.Types ?? []).With( IMPACT );

		data.Origin = origin;
		data.HitPosition = impact.HitPosition;

		data.Hitbox = impact.Hitbox;
		data.Shape = impact.Shape;

		data.Attacker = atkr;
		data.Inflictor = phys;

		if ( phys.IsValid() && phys.IsClass )
			data.Source = phys.ClassId;

		if ( data.Source.IsBlank() )
			data.Source = DamageTypes.IMPACT;

		return data;
	}

	public static DamageData FromExplosion( in DamageSettings s, in Vector3 origin, in Vector3 hitPos, Equipment equip = null, Pawn atkr = null )
	{
		var data = new DamageData();

		var dmg = s.GetRangeDamage( in origin, in hitPos );
		data.Damage = dmg;

		// Apply impulse settings.
		data.Impulse = s.GetImpulse( origin.Direction( hitPos ), dmg );

		// Always use impact damage type.
		data.Types = (s.Types ?? []).With( IMPACT );

		data.Origin = origin;
		data.HitPosition = hitPos;

		data.Attacker = atkr;

		if ( equip.IsValid() )
		{
			data.Inflictor = equip;

			if ( equip.IsClass )
				data.Source = equip.ClassId;
		}

		if ( data.Source.IsBlank() )
			data.Source = DamageTypes.EXPLOSIVE;

		return data;
	}
}
