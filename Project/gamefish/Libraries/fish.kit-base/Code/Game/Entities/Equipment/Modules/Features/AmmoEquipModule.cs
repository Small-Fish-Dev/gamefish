using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// An equipment function module providing ammo and reloading features.
/// </summary>
[Title( "Equipment Ammo" )]
public partial class AmmoEquipModule : EquipFunction
{
	protected const int AMMO_ORDER = EQUIP_ORDER - 500;

	protected new const int INPUT_ORDER = AMMO_ORDER - 10;

	protected const int MAGAZINE_ORDER = AMMO_ORDER + 10;
	protected const int RESERVE_ORDER = AMMO_ORDER + 20;
	protected const int RELOADING_ORDER = AMMO_ORDER + 30;

	public override int ExecutionOrder => EXECUTION_ORDER_DEFAULT - 5;

	public override bool IsInputEnabled
	{
		get => IsReloadingEnabled;
		set => IsReloadingEnabled = value;
	}

	public override FunctionInput Input { get; set; } = new( "Reload", InputMode.Held, 0.1f );


	/// <summary>
	/// The magazine's current ammo count.
	/// </summary>
	[Title( "Ammo" )]
	[Property, ReadOnly, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( AMMO ), Group( MAGAZINE ), Order( MAGAZINE_ORDER - 1 )]
	protected virtual float InspectorAmmo
	{
		get => AmmoCount;
		set => AmmoCount = value;
	}

	/// <summary>
	/// The ammo the magazine starts with.
	/// </summary>
	[Property]
	[Title( "Starting" )]
	[Feature( AMMO ), Group( MAGAZINE ), Order( MAGAZINE_ORDER )]
	public virtual float StartingAmmo { get; set; } = 20;

	/// <summary>
	/// The magazine's default size.
	/// </summary>
	[Property]
	[Title( "Size" )]
	[Feature( AMMO ), Group( MAGAZINE ), Order( MAGAZINE_ORDER )]
	public virtual float DefaultMagazineSize { get; set; } = 20;


	/// <summary>
	/// Can ammo be reloaded?
	/// </summary>
	[Property]
	[Feature( AMMO ), Order( RELOADING_ORDER )]
	[ToggleGroup( nameof( IsReloadingEnabled ), Label = RELOADING )]
	public virtual bool IsReloadingEnabled { get; set; } = true;

	/// <summary>
	/// Are they currently reloading?
	/// </summary>
	[Title( "Is Reloading" )]
	[Property, ReadOnly, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( AMMO ), Order( RELOADING_ORDER - 1 )]
	[ToggleGroup( nameof( IsReloadingEnabled ) )]
	protected virtual bool InspectorIsReloading => IsReloading;

	/// <summary>
	/// How long is left on the reload?
	/// </summary>
	[Title( "Reload Time" )]
	[Property, ReadOnly, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( AMMO ), Order( RELOADING_ORDER - 1 )]
	[ToggleGroup( nameof( IsReloadingEnabled ) )]
	protected virtual float InspectorReloadTime => ReloadTime;

	[Property]
	[Title( "Duration" )]
	[Feature( AMMO ), Order( RELOADING_ORDER )]
	[ToggleGroup( nameof( IsReloadingEnabled ) )]
	protected virtual float ReloadDuration { get; set; } = 1f;

	/// <summary>
	/// Prevent ammo-dependent functions until this delay after finishing a reload.
	/// </summary>
	[Property]
	[Title( "Cooldown" )]
	[Feature( AMMO ), Order( RELOADING_ORDER )]
	[ToggleGroup( nameof( IsReloadingEnabled ) )]
	public virtual float ReloadCooldown { get; set; } = 0.2f;

	/// <summary>
	/// Sound to play when you begin a reload.
	/// </summary>
	[Property]
	[Title( "Start Sound" )]
	[Feature( AMMO ), Order( RELOADING_ORDER )]
	[ToggleGroup( nameof( IsReloadingEnabled ) )]
	public virtual SoundEvent ReloadStartSound { get; set; }

