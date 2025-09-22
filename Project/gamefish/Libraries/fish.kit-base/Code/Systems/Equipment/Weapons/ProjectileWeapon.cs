using System;

namespace GameFish;

/// <summary>
/// 🚀 Shoots projectiles.
/// </summary>
[Icon( "rocket_launch" )]
public partial class ProjectileWeapon : AmmoEquip
{
	[Property]
	[Feature( WEAPON ), Group( GROUP_PROJECTILE )]
	public virtual float Speed { get; set; } = 1500f;

	[Property]
	[Title( "Prefab" )]
	[Feature( WEAPON ), Group( GROUP_PROJECTILE )]
	public virtual PrefabFile ProjectilePrefab { get; set; }

	/// <summary>
	/// The distance to spawn the projectile from the aiming origin.
	/// </summary>
	[Property]
	[Title( "Initial Distance" )]
	[Feature( WEAPON ), Group( GROUP_PROJECTILE )]
	public float ProjectileStartDistance { get; set; } = 16f;

	/// <summary>
	/// Set the prefab's scale when it's spawned?
	/// </summary>
	[Header( "Overrides" )]
	[Property, Title( "Scaling" )]
	[Feature( WEAPON ), Group( GROUP_PROJECTILE )]
	public virtual bool OverrideScale { get; set; }

	/// <summary>
	/// What to override the spawned prefab's scale with.
	/// </summary>
	[Property]
	[ShowIf( nameof( OverrideScale ), true )]
	[Feature( WEAPON ), Group( GROUP_PROJECTILE )]
	public virtual Vector3 Scale { get; set; } = Vector3.One;

	public override void OnPrimary()
	{
		var proj = SpawnProjectile();

		if ( proj.IsValid() && proj.Components.TryGet<Rigidbody>( out var rb ) )
			rb.Velocity = Owner.EyeForward * Speed;
	}

	public virtual GameObject SpawnProjectile( Vector3? pos = null, Rotation? r = null, Vector3? scale = null )
	{
		pos ??= AimPosition + (AimDirection * ProjectileStartDistance);

		if ( !ProjectilePrefab.TrySpawn( pos.Value, out var go ) )
			return null;

		r ??= Rotation.LookAt( AimDirection );

		if ( !scale.HasValue && OverrideScale )
			scale = Scale;

		go.WorldRotation = r.Value;

		if ( scale.HasValue )
			go.WorldScale = scale.Value;

		return go;
	}
}
