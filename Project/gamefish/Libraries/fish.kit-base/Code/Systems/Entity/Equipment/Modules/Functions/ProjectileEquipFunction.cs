using System;

namespace GameFish;

/// <summary>
/// 🚀 An equipment functionality module for shooting projectiles.
/// </summary>
[Icon( "rocket_launch" )]
[Title( "Projectile Shooter" )]
public partial class ProjectileEquipFunction : EquipFunction
{
	/// <summary>
	/// Play this sound upon successfully firing a projectile.
	/// </summary>
	[Property]
	[Title( "Sound" )]
	[Feature( MODULE ), Group( PROJECTILE )]
	public virtual SoundEvent ShootSound { get; set; }

	[Property]
	[Title( "Prefab" )]
	[Feature( MODULE ), Group( PROJECTILE )]
	public virtual PrefabFile ProjectilePrefab { get; set; } = Prefab.GetFile( "prefabs/gamefish/kit_fps/projectiles/missile1.prefab" );

	/// <summary>
	/// The projectile's velocity relative to the owner's aim. <br />
	/// The <c>X</c> axis is forward speed.
	/// </summary>
	[Property]
	[Title( "Velocity" )]
	[Feature( MODULE ), Group( PROJECTILE )]
	public virtual Vector3 ProjectileVelocity { get; set; } = new Vector3( 1500f, 0f, 0f );

	/// <summary>
	/// Offsets the projectile's spawning transform by this position/rotation.
	/// </summary>
	[Property]
	[InlineEditor]
	[Title( "Spawn Offset" )]
	[Feature( MODULE ), Group( PROJECTILE )]
	public virtual Offset ProjectileOffset { get; set; } = new( Vector3.Forward * 16f, Rotation.Identity );

	/// <summary>
	/// The angle in degrees of the random spread cone.
	/// </summary>
	[Property]
	[Title( "Cone" )]
	[Feature( MODULE ), Group( SPREAD )]
	public virtual float AimSpreadCone { get; set; } = 0f;

	/// <summary>
	/// If enabled: force the scale. <br />
	/// Otherwise it will use the scale of the prefab itself.
	/// </summary>
	[Property]
	[Feature( MODULE ), Group( PROJECTILE )]
	[ToggleGroup( nameof( HasScaleOverride ), Label = "Scale Override" )]
	public virtual bool HasScaleOverride { get; set; }

	/// <summary>
	/// What to override the spawned prefab's scale with.
	/// </summary>
	[Property]
	[Title( "Scale" )]
	[Feature( MODULE )]
	[ToggleGroup( nameof( HasScaleOverride ) )]
	public virtual Vector3 ProjectileScale { get; set; } = Vector3.One;

	/// <summary>
	/// If enabled: NPCs will aim towards where the target will be.
	/// </summary>
	[Property]
	[Title( "Prediction" )]
	[Feature( NPC ), Group( PROJECTILE )]
	public virtual bool AimPrediction { get; set; }

	/*
	/// <summary>
	/// What min/max fraction to aim before/ahead of the target's destination.
	/// </summary>
	[Property]
	[Title( "Prediction Range" )]
	[Feature( NPC ), Group( PROJECTILE )]
	public virtual FloatRange AimPredictionRange { get; set; }
	*/

	public override bool TryActivate()
	{
		if ( !ProjectilePrefab.IsValid() )
			return false;

		return base.TryActivate();
	}

	protected override void Activate()
	{
		var tAim = AimTransform;

		if ( AimSpreadCone != 0f )
			tAim.Rotation *= GetSpreadConeRotation( AimSpreadCone );

		if ( TrySpawnProjectile( out var _, tAim ) )
			PlayActivationEffect( tAim );
	}

	protected override void PlayActivationEffect( in Transform tOrigin )
	{
		var obj = Parent?.GameObject ?? GameObject;
		var s = SoundSettings.InWorld( obj, tOrigin.Position );

		TryPlaySound( ShootSound, s );
	}

