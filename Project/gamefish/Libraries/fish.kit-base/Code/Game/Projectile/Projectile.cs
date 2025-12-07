using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// It gets thrown. It hurts enemies.
/// </summary>
[Icon( "rocket_launch" )]
public partial class Projectile : MovingEntity, Component.ICollisionListener, ITeam
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
	/// This trace is used when moving the projectile.
	/// </summary>
	[Property, WideMode, InlineEditor]
	[Feature( PROJECTILE ), Group( PHYSICS )]
	public TraceSettings TraceSettings { get; set; } = new(
		shape: TraceShape.Box,
		r: Rotation.Identity,
		hit: [TAG_PAWN],
		ignore: [TAG_TRIGGER]
	);


	/*
	/// <summary>
	/// Should the target play its own hurt effect upon taking damage?
	/// </summary>
	[Property]
	[Title( "Hurt Effects" )]
	[Feature( PROJECTILE ), Group( IMPACT )]
	public bool AllowHurtEffects { get; set; } = false;
	*/


	/// <summary> The sound played when spawned by equipment. </summary>
	[Property]
	[Feature( PROJECTILE ), Category( SOUNDS )]
	public SoundEvent FireSound { get; set; }

	/// <summary> The sound meant to be played continuously. </summary>
	[Property]
	[Feature( PROJECTILE ), Category( SOUNDS )]
	public SoundEvent LoopingSound { get; set; }


	[Sync]
	public Pawn Attacker { get; set; }

	[Sync]
	public Entity Source { get; set; }

	[Sync]
	public TimeSince SinceCreated { get; set; }


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
		if ( !ProjectileTargetSpeed.HasValue )
		{
			if ( Velocity != default )
				ProjectileTargetSpeed = Velocity.Length;
			else
				ProjectileTargetSpeed = DefaultSpeed;
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
		Move( deltaTime, isFixedUpdate: true );
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

	protected virtual IEnumerable<Pawn> GetEnemiesWithin( in Vector3 origin, in float radius )
	{
		if ( !Scene.IsValid() || !Team.IsValid() )
			return [];

		var trSphere = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject )
			.Sphere( radius, origin, origin ).RunAll();

		var enemies = trSphere
			.Select( tr => Pawn.TryGet<Pawn>( tr.GameObject, out var pawn ) ? pawn : null )
			.Where( pawn => pawn.IsValid() && Team.IsEnemy( pawn.Team ) )
			.Distinct();

		return enemies;
	}
}
