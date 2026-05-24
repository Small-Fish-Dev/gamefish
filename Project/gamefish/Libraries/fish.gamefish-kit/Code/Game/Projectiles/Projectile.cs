using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// It gets launched. It hurts enemies.
/// </summary>
[Icon( "rocket_launch" )]
public partial class Projectile : DynamicEntity, ITeam
{
	protected const int PROJECTILE_ORDER = DEFAULT_ORDER - 2000;
	protected const int PROJECTILE_DEBUG_ORDER = PROJECTILE_ORDER - 100;

	protected const int COLLISION_ORDER = PROJECTILE_ORDER - 10;

	/// <summary>
	/// If true: log what we hit.
	/// </summary>
	[Property]
	[Title( "Logging" )]
	[Order( PROJECTILE_DEBUG_ORDER )]
	[Feature( PROJECTILE ), Group( DEBUG )]
	public bool DebugLogging { get; set; }

	/// <summary>
	/// If true: render trace gizmos in-editor.
	/// <br /> <br />
	/// <b> NOTE: </b> Disable this for better visibility.
	/// </summary>
	[Property]
	[JsonIgnore]
	[Title( "Render Trace" )]
	[Order( PROJECTILE_DEBUG_ORDER )]
	[Feature( PROJECTILE ), Group( DEBUG )]
	public bool DebugRenderTrace { get; set; } = true;

	/// <summary>
	/// Destroys the object if it's been going on for too long.
	/// </summary>
	[Property]
	[Order( PROJECTILE_ORDER - 1 )]
	[Title( "Self-Destruct Delay" )]
	[Range( 0f, 20f, clamped: false )]
	[Feature( PROJECTILE ), Group( TIMING )]
	public float SelfDestructDelay { get; set; } = 10f;


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

		InitializePhysics();

		PlayFiringSound();
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

		if ( !GameObject.IsValid() )
			return;

		var deltaTime = Time.Delta;

		if ( IsProxy )
		{
			if ( ProxyCollision )
			{
				var startPos = WorldPosition;
				var move = Velocity * deltaTime;

				TryCollide( startPos, startPos + move, out _ );
			}

			return;
		}

		if ( SinceCreated > SelfDestructDelay )
		{
			if ( IsExplosive )
				PlayExplosionEffect( WorldTransform );
			else
				PlayImpactEffect( WorldTransform );

			GameObject?.Destroy();

			return;
		}

		UpdateVelocity( deltaTime );
		Move( deltaTime, isFixedUpdate: true );
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( !GameObject.IsValid() )
			return;

		RenderTraceGizmos();
	}

	protected virtual void RenderTraceGizmos()
	{
		if ( !DebugRenderTrace )
			return;

		var c = Color.Magenta.Desaturate( 0.3f );

		TraceSettings.DrawGizmos( WorldTransform, cLines: c, cSolid: c.WithAlphaMultiplied( 0.2f ) );
	}

	/// <summary>
	/// Determines stuff like target velocity on start.
	/// </summary>
	protected virtual void InitializePhysics()
	{
		if ( !ProjectileTargetSpeed.HasValue )
		{
			if ( Velocity != default )
				ProjectileTargetSpeed = Velocity.Length;
			else
				ProjectileTargetSpeed = DefaultSpeed;
		}
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
	}

	protected virtual void PlayFiringSound()
	{
		if ( FireSound.IsValid() )
			BroadcastSound( FireSound );
	}

	protected virtual void PlayLoopingSound()
	{
		if ( LoopingSound.IsValid() )
			EmitSound( LoopingSound );
	}
}