	/// <summary>
	/// Allows you to offset the default projectile origin/direction.
	/// </summary>
	/// <param name="tAim"> The direction it'll be aimed. </param>
	/// <returns> The projectile's origin and direction as a transform. </returns>
	public virtual Transform GetProjectileOrigin( in Transform? tAim = null )
	{
		var tDir = tAim ?? AimTransform;
		var tOrigin = tDir;

		return tOrigin;
	}

	/// <returns> The velocity to apply relative to an aiming rotation. </returns>
	public virtual Vector3 GetProjectileVelocity( in Transform tAim )
		=> ProjectileVelocity != Vector3.Zero
			? tAim.Rotation * ProjectileVelocity
			: Vector3.Zero;

	/// <summary>
	/// Tries to spawn a projectile in this direction.
	/// </summary>
	/// <param name="proj"> The resulting object(or null). </param>
	/// <param name="tAim"> The direction to launch it. </param>
	/// <returns> If the projectile could be spawned. </returns>
	public virtual bool TrySpawnProjectile( out GameObject proj, in Transform? tAim = null )
	{
		var tOrigin = GetProjectileOrigin( tAim ?? AimTransform );

		proj = SpawnProjectile(
			prefab: ProjectilePrefab,
			tAim: tOrigin,
			offset: ProjectileOffset,
			scale: HasScaleOverride ? ProjectileScale : null
		);

		return proj.IsValid();
	}

	/// <summary>
	/// Spawns this weapon's projectile at the origin with an optional overrides.
	/// Optionally sets the object's team and network spawns it.
	/// </summary>
	/// <param name="prefab"> The projectile's prefab. </param>
	/// <param name="tAim"> The position and direction. </param>
	/// <param name="offset"> Adds position/rotation relative to <paramref name="tAim"/>(if defined). </param>
	/// <param name="scale"> Overrides the final scaling of the prefab(if defined). </param>
	/// <param name="setTeam"> Set the projectile's team? </param>
	/// <param name="netSpawn"> Force it to be network spawned? </param>
	/// <returns></returns>
	protected virtual GameObject SpawnProjectile( PrefabFile prefab, in Transform tAim, in Offset? offset = null, in Vector3? scale = null, bool setTeam = true, bool netSpawn = true )
	{
		if ( !prefab.TrySpawn( tAim.Position, tAim.Rotation, out var proj ) )
		{
			this.Warn( $"Tried to spawn missing/invalid prefab:[{prefab}]" );
			return null;
		}

		// Aim Offset
		if ( offset.HasValue )
		{
			var tProj = proj.WorldTransform;
			var tOffset = tProj.ToLocal( tAim.WithOffset( offset.Value ) );

			proj.WorldTransform = tProj.WithOffset( tOffset );
		}

		// Scale Override
		if ( scale.HasValue )
			proj.WorldScale = scale.Value;

		// Team Assignment
		if ( setTeam && Pawn?.Team is Team team && team.IsValid() )
			proj.SetTeam( team, FindMode.EverythingInSelfAndDescendants );

		// Apply Velocity
		OnSpawnProjectile( proj, tAim );

		// Network Setup
		if ( netSpawn && proj.IsValid() && !proj.Network.Active )
			proj.NetworkSetup( Network.Owner, NetworkOrphaned.Destroy );

		return proj;
	}

	/// <summary>
	/// Sets up the projectile before it has been network spawned.
	/// </summary>
	/// <param name="proj"> The spawned projectile's object. </param>
	/// <param name="tOrigin"> The origin and launch direction. </param>
	protected virtual void OnSpawnProjectile( GameObject proj, in Transform tOrigin )
	{
		if ( !proj.IsValid() )
			return;

		if ( proj.Components.TryGet<IVelocity>( out var iVel ) )
			iVel.Velocity = GetProjectileVelocity( tOrigin );
		else if ( proj.Components.TryGet<Rigidbody>( out var rb ) )
			rb.Velocity = GetProjectileVelocity( tOrigin );
	}

