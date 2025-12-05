using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// It gets thrown. It hurts enemies.
/// </summary>
[Icon( "rocket_launch" )]
public partial class Projectile : DynamicEntity, Component.ICollisionListener, ITeam
{
	protected const int PROJECTILE_ORDER = DEFAULT_ORDER - 2000;
	protected const int PROJECTILE_DEBUG_ORDER = PROJECTILE_ORDER - 100;

	/// <summary>
	/// The team to ignore collisions with.
	/// </summary>
	[Sync]
	[Property, JsonIgnore]
	[Order( PROJECTILE_DEBUG_ORDER )]
	[ShowIf( nameof( InGame ), true )]
	[Feature( PROJECTILE ), Group( DEBUG )]
	public Team Team
	{
		get => _team;
		protected set
		{
			_team = value;
			OnSetTeam( value );
		}
	}

	protected Team _team;

	/// <summary>
	/// Destroys the object if it's been going on for too long.
	/// </summary>
	[Property]
	[Order( PROJECTILE_ORDER - 1 )]
	[Title( "Self-Destruct Delay" )]
	[Range( 0f, 20f, clamped: false )]
	[Feature( PROJECTILE ), Group( TIMING )]
	public float SelfDestructDelay { get; set; } = 10f;

	/// <summary>
	/// Assigns this projectile's team.
	/// </summary>
	public virtual void SetTeam( Team team )
		=> Team = team;

	protected virtual void OnSetTeam( Team team )
		=> Team.UpdateTags( GameObject, team?.Tag );


	/// <summary>
	/// The speed to go if not otherwise specified.
	/// </summary>
	[Property]
	[Feature( PROJECTILE ), Group( PHYSICS )]
	[Range( 0f, 10000f, clamped: false ), Step( 1 )]
	public float DefaultSpeed { get; set; } = 1000f;

	[Property, WideMode, InlineEditor]
	[Feature( PROJECTILE ), Group( PHYSICS )]
	public TraceSettings TraceSettings { get; set; } = new(
		shape: TraceShape.Box,
		r: Rotation.Identity,
		hit: [TAG_PAWN],
		ignore: [TAG_TRIGGER]
	);

