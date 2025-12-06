namespace GameFish;

partial class Projectile
{
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
		var tImpact = WorldTransform;

		tImpact.Position = impact.HitPosition;
		tImpact.Rotation = Rotation.LookAt( impact.HitNormal );

		PlayImpactEffect( tImpact );

		if ( !GameObject.IsValid() )
			return;

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
