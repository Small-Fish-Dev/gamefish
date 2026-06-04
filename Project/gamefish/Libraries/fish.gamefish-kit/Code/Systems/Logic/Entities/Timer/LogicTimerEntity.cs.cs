using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Runs logic in fixed/random intervals.
/// <br /> <br />
/// <b> LOGIC: </b> Activating this will enable and start the timer if it's not active yet.
/// <code> logic_timer </code>
/// </summary>
[Icon( "timer" )]
[EditorHandle( Icon = "⏱" )]
partial class LogicTimerEntity : LogicEntity, IToggle, IActivate
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
	/// Execute this logic after being activated.
	/// <br /> <br />
	/// <b> LOGIC: </b> Activations output the value this was input with.
	/// </summary>
	[Property]
	[Title( "Debug (timer)" )]
	[Order( LOGIC_DEBUG_ORDER )]
	[Feature( LOGIC ), Group( DEBUG )]
	protected bool DebugTimerLog { get; set; } = false;

	/// <summary>
	/// Execute this logic after being activated.
	/// <br /> <br />
	/// <b> LOGIC: </b> Activations output the value this was input with.
	/// </summary>
	[Property]
	[JsonIgnore]
	[Title( "Cooldown" )]
	[Order( LOGIC_DEBUG_ORDER )]
	[Feature( LOGIC ), Group( DEBUG )]
	[ShowIf( nameof( InGame ), true )]
	protected float Cooldown => NextActivation.Relative.Positive();

	/// <summary>
	/// If disabled: does nothing unless you activate or toggle it on.
	/// </summary>
	[Property]
	[Title( "Auto-Start" )]
	[Order( LOGIC_TIMING_ORDER )]
	[Feature( LOGIC ), Group( TIMING )]
	protected virtual bool AutoStartEnabled { get; set; } = true;

	/// <summary>
	/// If enabled: timer will fire itself upon end automatically after a fixed/random delay.
	/// </summary>
	[Property]
	[Title( "Repeat" )]
	[Order( LOGIC_TIMING_ORDER )]
	[Feature( LOGIC ), Group( TIMING )]
	protected virtual bool Repeat { get; set; } = false;

	/// <summary>
	/// The fixed/random delay when first starting up.
	/// </summary>
	[Property]
	[Title( "Delay" )]
	[Order( LOGIC_TIMING_ORDER )]
	[Feature( LOGIC ), Group( TIMING )]
	protected virtual RangedFloat InitialDelay { get; set; } = 1f;

	/// <summary>
	/// The fixed/random delay between running logic.
	/// </summary>
	[Property]
	[Title( "Interval" )]
	[Order( LOGIC_TIMING_ORDER )]
	[ShowIf( nameof( Repeat ), true )]
	[Feature( LOGIC ), Group( TIMING )]
	protected virtual RangedFloat Interval { get; set; } = new( 1f, 2f );

	/// <summary>
	/// Execute this logic when the clock runs out of time.
	/// </summary>
	[Property]
	[Title( "On Expire" )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	protected virtual List<LogicAction> OnTimerEndLogic { get; set; } = [];

	/// <summary>
	/// Execute this logic when the timer is first starting up.
	/// </summary>
	[Property]
	[Title( "On Start" )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	[InlineEditor, WideMode( HasLabel = true )]
	protected virtual List<LogicAction> OnTimerStartLogic { get; set; } = [];

	public TimeUntil NextActivation { get; protected set; }

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
		if ( DebugTimerLog )
			this.Log( $"IsOn: {b}" );
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( AutoStartEnabled )
			Toggle( isOn: true );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		OnUpdateTimer( Time.Delta );
	}

	protected virtual void OnUpdateTimer( in float deltaTime )
	{
		if ( !InGame )
			return;

		if ( !IsOn )
			return;

		if ( IsProxy )
			return;

		if ( !NextActivation )
			return;

		OnTimerEnd();
	}

	protected virtual void OnTimerEnd()
		=> TryTrigger();

	public virtual bool TryTrigger()
	{
		if ( GameObject.IsDestroyed() )
			return false;

		// TODO: Bool to pass in our delay as the value.
		float? delay = null;

		if ( Repeat )
		{
			IsOn = true;

			delay = Interval.GetValue();

			if ( delay is float t )
			{
				NextActivation = t;

				if ( DebugTimerLog )
					this.Log( $"Firing on repeat. New delay: {delay}" );
			}
		}
		else
		{
			// Turn it off after it's been used.
			IsOn = false;

			if ( DebugTimerLog )
				this.Log( $"Firing once and disabling self." );
		}

		return LogicAction.TryExecute( OnTimerEndLogic, source: this, value: null );
	}

	public bool CanToggle( in bool isOn )
	{
		if ( GameObject.IsDestroyed() )
			return false;

		return !IsProxy;
	}

	public void Toggle( in bool isOn )
	{
		IsOn = isOn;

		if ( isOn )
		{
			var delay = InitialDelay.GetValue();

			NextActivation = delay;

			LogicAction.TryExecute( OnTimerStartLogic, source: this, value: null );
		}
	}

	public virtual bool CanActivate( object source )
	{
		if ( GameObject.IsDestroyed() )
			return false;

		// Allow toggling functions off like a switch.
		if ( !IsOn )
			return false;

		return true;
	}

	public virtual bool TryActivate( object source = null, object value = null )
	{
		IsOn = true;

		if ( !NextActivation )
			return false;

		if ( !CanActivate( source ) )
			return false;

		return TryTrigger();
	}
}