	/// <summary>
	/// Is impact damage dealt with effects?
	/// </summary>
	[Sync]
	[Property]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( HasImpact ), Label = IMPACT )]
	public bool HasImpact { get; set; } = true;

	[Property]
	[Title( "Sound" )]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( HasImpact ) )]
	public SoundEvent ImpactSound { get; set; }

	[Property]
	[Title( "Effect" )]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( HasImpact ) )]
	public PrefabFile ImpactPrefab { get; set; }

	[Title( "Damage" )]
	[Property, WideMode]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( HasImpact ) )]
	public DamageSettings ImpactDamage { get; set; } = new( [DamageTypes.IMPACT] )
	{
		EnableRange = true,
		EnableHitboxes = false,
	};


	/// <summary>
	/// Is explosive damage dealt with effects?
	/// </summary>
	[Sync]
	[Property]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( IsExplosive ), Label = EXPLOSIVE )]
	public bool IsExplosive { get; set; } = false;

	[Property]
	[Title( "Sound" )]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( IsExplosive ) )]
	public SoundEvent ExplosionSound { get; set; }

	[Property]
	[Title( "Effect" )]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( IsExplosive ) )]
	public PrefabFile ExplosionPrefab { get; set; }

	[Property]
	[Title( "Radius" )]
	[Feature( PROJECTILE )]
	[Range( 0f, 2048f, clamped: false )]
	[ToggleGroup( nameof( IsExplosive ) )]
	public float ExplosionRadius { get; set; } = 256f;


	/// <summary>
	/// The settings for how damage should be applied.
	/// </summary>
	[Title( "Damage" )]
	[Property, WideMode]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( IsExplosive ) )]
	public DamageSettings ExplosionDamage { get; set; } = new( [DamageTypes.EXPLOSIVE] )
	{
		EnableRange = true,
		EnableHitboxes = false,
	};


	/*
	/// <summary>
	/// Should the target play its own hurt effect upon taking damage?
	/// </summary>
	[Property]
	[Title( "Hurt Effects" )]
	[Feature( PROJECTILE ), Group( IMPACT )]
	public bool AllowHurtEffects { get; set; } = false;
	*/

	[Property]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( IsHoming ), Label = HOMING )]
	public bool IsHoming { get; set; } = false;

	/// <summary>
	/// <c>Min</c> = distance moving the farthest <br />
	/// <c>Max</c> = distance moving the slowest
	/// </summary>
	[Property]
	[Title( "Distance" )]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( IsHoming ) )]
	public FloatRange HomingDistance { get; set; } = new( 250f, 1500f );

	/// <summary>
	/// <c>Min</c> = turning speed while farthest <br />
	/// <c>Max</c> = turning speed while closest
	/// </summary>
	[Property]
	[Title( "Speed" )]
	[Feature( PROJECTILE )]
	[ToggleGroup( nameof( IsHoming ) )]
	public FloatRange HomingSpeed { get; set; } = new( 0f, 1000f );


	/// <summary> The sound played when spawned by equipment. </summary>
	[Property]
	[Feature( PROJECTILE ), Category( SOUNDS )]
	public SoundEvent FireSound { get; set; }

	/// <summary> The sound meant to be played continuously. </summary>
	[Property]
	[Feature( PROJECTILE ), Category( SOUNDS )]
	public SoundEvent LoopingSound { get; set; }


	[Property]
	[Feature( ENTITY ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public override Vector3 Velocity
	{
		get => ProjectileVelocity;
		set => ProjectileVelocity = value;
	}

	/// <summary>
	/// Overrides what speed the projectile should be moving.
	/// </summary>
	[Sync]
	public float? ProjectileSpeed { get; set; }


	[Sync]
	public Entity Attacker { get; set; }

	[Sync]
	public Entity Source { get; set; }

	/// <summary>
	/// How many times has this collided?
	/// </summary>
	[Sync]
	public int CollisionCount { get; set; }

	[Sync]
	public TimeSince SinceCreated { get; set; }

	[Sync]
	protected Vector3 ProjectileVelocity { get; set; }


	/// <summary>
	/// A consistent way of getting an <see cref="Projectile"/> from a <see cref="GameObject"/>.
	/// </summary>
	/// <returns> If the projectile was found. </returns>
	public static bool TryGet( GameObject obj, out Projectile proj )
	{
		if ( !obj.IsValid() )
		{
			proj = null;
			return false;
		}

		return obj.Components.TryGet( out proj, FindMode.EverythingInSelfAndAncestors );
	}


	protected override void OnEnabled()
	{
		base.OnEnabled();

		Tags?.Add( TAG_PROJECTILE );
	}

	protected override void OnStart()
	{
		base.OnStart();

		SinceCreated = 0;

		if ( !GameObject.IsValid() )
			return;

		// What speed is this meant to be going?
		if ( !ProjectileSpeed.HasValue )
		{
			ProjectileSpeed = Velocity != default
				? Velocity.Length
				: DefaultSpeed;
		}

		PlayLoopingSound();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		// You'd think this wouldn't be necessary..
		GameObject?.StopAllSounds();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( IsProxy )
			return;

		if ( SinceCreated > SelfDestructDelay )
		{
			if ( IsExplosive )
				PlayExplosionEffect( WorldTransform );
			else
				PlayImpactEffect( WorldTransform );

			GameObject?.Destroy();
			return;
		}

		var deltaTime = Time.Delta;

		UpdateVelocity( deltaTime );
		Move( deltaTime );
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( !GameObject.IsValid() )
			return;

		var c = Color.Magenta.Desaturate( 0.3f );

		TraceSettings.DrawGizmos( WorldTransform, cLines: c, cSolid: c.WithAlphaMultiplied( 0.2f ) );
	}

	/// <returns> If this projectile should destroy itself. </returns>
	public virtual bool IsFinished()
		=> !GameObject.IsValid() || CollisionCount > 0;

	/// <summary>
	/// Called when spawned by an equipment.
	/// </summary>
	public void OnSpawned( Pawn atkr, Equipment equip, EquipFunction func = null )
	{
		Attacker = atkr.AsValid();
		Source = equip.AsValid<Entity>() ?? func.AsValid<Entity>();

		if ( FireSound.IsValid() )
			BroadcastSound( FireSound );
	}

	protected virtual void PlayLoopingSound()
	{
		if ( LoopingSound.IsValid() )
			EmitSound( LoopingSound );
	}

	protected virtual void Move( in float deltaTime )
	{
		var startPos = WorldPosition;
		var move = Velocity * deltaTime;

		var trAll = TraceSettings.RunAll( GameObject, startPos, startPos + move );

		foreach ( var tr in trAll )
		{
			if ( !IsCollision( in tr ) )
				continue;

			if ( TryCollide( new ImpactData( GameObject, tr ) ) )
				if ( IsFinished() )
					return;
		}

		if ( GameObject.IsValid() )
			WorldPosition += move;
	}

	protected virtual bool IsCollision( in SceneTraceResult tr )
	{
		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		var objTags = tr.GameObject.Tags;

		if ( objTags is null )
			return false;

		if ( Team.IsValid() && Team.Tag.IsValidTag() && objTags.Has( Team.Tag ) )
			return false;

		// Ignore specific tags.
		if ( TraceSettings.TagsIgnore.HasAny( objTags ) )
		{
			// .. unless have a tag meant to be hit.
			if ( !TraceSettings.TagsHit.HasAny( objTags ) )
				return false;
		}

		return true;
	}

	protected virtual void UpdateVelocity( in float deltaTime )
	{
		// Homing missiles.
		if ( !IsHoming )
			return;

		// Find the nearest enemy.
		var projPos = Center;

		var (target, dist) = GetEnemiesWithin( projPos, HomingDistance.Max )
			.Where( enemy => enemy.IsValid() && enemy.Active )
			.Select( enemy => (enemy, projPos.Distance( enemy.Center )) )
			.OrderBy( tuple => tuple.Item2 )
			.FirstOrDefault();

		if ( !target.IsValid() || dist > HomingDistance.Max )
			return;

		var targetPos = target.Center + (target.Velocity * deltaTime);
		var dir = Center.Direction( targetPos );

		var projSpeed = ProjectileSpeed ??= DefaultSpeed;
		var homingSpeed = HomingDistance.Remap( dist, HomingSpeed.Max, HomingSpeed.Min );

		var homingVel = dir * homingSpeed * deltaTime;

		var oldVel = Velocity;

		Velocity = (oldVel + homingVel).Normal * projSpeed;
	}

	protected virtual IEnumerable<Pawn> GetEnemiesWithin( in Vector3 origin, in float radius )
	{
		if ( !Scene.IsValid() )
			return [];

		var trSphere = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject )
			.Sphere( radius, origin, origin ).RunAll()
			.Select( tr => Pawn.TryGet<Pawn>( tr.GameObject, out var pawn ) ? pawn : null )
			.Where( pawn => pawn.IsValid() && pawn.IsEnemy( Team ) );

		return trSphere;
	}

	void ICollisionListener.OnCollisionStart( Collision c )
	{
		if ( GameObject.IsValid() && GameObject.Active )
			TryCollide( c );
	}

	/// <returns> If the collision was possible and allowed. </returns>
	public virtual bool TryCollide( in ImpactData impact )
	{
		if ( !GameObject.IsValid() || !Active )
			return false;

		if ( !impact.IsValid )
			return false;

		var hitObj = impact.GameObject;

		if ( !hitObj.IsValid() )
			return false;

		if ( impact.EndPosition.HasValue )
			WorldPosition = impact.EndPosition.Value;

		if ( IsFinished() )
			goto Finish;

		CollisionCount++;

		if ( HasImpact )
			DoImpact( in impact );

		if ( IsExplosive )
			DoExplosion( in impact );

		Finish:

		// Exploding on contact for now.
		GameObject?.Destroy();

		return true;
	}

	protected virtual void DoImpact( in ImpactData impact )
	{
		if ( !GameObject.IsValid() )
			return;

		var tImpact = WorldTransform;

		tImpact.Position = impact.HitPosition;
		tImpact.Rotation = Rotation.LookAt( impact.HitNormal );

		PlayImpactEffect( tImpact );

		DoImpactDamage( in impact );
		DoImpactForce( in impact );
	}

	protected virtual void DoImpactDamage( in ImpactData impact )
	{
		var target = impact.GameObject;

		if ( !target.IsValid() )
			return;

		var dmg = ImpactDamage.BaseDamage;

		if ( dmg is 0f )
			return;

		var objAtkr = Attacker?.GameObject ?? GameObject;
		var objSource = Source?.GameObject ?? GameObject;

		var info = new DamageInfo( dmg, objAtkr, objSource )
			.WithTags( ImpactDamage.Types );

		target.TryDamage( info );

		/*
		if ( obj.TryDamage( info ) && AllowHurtEffects )
			if ( obj.Components.Get<DamageModule>( FindMode.EnabledInSelfAndDescendants ) is var dm )
				dm?.PlayHitEffect( hitPos + hitNormal * 10f, Rotation.LookAt( -hitNormal ) );
		*/
	}

	protected virtual void DoImpactForce( in ImpactData impact )
	{
		if ( !GameObject.IsValid() )
			return;

		var target = impact.GameObject;

		if ( !target.IsValid() )
			return;

		if ( ImpactDamage.Impulse is 0 )
			return;

		var moveDir = Velocity.Normal;
		var dmg = ImpactDamage.BaseDamage;

		var force = ImpactDamage.GetImpulse( moveDir, in dmg );

		if ( force == Vector3.Zero )
			return;

		if ( target.Components.TryGet<IVelocity>( out var iVel, FindMode.EnabledInSelf | FindMode.InAncestors ) )
			iVel.Velocity += force;
		else if ( target.Components.TryGet<Rigidbody>( out var rb, FindMode.EnabledInSelf | FindMode.InAncestors ) )
			rb.Velocity += force;
	}

	protected virtual void DoExplosion( in ImpactData impact )
	{
		PlayExplosionEffect( WorldTransform );

		if ( !GameObject.IsValid() )
			return;

		var baseDamage = ImpactDamage.BaseDamage;

		if ( baseDamage is 0 )
			return;

		var objAtkr = Attacker?.GameObject ?? GameObject;
		var objSource = Source?.GameObject ?? GameObject;

		var origin = impact.EndPosition ?? Center;

		foreach ( var enemy in GetEnemiesWithin( origin, ExplosionRadius ) )
		{
			if ( !enemy.IsValid() || !enemy.Active )
				continue;

			var dmg = ExplosionDamage.GetRangeDamage( in origin, enemy.Center );

			var info = new DamageInfo( dmg, objAtkr, objSource )
				.WithTags( ImpactDamage.Types );

			enemy.TryDamage( info );
		}
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.SendImmediate | NetFlags.OwnerOnly )]
	public virtual void PlayImpactEffect( Transform t )
	{
		if ( ImpactSound.IsValid() )
			Sound.Play( ImpactSound, t.Position );

		ImpactPrefab.TrySpawn( t, out var _ );
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.SendImmediate | NetFlags.OwnerOnly )]
	public virtual void PlayExplosionEffect( Transform t )
	{
		if ( ExplosionSound.IsValid() )
			Sound.Play( ExplosionSound, t.Position );

		ExplosionPrefab.TrySpawn( t, out var _ );
	}
}