	public override Vector3? GetTargetAimPosition( Pawn pawn, in Vector3? aimAt = null, in bool clampLength = true )
	{
		if ( AimPrediction && pawn.IsValid() )
			return GetPredictedTargetPosition( aimAt ?? pawn.Center, pawn.Velocity );

		return base.GetTargetAimPosition( pawn, aimAt );
	}

	/// <returns> A position that may be more likely to hit a moving target. </returns>
	public Vector3 GetPredictedTargetPosition( in Vector3 targetOrigin, in Vector3 targetVel, in bool clampLength = true )
	{
		var tAim = GetProjectileOrigin();
		var projVel = GetProjectileVelocity( tAim );
		var targetPos = GetPredictedTargetPosition( tAim.Position, projVel, targetOrigin, targetVel );

		// They may not be allowed to shoot far enough given certain angles.
		if ( clampLength )
		{
			var aimPos = AimPosition;
			var dist = aimPos.Length;

			// Clamp the length from the eye position to pass distance checks.
			if ( !UsableAtDistance( dist ) )
			{
				var delta = targetPos - aimPos;
				dist = (UsableRange.Max - 16f).Positive();
				return aimPos + (aimPos.Direction( targetPos ) * dist);
			}
		}

		return targetPos;
	}

	/// <returns> A position that may be more likely to hit a moving target. </returns>
	public virtual Vector3 GetPredictedTargetPosition( in Vector3 projOrigin, in Vector3 projVel, in Vector3 targetOrigin, in Vector3 targetVel )
	{
		// Some vibecoded shit I don't understand but works well enough.
		const float epsilon = 1e-6f;

		var projSpeed = MathF.Sqrt( Vector3.Dot( projVel, projVel ) );

		if ( projSpeed <= epsilon )
			return targetOrigin;

		var r = targetOrigin - projOrigin;
		var v = targetVel;

		var r2 = Vector3.Dot( r, r );

		if ( r2 <= epsilon )
			return targetOrigin;

		var v2 = Vector3.Dot( v, v );

		if ( v2 <= epsilon )
		{
			// Target stationary: aim at current position (optionally lead by travel time)
			var dist = MathF.Sqrt( r2 );
			var tStatic = dist / projSpeed;
			return targetOrigin + v * tStatic; // v is zero, so this is targetOrigin
		}

		// Solve |r + v*t| = s * t  =>  (v·v - s^2) t^2 + 2 (r·v) t + (r·r) = 0
		var s2 = projSpeed * projSpeed;
		var a = v2 - s2;
		var b = 2f * Vector3.Dot( r, v );
		var c = r2;

		float t;

		if ( MathF.Abs( a ) < epsilon )
		{
			// Linear case: a ~ 0 => b t + c = 0
			if ( MathF.Abs( b ) < epsilon )
			{
				t = MathF.Sqrt( r2 ) / projSpeed;
			}
			else
			{
				var tLin = -c / b;
				t = tLin > 0f ? tLin : (MathF.Sqrt( r2 ) / projSpeed);
			}
		}
		else
		{
			var disc = b * b - 4f * a * c;

			if ( disc < 0f )
			{
				t = MathF.Sqrt( r2 ) / projSpeed;
			}
			else
			{
				var sqrtD = MathF.Sqrt( disc );
				var t1 = (-b + sqrtD) / (2f * a);
				var t2 = (-b - sqrtD) / (2f * a);

				// choose smallest positive root
				t = float.PositiveInfinity;

				if ( t1 > 0f && t1 < t ) t = t1;
				if ( t2 > 0f && t2 < t ) t = t2;
				if ( float.IsPositiveInfinity( t ) )
					t = MathF.Sqrt( r2 ) / projSpeed;
			}
		}

		return targetOrigin + v * t;
	}
}
