namespace GameFish;

partial class Projectile
{
	/// <summary>
	/// If enabled: ignore collision with teammates.
	/// </summary>
	[Property]
	[Feature( PROJECTILE ), Group( COLLISION ), Order( COLLISION_ORDER )]
	public bool IgnoreTeam { get; set; } = true;

	/// <summary>
	/// Settings used when moving to trace for collision.
	/// </summary>
	[Property, WideMode, InlineEditor]
	[Feature( PROJECTILE ), Group( COLLISION ), Order( COLLISION_ORDER + 1 )]
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
		if ( GameObject.IsValid() && GameObject.Active )
			TryCollide( c );
	}

	protected virtual bool IsCollision( in SceneTraceResult tr )
	{
		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		if ( IgnoreTeam && Team.IsValid() )
			if ( tr.GameObject.IsTeam( Team ) )
				return false;

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

		if ( !GameObject.IsValid() )
			return;

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

		if ( !target.TryDamage( in data ) )
			return;

		// TEMP: Manual impulse. Should be in `ApplyDamage`.
		if ( data.Impulse is Vector3 vel )
			if ( target.Components.TryGet<IVelocity>( out var iVel, FindMode.EnabledInSelf | FindMode.InAncestors ) )
				iVel.TryImpulse( vel );

		/*
		if ( obj.TryDamage( info ) && AllowHurtEffects )
			if ( obj.Components.Get<DamageModule>( FindMode.EnabledInSelfAndDescendants ) is var dm )
				dm?.PlayHitEffect( hitPos + hitNormal * 10f, Rotation.LookAt( -hitNormal ) );
		*/
	}

	protected virtual void DoExplosion( in ImpactData impact )
	{
		PlayExplosionEffect( WorldTransform );

		if ( !GameObject.IsValid() )
			return;

		var origin = impact.EndPosition ?? Center;

		foreach ( var enemy in GetEnemiesWithin( origin, ExplosionRadius ) )
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
}
