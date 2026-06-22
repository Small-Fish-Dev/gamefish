using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Runs logic that can output the input provided by activation.
/// You can think of this sort of like defining a function in the C# language.
/// <code> logic_relay </code>
/// </summary>
[Icon( "satellite_alt" )]
[EditorHandle( Icon = "📡" )]
partial class LogicRelayEntity : LogicEntity, IToggle, IActivate
{
	protected const int LOGIC_TIMING_ORDER = LOGIC_ORDER + 10;

	/// <summary>
	/// Execute this logic after being activated.
	/// <br /> <br />
	/// <b> LOGIC: </b> Activations output the value this was input with.
	/// </summary>
	[Property]
	[JsonIgnore]
	[Title( "Is On" )]
	[Order( LOGIC_DEBUG_ORDER )]
	[Feature( LOGIC ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected bool InspectorIsOn
	{
		get => IsOn;
		set => IsOn = value;
	}

	/// <summary>
	/// If enabled: print relay info to console.
	/// </summary>
	[Property]
	[Title( "Logging (relay)" )]
	[Order( LOGIC_DEBUG_ORDER )]
	[Feature( LOGIC ), Group( DEBUG )]
	protected bool DebugRelayLog { get; set; } = false;

	/// <summary>
	/// If enabled: you can't use this unless you manually toggle it on.
	/// </summary>
	[Property]
	[Feature( LOGIC )]
	[Order( LOGIC_ORDER )]
	[Title( "Start Disabled" )]
	protected virtual bool StartDisabled { get; set; } = false;

	/// <summary>
	/// If enabled: this fires when it first becomes active.
	/// <br /> <br />
	/// <b> LOGIC: </b> Does not output a value by default since nothing passed one in.
	/// </summary>
	[Property]
	[Title( "On Start" )]
	[Order( LOGIC_TIMING_ORDER )]
	[Feature( LOGIC ), Group( TIMING )]
	protected virtual bool RunOnStartEnabled { get; set; } = false;

	/// <summary>
	/// The time that activations are prevented since last being triggered.
	/// </summary>
	[Property]
	[Title( "Cooldown" )]
	[Order( LOGIC_TIMING_ORDER )]
	[Range( 0f, 2f, clamped: false )]
	[Feature( LOGIC ), Group( TIMING )]
	protected virtual float Cooldown { get; set; } = 0.1f;

	/// <summary>
	/// Execute this logic after being activated.
	/// <br /> <br />
	/// <b> LOGIC: </b> Activations output the value this was input with.
	/// </summary>
	[Property]
	[Title( "On Activated" )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	protected virtual List<LogicAction> OnActivatedLogic { get; set; } = [];

	/// <summary>
	/// Execute this logic whenever the cooldown is set.
	/// <br /> <br />
	/// <b> LOGIC: </b> Activations output the cooldown.
	/// <br /> <br />
	/// <b> EXAMPLE: </b> Activating a counter to set its value.
	/// </summary>
	[Property]
	[Title( "On Cooldown" )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	protected virtual List<LogicAction> OnCooldownLogic { get; set; } = [];

	public TimeSince? LastActivation { get; protected set; }

	/// <summary>
	/// If enabled: the relay is allowed to execute logic in response to activation.
	/// </summary>
	[Sync]
	public bool IsOn
	{
		get => _isOn ?? false;

		protected set
		{
			if ( _isOn is bool b )
				if ( b == value )
					return;

			_isOn = value;
			OnSetIsOn( in value );
		}
	}

	protected bool? _isOn = null;

	protected virtual void OnSetIsOn( in bool b )
	{
		if ( DebugRelayLog )
			this.Log( $"IsOn: {b}" );
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( RunOnStartEnabled )
			Activate();

		Toggle( !StartDisabled );
	}

	public virtual bool IsOnCooldown()
	{
		if ( LastActivation?.Relative is not float cd )
			return false;

		return cd < Cooldown;
	}

	public bool CanToggle( in bool isOn )
		=> !GameObject.IsDestroyed();

	public void Toggle( in bool isOn )
		=> IsOn = isOn;

	public virtual bool CanActivate( object source )
	{
		if ( GameObject.IsDestroyed() )
			return false;

		// Allow toggling functions off like a switch.
		if ( !IsOn )
			return false;

		// Relays would be a quick way to cause infinite loops.
		if ( source == this )
			return false;

		if ( IsOnCooldown() )
			return false;

		return true;
	}

	/// <summary>
	/// Directly forces running logic with an optional value.
	/// </summary>
	/// <returns> If activation did something. </returns>
	protected bool Activate( object value = null )
	{
		if ( !LogicAction.TryExecute( OnActivatedLogic, source: this, value: value ) )
			return false;

		// Only start cooldown if it fired successfully.
		LastActivation = 0f;

		return true;
	}

	public virtual bool TryActivate( object source = null, object value = null )
	{
		// Allow the relay to prevent spam and limit speeds.
		if ( IsOnCooldown() )
		{
			var cd = LastActivation?.Relative;
			return LogicAction.TryExecute( OnCooldownLogic, source: this, value: cd );
		}

		if ( !CanActivate( source ) )
			return false;

		return Activate( value );
	}
}