	/// <summary>
	/// Sound to play when you successfully finish a reload.
	/// </summary>
	[Property]
	[Title( "Finish Sound" )]
	[Feature( AMMO ), Order( RELOADING_ORDER )]
	[ToggleGroup( nameof( IsReloadingEnabled ) )]
	public virtual SoundEvent ReloadFinishSound { get; set; }


	/// <summary>
	/// Is ammo reloaded from a depleting pool?
	/// </summary>
	[Property]
	[Feature( AMMO ), Order( RESERVE_ORDER )]
	[ToggleGroup( nameof( IsReserveEnabled ), Label = RESERVE )]
	public virtual bool IsReserveEnabled { get; set; } = false;

	/// <summary>
	/// The actively remaining ammo we can refill our magazine from.
	/// </summary>
	[Title( "Reserve" )]
	[Property, ReadOnly, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[ToggleGroup( nameof( IsReserveEnabled ) )]
	[Feature( AMMO ), Order( RESERVE_ORDER - 1 )]
	protected virtual float InspectorReserve => ReserveCount;

	/// <summary>
	/// The ammo to start with that we can reload from.
	/// </summary>
	[Property]
	[Title( "Starting" )]
	[Feature( AMMO ), Order( RESERVE_ORDER )]
	[ToggleGroup( nameof( IsReserveEnabled ) )]
	public virtual float StartingReserve { get; set; } = 0;

	/// <summary>
	/// The default limit of how much reserve ammo we can have.
	/// </summary>
	[Property]
	[Title( "Limit" )]
	[Feature( AMMO ), Order( RESERVE_ORDER )]
	[ToggleGroup( nameof( IsReserveEnabled ) )]
	protected virtual float DefaultReserveLimit { get; set; } = 100;


	/// <summary> Ammo can't have ammo. That don't make no sense, man. </summary>
	public override AmmoEquipModule Ammo => null;

	/// <summary> Ammo can't have ammo. That don't make no sense, man. </summary>
	public override float AmmoCost => 0;


	/// <summary>
	/// The magazine's current ammo count.
	/// </summary>
	[Sync]
	public float AmmoCount
	{
		get => _ammoCount;
		set
		{
			_ammoCount = value;
			OnSetAmmoCount( in value );
		}
	}

	protected float _ammoCount;


	/// <summary>
	/// The reserve ammo this particular module has remaining.
	/// </summary>
	[Sync]
	public float ReserveCount
	{
		get => _reserveCount;
		set
		{
			_ammoCount = value;
			OnSetReserveCount( in value );
		}
	}

	protected float _reserveCount;


	/// <summary>
	/// Has reloading been initiated?
	/// </summary>
	[Sync]
	public bool IsReloading
	{
		get => _isReloading;
		set
		{
			_isReloading = value;
			OnSetIsReloading( value );
		}
	}

	protected bool _isReloading = false;

	/// <summary> How many seconds are left of the reload? </summary>
	[Sync]
	public float ReloadTime
	{
		get => _reloadTime;
		set => _reloadTime = value.Positive();
	}

	protected float _reloadTime = 0f;

	/// <summary> Has reloading finished, and if so since when? </summary>
	[Sync] public TimeSince? SinceReload { get; set; }


	/// <summary>
	/// Called directly after <see cref="AmmoCount"/> has been set.
	/// </summary>
	protected virtual void OnSetAmmoCount( in float ammoCount )
	{
	}

	/// <summary>
	/// Called directly after <see cref="ReserveCount"/> has been set.
	/// </summary>
	protected virtual void OnSetReserveCount( in float reserveCount )
	{
	}

	/// <summary>
	/// Called directly after <see cref="IsReloading"/> has been set.
	/// </summary>
	protected virtual void OnSetIsReloading( in bool isReloading )
	{
	}


	protected override void OnStart()
	{
		base.OnStart();

		AmmoCount = StartingAmmo.Max( AmmoCount );

		if ( IsReserveEnabled )
			ReserveCount = StartingReserve.Max( ReserveCount );
	}

	public override void Simulate( in float deltaTime )
	{
		base.Simulate( in deltaTime );

		UpdateReloading( in deltaTime );
	}

	public override void OnHolster()
	{
		base.OnHolster();

		StopReloading();
	}

	protected virtual void UpdateReloading( in float deltaTime )
	{
		if ( !IsReloading )
			return;

		ReloadTime -= deltaTime;

		if ( ReloadTime <= 0f )
			TryFinishReload();
	}

	/// <summary>
	/// Allows you to block other functions(such as while reloading).
	/// </summary>
	/// <returns> If true: a function can do its thing. </returns>
	public virtual bool IsFunctionAllowed( EquipFunction func )
	{
		if ( IsReloading )
			return false;

		if ( SinceReload.HasValue && SinceReload < ReloadCooldown )
			return false;

		return true;
	}

	public override bool CanActivate()
		=> CanReload();

	protected override void Activate()
		=> TryStartReload();

	protected override void OnActivate() { }

	public virtual bool CanReload()
	{
		if ( !IsReloadingEnabled )
			return false;

		if ( IsReloading )
			return false;

		return AmmoCount < GetMagazineSize();
	}

	public virtual bool TryStartReload()
	{
		if ( !CanReload() )
			return false;

		SinceUsed = 0f;
		SinceReload = null;

		IsReloading = true;
		ReloadTime = GetReloadDuration();

		if ( ReloadStartSound.IsValid() )
			BroadcastSound( ReloadStartSound );

		return true;
	}

	public virtual bool TryFinishReload()
	{
		if ( !IsReloading )
			return false;

		IsReloading = false;
		ReloadTime = 0f;

		SinceReload = 0f;

		TryRefillMagazine();

		if ( ReloadFinishSound.IsValid() )
			BroadcastSound( ReloadFinishSound );

		return true;
	}

	public virtual bool TryInterruptReload()
	{
		// Nothing to interrupt?
		if ( !IsReloading )
			return true;

		StopReloading();

		return true;
	}

	/// <summary>
	/// Forces any current reload to halt immediately.
	/// </summary>
	protected virtual void StopReloading()
	{
		IsReloading = false;
		ReloadTime = 0f;
	}

	public virtual bool TryTakeAmmo( float count )
	{
		if ( count > AmmoCount )
			return false;

		AmmoCount -= count;
		return true;
	}

	/// <summary>
	/// Tries to restore ammo to its full capacity.
	/// </summary>
	/// <param name="fromReserve"> Take from our reserve(if enabled)? </param>
	/// <returns> If any ammo could be refilled. </returns>
	public virtual bool TryRefillMagazine( bool fromReserve = true )
	{
		var capacity = GetMagazineSize();

		if ( !fromReserve || !IsReserveEnabled )
		{
			if ( AmmoCount >= capacity )
				return false;

			AmmoCount = capacity;
			return true;
		}

		var needed = (capacity - AmmoCount).Positive();
		var reserve = ReserveCount.Positive();
		var take = needed.Min( reserve );

		if ( take <= 0 )
			return false;

		ReserveCount -= take;
		AmmoCount += take;

		return true;
	}

	public virtual float GetMagazineSize()
	{
		var capacity = DefaultMagazineSize;

		if ( Equip.IsValid() )
			return Equip.GetMagazineSize( capacity, this );

		return capacity;
	}

	public virtual float GetReserveLimit()
	{
		var capacity = DefaultReserveLimit;

		if ( Equip.IsValid() )
			return Equip.GetReserveLimit( capacity, this );

		return capacity;
	}

	public override float GetCooldownRemaining()
		=> IsReloading ? ReloadTime : 0f;

	public virtual float GetReloadDuration()
	{
		var duration = ReloadDuration;

		if ( Equip.IsValid() )
			return Equip.GetReloadDuration( duration, this );

		return duration;
	}
}
