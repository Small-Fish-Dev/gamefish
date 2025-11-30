namespace GameFish;

/// <summary>
/// Provides a function to an equipment.
/// Looks for input in a configurable way to do so.
/// </summary>
[Icon( "electric_bolt" )]
public abstract partial class EquipFunction : EquipModule
{
	protected const int INPUT_ORDER = EQUIP_ORDER - 20;
	protected const int DISPLAY_ORDER = EQUIP_ORDER - 10;

	/// <summary>
	/// Should this input be usable, displayable?
	/// </summary>
	[DefaultValue( true )]
	[Title( "Is Enabled" )]
	[Property( Name = "Input" )]
	[Feature( MODULE ), Group( INPUT ), Order( INPUT_ORDER )]
	protected bool InspectorIsInputEnabled
	{
		get => IsInputEnabled;
		set => IsInputEnabled = value;
	}

	public virtual bool IsInputEnabled { get; set; } = true;

	/// <summary>
	/// What button should be used? Is it on press/hold/release?
	/// </summary>
	[Title( "Input" )]
	[Property, InlineEditor( Label = false )]
	[Feature( MODULE ), Group( INPUT ), Order( INPUT_ORDER )]
	protected FunctionInput InspectorInput
	{
		get => Input;
		set => Input = value;
	}

	public virtual FunctionInput Input { get; set; } = new( "Attack1", InputMode.Held, 0.15f );

	/// <summary>
	/// The name to display for this function(if any).
	/// </summary>
	[Property]
	[Title( "Name" )]
	[Feature( MODULE ), Group( DISPLAY ), Order( DISPLAY_ORDER )]
	public virtual string FunctionName { get; set; }

	/// <summary>
	/// The description to display for this function(if any).
	/// </summary>
	[Property]
	[Title( "Description" )]
	[Feature( MODULE ), Group( DISPLAY ), Order( DISPLAY_ORDER )]
	public virtual string FunctionDescription { get; set; }

	/// <summary>
	/// If defined: this function uses ammo.
	/// </summary>
	[Property]
	[Feature( MODULE ), Group( AMMO )]
	public virtual AmmoEquipModule Ammo { get; set; }

	/// <summary>
	/// How much ammo should this consume at a time?
	/// </summary>
	[Property]
	[Title( "Cost" )]
	[Feature( MODULE ), Group( AMMO )]
	[Range( 0.1f, 100f, clamped: false ), Step( 1f )]
	public virtual float AmmoCost { get; set; } = 1f;

	/// <summary>
	/// Is the input state requirement being met? <br />
	/// In other words: are they pressing/holding/releasing the button?
	/// </summary>
	public virtual bool IsInputting => IsInputEnabled && Input?.IsInputting() is true;

	/// <summary>
	/// The point in time that this was last used.
	/// </summary>
	[Sync] public TimeSince? SinceUsed { get; set; }

	public override void Simulate( in float deltaTime )
	{
		base.Simulate( deltaTime );

		if ( IsInputting )
			TryActivate();
	}

	/// <returns> The remaining seconds before this can be used again. </returns>
	public virtual float GetCooldownRemaining()
	{
		if ( !SinceUsed.HasValue )
			return 0f;

		return (GetCooldownDuration() - SinceUsed.Value).Positive();
	}

	/// <returns> How long this function's cooldown should last. </returns>
	public virtual float GetCooldownDuration()
	{
		var cooldown = Input?.Cooldown ?? 1f;

		if ( Equip.IsValid() )
			return Equip.GetCooldownDuration( cooldown, this );

		return cooldown;
	}

	/// <summary>
	/// Allows implementing various activation requirements.
	/// Requirements should be here, not in <see cref="TryActivate"/>.
	/// </summary>
	/// <remarks> This may be called from UI/NPCs so keep it lite. </remarks>
	/// <returns> If activation would be successful. </returns>
	public virtual bool CanActivate()
	{
		if ( GetCooldownRemaining() > 0f )
			return false;

		if ( Ammo.IsValid() && !Ammo.IsFunctionAllowed( this ) )
			return false;

		return true;
	}

	/// <summary>
	/// Attempts to activate this functionality with respect for cooldown etc.
	/// Requirements should be put in <see cref="CanActivate"/>, not here.
	/// </summary>
	/// <returns> If the activation was successful. </returns>
	public virtual bool TryActivate()
	{
		if ( !CanActivate() )
			return false;

		if ( Ammo.IsValid() && !TryTakeAmmo( AmmoCost, out _ ) )
		{
			ActivateEmpty();
			return false;
		}

		Activate();
		OnActivate();

		return true;
	}

	/// <summary>
	/// Broadcasts a sound at a position.
	/// </summary>
	protected virtual bool TryPlaySound( in SoundEvent snd, in SoundSettings settings )
	{
		if ( !snd.IsValid() )
			return false;

		BroadcastSound( snd, settings );
		return true;
	}

	/// <summary>
	/// Plays SFX/VFX of this function when it is activated.
	/// </summary>
	protected virtual void PlayActivationEffect( in Transform tOrigin ) { }

	/// <summary>
	/// Allows the function to decide if it wants to take this ammo and how much.
	/// </summary>
	/// <returns> If the ammo was taken. </returns>
	protected virtual bool TryTakeAmmo( in float cost, out float taken )
	{
		var toTake = cost.Positive();

		if ( !Ammo.IsValid() || !Ammo.TryTakeAmmo( toTake ) )
		{
			taken = 0f;
			return false;
		}

		taken = toTake;
		return true;
	}

	/// <summary>
	/// Called by <see cref="TryActivate"/> which respects conditions such as cooldowns.
	/// </summary>
	protected virtual void Activate()
	{
	}

	/// <summary>
	/// Called by <see cref="TryActivate"/> when we support ammo and are out of it.
	/// </summary>
	protected virtual void ActivateEmpty()
	{
		if ( Ammo.IsValid() )
			Ammo.TryStartReload();
	}

	/// <summary>
	/// Called after <see cref="Activate"/> to set the cooldown etc.
	/// </summary>
	protected virtual void OnActivate()
	{
		SinceUsed = 0;
	}

	/// <returns> A rotation representing a random direction and roll within a cone. </returns>
	public virtual Rotation GetSpreadConeRotation( float spread )
	{
		if ( spread == 0f )
			return Rotation.Identity;

		return Rotation.Identity
			.RotateAroundAxis( Vector3.Forward, Random.Float( 0f, 360f ) )
			.RotateAroundAxis( Vector3.Right, Random.Float( 0f, spread ) );
	}
}
