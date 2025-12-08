namespace GameFish;

partial class DynamicEntity : IHealth
{
	[Sync]
	[Property, Feature( HEALTH )]
	public bool IsAlive { get; protected set; } = true;

	/// <summary> Is this capable of ever taking damage? </summary>
	[Property, Feature( HEALTH )]
	public virtual bool IsDestructible { get; set; } = true;

	[Sync]
	[Feature( HEALTH )]
	[Property, Title( "Health" )]
	[ShowIf( nameof( IsDestructible ), true )]
	public float Health { get; protected set; } = 100f;

	[Sync]
	[Feature( HEALTH )]
	[Property, Title( "Max Health" )]
	[ShowIf( nameof( IsDestructible ), true )]
	public float MaxHealth { get; set; } = 100f;

	[Property]
	[Order( DEBUG_ORDER )]
	[Feature( HEALTH ), Group( DEBUG )]
	[ShowIf( nameof( IsDestructible ), true )]
	public float DebugDamage { get; set; } = 25f;

	[Button]
	[Order( DEBUG_ORDER )]
	[Title( "Take Damage" )]
	[Feature( HEALTH ), Group( DEBUG )]
	[ShowIf( nameof( IsDestructible ), true )]
	protected void DebugTakeDamage()
		=> TrySendDamage( new() { Damage = DebugDamage } );


	public virtual bool TrySendDamage( in DamageData data )
	{
		if ( !CanDamage( in data ) )
			return false;

		SendDamage( data );
		return true;
	}

	[Rpc.Owner( NetFlags.Reliable | NetFlags.SendImmediate )]
	protected void SendDamage( DamageData data )
		=> TryReceiveDamage( in data );


	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostSetHealth( float hp )
		=> SetHealth( in hp );

	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostModifyHealth( float hp )
		=> ModifyHealth( in hp );

	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostKill()
		=> TryKill();

	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostRevive( bool restoreHealth = false )
		=> TryRevive( restoreHealth );


	public virtual void SetHealth( in float hp )
	{
		if ( IsProxy )
			return;

		Health = hp.Clamp( 0f, MaxHealth );

		if ( !IsAlive && Health > 0 )
			TryRevive();
		else if ( IsAlive && Health <= 0 )
			TryKill();
	}

	public virtual void ModifyHealth( in float hp )
		=> SetHealth( Health + hp );

	public virtual bool TryKill()
	{
		if ( IsProxy || !IsAlive )
			return false;

		if ( Health > 0f )
			Health = 0f;

		IsAlive = false;
		OnDeath();

		return true;
	}

	public virtual bool TryRevive( bool restoreHealth = false )
	{
		if ( IsProxy || IsAlive )
			return false;

		IsAlive = true;
		Health = Health.Max( restoreHealth ? MaxHealth : Health.Max( 1 ) );

		OnRevival();

		return true;
	}

	public virtual void OnDeath()
	{
	}

	public virtual void OnRevival()
	{
	}


	public virtual bool CanDamage( in DamageData data )
		=> IsDestructible && data.Damage > 0;

	/// <summary>
	/// Called by the owner to attempt inflicting the damage.
	/// </summary>
	/// <returns> If this damage should be inflicted or not. </returns>
	protected virtual bool TryReceiveDamage( in DamageData data )
	{
		if ( !CanDamage( in data ) )
			return false;

		ApplyDamage( data );
		return true;
	}

	/// <summary>
	/// Actually performs the damage meant to be dealt.
	/// </summary>
	/// <param name="data"></param>
	protected virtual void ApplyDamage( DamageData data )
	{
		ModifyHealth( -data.Damage );

		OnDamaged( in data );
	}

	/// <summary>
	/// Called after the damage has been successfully applied.
	/// </summary>
	protected virtual void OnDamaged( in DamageData data )
	{
	}
}
