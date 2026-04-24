namespace GameFish;

partial class Projectile : Component.ICollisionListener
{
	/// <summary>
	/// Forces collision detection to be handled by whoever owns the other thing.
	/// <br /> <br />
	/// <b> NOTE: </b> Important if shot by an NPC,
	/// otherwise you should really keep this disabled.
	/// <br /> <br />
	/// <b> EXPLANATION: </b> This makes it so that the owner of the projectile
	/// doesn't bother trying to hit stuff owned by other connections and that
	/// they will perform the collision detection on their end. This solves the
	/// problem of getting hit by NPC projectiles you have clearly dodged
	/// on your end as a client due to lag.
	/// </summary>
	[Sync]
	[Property]
	[Title( "Proxy Hit" )]
	[Order( COLLISION_ORDER - 1 )]
	[Feature( PROJECTILE ), Group( COLLISION )]
	public bool ProxyCollision { get; set; }

	/// <summary>
	/// If enabled: ignore collision with teammates.
	/// </summary>
	[Property]
	[Order( COLLISION_ORDER - 1 )]
	[Feature( PROJECTILE ), Group( COLLISION )]
	public bool IgnoreTeam { get; set; } = true;

	/// <summary>
	/// The team to ignore collisions with.
	/// <br /> <br />
	/// <b> NOTE: </b> This is typically set upon spawning,
	/// so you're only really putting in a default value here.
	/// </summary>
	[Sync]
	[Property]
	[Order( COLLISION_ORDER - 1 )]
	[Feature( PROJECTILE ), Group( COLLISION )]
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
	/// Assigns this projectile's team.
	/// </summary>
	public virtual void SetTeam( Team team )
		=> Team = team;

	protected virtual void OnSetTeam( Team team )
		=> Team.UpdateTags( GameObject, team?.Tag );

	/// <summary>
	/// Settings used when moving to trace for collision.
	/// </summary>
	[Order( COLLISION_ORDER + 1 )]
	[Property, WideMode, InlineEditor]
	[Feature( PROJECTILE ), Group( COLLISION )]
	public TraceSettings TraceSettings { get; set; }


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


	/// <summary>
	/// How many times has this collided?
	/// </summary>
	[Sync]
	public int CollisionCount { get; set; }


	void ICollisionListener.OnCollisionStart( Collision c )
	{
		if ( !GameObject.IsValid() || !Active )
			return;

		TryCollide( c );
	}


	public virtual bool IsCollision( in SceneTraceResult tr )
	{
		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		if ( IgnoreTeam && Team.IsValid() )
			if ( tr.GameObject.IsTeam( Team ) )
				return false;

		if ( ProxyCollision )
		{
			// In proxy mode only the hit object's owner can impact.
			if ( tr.GameObject.IsProxy )
				return false;
		}
		else if ( IsProxy )
		{
			// Otherwise only the owner can check for collision.
			return false;
		}

		return true;
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

		// Proxy collision should defer to the owner still.
		if ( IsProxy )
		{
			OnProxyCollision( impact );
			return true;
		}

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

		if ( DebugLogging )
			this.Log( $"Hit object:[{impact.GameObject}]" );

		// Exploding on contact for now.
		GameObject?.Destroy();

		return true;
	}

	protected virtual void DoImpact( in ImpactData impact )
	{
		var tImpact = WorldTransform;

		tImpact.Position = impact.HitPosition;
		tImpact.Rotation = Rotation.LookAt( impact.HitNormal );

		PlayImpactEffect( tImpact );

		DoImpactDamage( in impact );
	}

	protected virtual void DoImpactDamage( in ImpactData impact )
	{
		var target = impact.GameObject;

		if ( !target.IsValid() )
			return;

		if ( ImpactDamage.BaseDamage == 0f && ImpactDamage.Impulse == 0f )
			return;

		var data = DamageData.FromImpact( ImpactDamage, in impact, this, Attacker );

		if ( target.TryDamage( in data ) )
			OnImpactDamage( in data );
	}

	protected virtual void OnImpactDamage( in DamageData data )
	{
	}

	protected virtual void DoExplosion( in ImpactData impact )
	{
		PlayExplosionEffect( WorldTransform );

		var origin = impact.EndPosition ?? Center;

		foreach ( var enemy in FindEnemiesWithin( origin, ExplosionRadius ) )
		{
			if ( !enemy.IsValid() || !enemy.Active )
				continue;

			var ePos = enemy.Center;
			var dir = origin.Direction( ePos );

			var dmg = ExplosionDamage.GetRangeDamage( in origin, ePos );
			var impulse = ImpactDamage.GetImpulse( dir, in dmg );

			var data = new DamageData( dmg, impulse, Attacker, Source, ImpactDamage.Types );

			enemy.TrySendDamage( data );
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

	protected virtual void OnProxyCollision( ImpactData impact )
	{
		// TODO: Prevent RPC/impact spam from lag.
		RpcCollide( impact );
	}

	[Rpc.Owner( NetFlags.Reliable | NetFlags.SendImmediate )]
	protected void RpcCollide( ImpactData impact )
		=> TryCollide( in impact );
}
