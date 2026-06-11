namespace GameFish.Nodes;

partial class NodeEntity : IToggle, IActivate
{
	/// <summary>
	/// Runs when this node first starts(such as when it spawns).
	/// </summary>
	[Property]
	[Title( "On Start" )]
	[WideMode( HasLabel = true )]
	[InlineEditor( Label = true )]
	[Order( METHODS_LOGIC_ORDER + 1 )]
	[Feature( METHODS ), Group( LOGICAL )]
	protected virtual List<LogicAction> OnStartLogic { get; set; }

	/// <summary>
	/// Runs when this node is activated(enabled).
	/// </summary>
	[Property]
	[Title( "On Enabled" )]
	[WideMode( HasLabel = true )]
	[InlineEditor( Label = true )]
	[Order( METHODS_LOGIC_ORDER + 2 )]
	[Feature( METHODS ), Group( LOGICAL )]
	protected virtual List<LogicAction> OnEnabledLogic { get; set; }

	/// <summary>
	/// Runs when this node is deactivated(disabled).
	/// </summary>
	[Property]
	[Title( "On Disabled" )]
	[WideMode( HasLabel = true )]
	[InlineEditor( Label = true )]
	[Order( METHODS_LOGIC_ORDER + 3 )]
	[Feature( METHODS ), Group( LOGICAL )]
	protected virtual List<LogicAction> OnDisabledLogic { get; set; }

	/// <summary>
	/// Is this meant to be actively ticking?
	/// </summary>
	public virtual bool IsTicking => Active && IsOn && TickingEnabled;

	/// <summary>
	/// The moment that the last tick happened(if ever).
	/// </summary>
	[Sync]
	protected TimeSince? LastTick { get; set; }

	public bool IsOn
	{
		get => NodeEnabled.IsEnabled();
		protected set => NodeEnabled = value.ToState();
	}

	protected virtual void OnNodeStateSet( ToggleState oldValue, ToggleState newValue )
	{
		if ( GameObject.IsDestroyed() )
			return;

		if ( !InGame )
			return;

		if ( newValue.IsEnabled() )
			OnNodeEnabled();
		else
			OnNodeDisabled();

		UpdateLinkedStates();
	}

	protected virtual void OnNodeStart()
	{
		SetupDefaultLinks();

		LogicAction.TryExecute( OnStartLogic, source: this );
	}

	/// <summary>
	/// Respond to <see cref="NodeEnabled"/> being set to <see cref="ToggleState.Enabled"/>.
	/// </summary>
	protected virtual void OnNodeEnabled()
	{
		// Don't immediately tick on start.
		if ( TickingEnabled )
			LastTick = 0f;

		LogicAction.TryExecute( OnEnabledLogic, source: this );
	}

	/// <summary>
	/// Respond to <see cref="NodeEnabled"/> being set to <see cref="ToggleState.Disabled"/>.
	/// </summary>
	protected virtual void OnNodeDisabled()
	{
		LogicAction.TryExecute( OnDisabledLogic, source: this );
	}

	protected virtual void Think()
	{
		if ( !IsTicking )
			return;

		if ( LastTick is TimeSince t )
			if ( t < TickRate )
				return;

		Tick( TickRate );
	}

	public virtual void Tick( float deltaTime )
	{
		if ( deltaTime <= 0f )
			deltaTime = Time.Delta;

		LastTick = 0f;

		LogicAction.TryExecute( OnTickLogic, source: this, value: deltaTime );
	}

	public virtual bool CanToggle( in bool isOn )
		=> IsOn != isOn;

	public virtual void Toggle( in bool isOn )
	{
		if ( !CanToggle( isOn ) )
			return;

		IsOn = isOn;
	}

	public virtual bool CanActivate( object source )
	{
		if ( GameObject.IsDestroyed() )
			return false;

		return !IsProxy;
	}

	public virtual bool TryActivate( object source = null, object value = null )
	{
		if ( !CanActivate( source ) )
			return false;

		if ( value is StringMatch sm )
			return TryRunMethod( sm );

		if ( value is string funcName )
			return TryRunMethod( new( funcName ) );

		return false;
	}
}
