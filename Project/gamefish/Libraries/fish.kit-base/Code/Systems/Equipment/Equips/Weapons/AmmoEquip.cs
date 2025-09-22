using System;

namespace GameFish;

/// <summary>
/// Has ammo and can reload.
/// </summary>
public abstract partial class AmmoEquip : BaseEquip
{
	public const string GROUP_AMMO = "Ammo";
	public const string GROUP_BULLET = "Bullet";
	public const string GROUP_PROJECTILE = "Projectile";

	[Property]
	[Feature( WEAPON ), Group( "Ammo" )]
	public virtual bool HasAmmo { get; set; } = true;

	[ShowIf( nameof( HasAmmo ), true )]
	[Property, Feature( WEAPON ), Group( "Ammo" ), Title( "Max Ammo" )]
	public virtual int DefaultMaxAmmo { get; set; } = 20;

	[ShowIf( nameof( HasAmmo ), true )]
	[Property, Feature( WEAPON ), Group( "Ammo" )]
	public float ReloadDuration { get; set; } = 1f;

	public virtual int Ammo { get => _ammo; set => _ammo = Math.Max( 0, value ); }
	private int _ammo;

	public bool Reloading { get; set; }
	public TimeUntil ReloadEnds { get; set; }

	public virtual int GetMaxAmmo() => DefaultMaxAmmo;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !IsDeployed )
			return;

		if ( Reloading )
		{
			if ( ReloadEnds )
				FinishReload();

			return;
		}
	}

	public override void OnHolster( BasePawn owner, PawnEquipment inv )
	{
		base.OnHolster( owner, inv );

		Reloading = false;
	}

	public override bool CanPrimary()
		=> base.CanPrimary() && (!HasAmmo || !Reloading);

	public override bool CanSecondary()
		=> base.CanSecondary() && (!HasAmmo || !Reloading);

	public virtual void StartReloading()
	{
		if ( !HasAmmo || Reloading )
			return;

		ReloadEnds = ReloadDuration;
	}

	public virtual void FinishReload()
	{
		ReloadEnds = 0f;
		Ammo = GetMaxAmmo();
	}
}
