using System;

namespace GameFish;

public interface IHealth
{
	public const string FEATURE = "💖 Health";
	public const string GROUP_VALUES = "Values";

	abstract bool IsAlive { get; set; }

	/// <summary> Is this capable of ever taking damage? </summary>
	abstract bool IsDestructible { get; set; }

	abstract float Health { get; set; }
	abstract float MaxHealth { get; set; }

	/// <summary>
	/// The collection of <see cref="IHealthEvent"/>s relevant to this object. <br />
	/// Example: retrieved from a <see cref="ComponentList"/>.
	/// </summary>
	public IEnumerable<IHealthEvent> HealthEvents { get; }

	public virtual void SetHealth( in float hp )
	{
		Health = hp.Clamp( 0f, MaxHealth );

		foreach ( var e in HealthEvents )
			e.OnSetHealth( hp );

		if ( Health > 0 )
			Revive();
		else if ( Health <= 0 )
			Die();
	}

	public virtual void ModifyHealth( in float hp )
		=> SetHealth( Health + hp );

	public virtual void Die()
	{
		if ( !IsAlive )
			return;

		if ( Health > 0f )
			Health = MathF.Min( 0f, Health );

		IsAlive = false;
		OnDeath();
	}

	public virtual void Revive( bool restoreHealth = false )
	{
		if ( IsAlive )
			return;

		IsAlive = true;

		if ( restoreHealth )
			Health = MathF.Max( Health, MaxHealth );

		OnRevival();
	}

	public virtual void OnDeath()
	{
		foreach ( var e in HealthEvents )
			e.OnDeath();
	}

	public virtual void OnRevival()
	{
		foreach ( var e in HealthEvents )
			e.OnRevival();
	}

	public virtual bool CanDamage( in DamageInfo dmgInfo )
	{
		foreach ( var e in HealthEvents )
			if ( !e.CanDamage( in dmgInfo ) )
				return false;

		return IsDestructible && dmgInfo.Damage > 0;
	}

	public virtual bool TryDamage( DamageInfo dmgInfo )
	{
		if ( !CanDamage( in dmgInfo ) )
			return false;

		foreach ( var e in HealthEvents )
			if ( !e.TryDamage( ref dmgInfo ) )
				return false;

		ApplyDamage( dmgInfo );
		return true;
	}

	public virtual void ApplyDamage( DamageInfo dmgInfo )
	{
		foreach ( var e in HealthEvents )
			e.OnApplyDamage( ref dmgInfo );

		ModifyHealth( -dmgInfo.Damage );
	}
}
