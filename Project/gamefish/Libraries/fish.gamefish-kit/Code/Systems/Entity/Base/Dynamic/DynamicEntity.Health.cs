namespace GameFish;

partial class DynamicEntity : IHealth
{
	protected const int HEALTH_ORDER = ENTITY_ORDER - 500;

	[Sync]
	[Property]
	[Feature( HEALTH )]
	[Order( HEALTH_ORDER )]
	public bool IsAlive
	{
		get => _isAlive;
		protected set
		{
			Tags?.Set( TAG_DEAD, state: !value );

			if ( _isAlive == value )
				return;

			var prev = _isAlive;
			_isAlive = value;

			if ( InGame )
			{
				if ( _isAlive )
					OnAlive();
				else
					OnDeath();
			}

			OnSetIsAlive( _isAlive, prev );
		}
	}

	protected bool _isAlive = true;

	protected virtual void OnSetIsAlive( in bool isAlive, in bool wasAlive )
	{
	}

	/// <summary> Is this capable of ever taking damage? </summary>
	[Property]
	[Feature( HEALTH )]
	[Order( HEALTH_ORDER )]
	public virtual bool IsDestructible { get; set; } = true;

	[Sync]
	[Feature( HEALTH )]
	[Order( HEALTH_ORDER )]
	[Property, Title( "Health" )]
	[ShowIf( nameof( IsDestructible ), true )]
	public float Health { get; protected set; } = 100f;

	[Sync]
	[Feature( HEALTH )]
	[Order( HEALTH_ORDER )]
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

	/// <summary>
	/// If true: destroy the parent's object upon death.
	/// </summary>
	[Property]
	[Order( HEALTH_ORDER )]
	[Title( "Self-Destruct" )]
	[Feature( HEALTH ), Group( DEATH )]
	public virtual bool DestroyUponDeath { get; set; } = false;

	public IEnumerable<DamageModule> DamageModules
		=> GetModules<DamageModule>().Where( m => m.IsValid() && m.Active );

	public virtual bool TrySendDamage( in DamageData data )
	{
		if ( !CanDamage( in data ) )
			return false;

		SendDamage( data );
		return true;
	}

	[Rpc.Owner( NetFlags.Reliable | NetFlags.SendImmediate )]
	protected void SendDamage( DamageData data )
		=> TryReceiveDamage( data );

	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostSetHealth( float hp )
		=> SetHealth( in hp );

	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostModifyHealth( float hp )
		=> ModifyHealth( in hp );

	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostTryKill()
		=> TryKill();

	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostTryRevive( bool restoreHealth = false )
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

		return true;
	}

	public virtual bool TryRevive( bool restoreHealth = false )
	{
		if ( IsProxy || IsAlive )
			return false;

		Health = Health.Max( restoreHealth ? MaxHealth : Health.Max( 1 ) );

		IsAlive = true;

		return true;
	}

	/// <summary>
	/// The entity has been killed/broken.
	/// <br /> <br />
	/// <b> WARNING: </b> The object may be destroyed from this!
	/// Make sure to check if the game object is valid afterwards.
	/// </summary>
	protected virtual void OnDeath()
	{
		foreach ( var m in DamageModules )
			m.OnDeath();

		if ( DestroyUponDeath )
			SelfDestruct();
	}

	protected virtual void OnAlive()
	{
		foreach ( var m in DamageModules )
			m.OnAlive();
	}

	public virtual bool CanDamage( in DamageData data )
		=> IsDestructible && IsAlive;

	/// <summary>
	/// Called by the owner to attempt inflicting the damage.
	/// </summary>
	/// <returns> If this damage should be inflicted or not. </returns>
	protected bool TryReceiveDamage( DamageData data )
	{
		// Pre-damage module modification.
		foreach ( var m in DamageModules )
			m.ModifyDamage( ref data );

		// Check if the damage is valid after modification.
		if ( !CanDamage( in data ) )
			return false;

		ApplyDamage( in data );
		return true;
	}

	/// <summary>
	/// Actually performs the damage meant to be dealt.
	/// </summary>
	protected virtual void ApplyDamage( in DamageData data )
	{
		ModifyHealth( -data.Damage );

		// Post-damage entity response.
		OnDamaged( in data );
	}

	/// <summary>
	/// Called after the damage has been successfully applied.
	/// </summary>
	protected virtual void OnDamaged( in DamageData data )
	{
		// Post-damage module response.
		foreach ( var m in DamageModules )
			m.OnDamaged( in data );

		// Post-damage impulse.
		if ( data.Impulse is Vector3 impulse )
			ApplyDamageImpulse( impulse );

		// Damage particles, sound etc.
		OnDamagedEffect( in data );
	}

	/// <summary>
	/// Called when taking damage to allow altering the force applied.
	/// </summary>
	protected virtual void ApplyDamageImpulse( Vector3 impulse )
		=> ApplyImpulse( impulse );

	/// <summary>
	/// A good place to play particles, sounds etc.
	/// </summary>
	public virtual void OnDamagedEffect( in DamageData data )
	{
	}

	protected virtual void SelfDestruct()
	{
		if ( IsProxy )
			return;

		if ( GameObject.IsValid() )
			GameObject.Destroy();
	}
}
