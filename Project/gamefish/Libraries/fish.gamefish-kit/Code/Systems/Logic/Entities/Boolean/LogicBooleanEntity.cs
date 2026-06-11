using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Has an on/off state that you can toggle. Runs logic for both states.
/// <br /> <br />
/// <b> LOGIC: </b> Compatible with the "<b>Toggle</b>" command.
/// <br /> <br />
/// <b> LOGIC: </b> This accepts various boolean value types from the "<b>Activate</b>" command.
/// If no supported input is specified it will be toggled as configured.
/// </summary>
[Icon( "toggle_on" )]
[EditorHandle( Icon = "toggle_on" )]
public class LogicBooleanEntity : LogicEntity, IToggle, IActivate
{
	protected const int LOGIC_DEFAULTS_ORDER = LOGIC_ORDER + 10;

	/// <summary>
	/// The on/off state it starts with.
	/// </summary>
	[Property]
	[EnumButtonGroup]
	[Title( "State" )]
	[Order( LOGIC_DEFAULTS_ORDER )]
	[ShowIf( nameof( InEditor ), true )]
	[Feature( LOGIC ), Group( DEFAULTS )]
	protected virtual ToggleState State
	{
		get => IsOn.ToState();
		set => IsOn = value.ToBool();
	}

	/// <summary>
	/// How this should respond to "<b>Activate</b>" commands that haven't specified a supported value.
	/// </summary>
	[Property]
	[EnumButtonGroup]
	[Title( "Activation" )]
	[Order( LOGIC_DEFAULTS_ORDER )]
	[Feature( LOGIC ), Group( DEFAULTS )]
	protected virtual ToggleCommand ActivationToggle { get; set; } = ToggleCommand.Toggle;

	/// <summary>
	/// If enabled: print boolean-related happenings to the console.
	/// </summary>
	[Property]
	[Title( "Logging (state)" )]
	[Order( LOGIC_DEBUG_ORDER )]
	[Feature( LOGIC ), Group( DEBUG )]
	public bool DebugLogBoolean { get; set; } = false;

	/// <summary>
	/// The on/off state it starts with.
	/// </summary>
	[Property]
	[JsonIgnore]
	[EnumButtonGroup]
	[Title( "State" )]
	[Order( LOGIC_DEBUG_ORDER )]
	[ShowIf( nameof( InGame ), true )]
	[Feature( LOGIC ), Group( DEBUG )]
	protected ToggleState InspectorState
	{
		get => State;
		set => Toggle( value.ToBool() );
	}

	/// <summary>
	/// Logic to run any time this is successfully toggled.
	/// <br /> <br />
	/// <b> LOGIC: </b> Outputs its on/off state as the activation value.
	/// </summary>
	[Property]
	[Title( "On Toggle" )]
	[WideMode( HasLabel = true )]
	[InlineEditor( Label = true )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	protected virtual List<LogicAction> OnToggleLogic { get; set; }

	/// <summary>
	/// Logic to run any time this is switched on.
	/// <br /> <br />
	/// <b> LOGIC: </b> Outputs its on/off state as the activation value.
	/// </summary>
	[Property]
	[Title( "On Enabled" )]
	[WideMode( HasLabel = true )]
	[InlineEditor( Label = true )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	protected virtual List<LogicAction> OnEnabledLogic { get; set; }

	/// <summary>
	/// Logic to run any time this is switched off.
	/// <br /> <br />
	/// <b> LOGIC: </b> Outputs its on/off state as the activation value.
	/// </summary>
	[Property]
	[Title( "On Disabled" )]
	[WideMode( HasLabel = true )]
	[InlineEditor( Label = true )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	protected virtual List<LogicAction> OnDisabledLogic { get; set; }

	/// <summary>
	/// Logic to run when the entity first starts.
	/// <br /> <br />
	/// <b> LOGIC: </b> Outputs its on/off state as the activation value.
	/// </summary>
	[Property]
	[Title( "On Start" )]
	[WideMode( HasLabel = true )]
	[InlineEditor( Label = true )]
	[Order( LOGIC_FUNCTIONS_ORDER )]
	[Feature( LOGIC ), Group( FUNCTIONS )]
	protected virtual List<LogicAction> OnStartLogic { get; set; }

	[Sync]
	public bool IsOn { get; protected set; }

	protected override void OnLogicStart()
	{
		if ( IsProxy )
			return;

		RpcBroadcastOnLogicStart( IsOn );
	}

	/// <summary>
	/// Tells other connections that this first started just now.
	/// </summary>
	/// <remarks>
	/// This is an RPC instead of a Sync callback so that
	/// clients don't execute related logic upon connecting.
	/// </remarks>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	protected virtual void RpcBroadcastOnLogicStart( bool isOn )
	{
		if ( DebugLogBoolean )
			this.Log( $"Started. State: {isOn}" );

		LogicAction.TryExecute( OnStartLogic, source: this, value: isOn );
	}

	/// <summary>
	/// Tells other connections that this was toggled just now.
	/// </summary>
	/// <remarks>
	/// This is an RPC instead of a Sync callback so that
	/// clients don't execute related logic upon connecting.
	/// </remarks>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	protected virtual void RpcBroadcastOnToggle( bool isOn )
	{
		if ( DebugLogBoolean )
			this.Log( $"Toggled. State: {isOn}" );

		LogicAction.TryExecute( OnToggleLogic, source: this, value: isOn );

		if ( isOn )
			LogicAction.TryExecute( OnEnabledLogic, source: this, value: true );
		else
			LogicAction.TryExecute( OnDisabledLogic, source: this, value: false );
	}

	public virtual bool CanToggle( in bool isOn )
		=> IsOn != isOn;

	public virtual void Toggle( in bool isOn )
	{
		if ( !CanToggle( isOn ) )
			return;

		IsOn = isOn;

		RpcBroadcastOnToggle( isOn );
	}

	public virtual bool CanActivate( object source )
	{
		if ( GameObject.IsDestroyed() )
			return false;

		if ( source == this )
			return false;

		return !IsProxy;
	}

	public virtual bool TryActivate( object source = null, object value = null )
	{
		if ( !CanActivate( source ) )
			return false;

		// Might be specifying a boolean.
		if ( value is bool b )
			return this.TryToggle( bState: b );
		else if ( value is ToggleState state )
			return this.TryToggle( bState: state.ToBool() );
		else if ( value is ToggleCommand cmd )
			return this.TryToggle( bState: cmd.Apply( IsOn ) );

		// Allow the mapper to decide the default toggle behavior.
		return this.TryToggle( cmd: ActivationToggle );
	}
}
