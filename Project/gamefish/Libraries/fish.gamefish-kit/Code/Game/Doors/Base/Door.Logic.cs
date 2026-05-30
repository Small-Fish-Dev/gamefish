namespace GameFish;

partial class Door : IToggle, IActivate
{
	/// <summary>
	/// Executed when the door is fully opened.
	/// </summary>
	[Property]
	[Title( "On Opened" )]
	[InlineEditor, WideMode]
	[Feature( LOGIC ), Order( LOGIC_ORDER )]
	protected List<LogicAction> OnOpenedLogic { get; set; }

	/// <summary>
	/// Executed when the door starts opening.
	/// </summary>
	[Property]
	[Title( "On Opening" )]
	[InlineEditor, WideMode]
	[Feature( LOGIC ), Order( LOGIC_ORDER )]
	protected List<LogicAction> OnOpeningLogic { get; set; }

	/// <summary>
	/// Executed when the door is fully closed.
	/// </summary>
	[Property]
	[Title( "On Closed" )]
	[InlineEditor, WideMode]
	[Feature( LOGIC ), Order( LOGIC_ORDER )]
	protected List<LogicAction> OnClosedLogic { get; set; }

	/// <summary>
	/// Executed when the door starts closing.
	/// </summary>
	[Property]
	[Title( "On Closing" )]
	[InlineEditor, WideMode]
	[Feature( LOGIC ), Order( LOGIC_ORDER )]
	protected List<LogicAction> OnClosingLogic { get; set; }

	protected virtual void OnLogicOpened() => LogicAction.TryExecute( OnOpenedLogic, source: this );
	protected virtual void OnLogicOpening() => LogicAction.TryExecute( OnOpeningLogic, source: this );

	protected virtual void OnLogicClosed() => LogicAction.TryExecute( OnClosedLogic, source: this );
	protected virtual void OnLogicClosing() => LogicAction.TryExecute( OnClosingLogic, source: this );

	/// <inheritdoc cref="IsOpened" />
	bool IToggle.IsOn => IsOpened;

	void IToggle.Toggle( in bool isOn ) => Toggle( isOn );
	bool IToggle.CanToggle( in bool isOn ) => CanToggle( isOn );

	protected virtual bool CanToggle( in bool isOn )
		=> IsOpened ? CanClose() : CanOpen();

	protected virtual void Toggle( in bool isOn )
		=> TryActivate( value: isOn );

	bool IActivate.CanActivate( object source )
		=> CanToggle( State.Reverse() is DoorState.Opened or DoorState.Opening );

	/// <summary>
	/// Toggles the door.
	/// <br /> <br />
	/// <b> NOTE: </b> Specify <paramref name="value"/> as a <c>bool</c> to open/close.
	/// <br /> <br />
	/// <c>true</c>: Open <br />
	/// <c>false</c>: Close <br />
	/// <c>default</c>: Toggle <br />
	/// </summary>
	/// <param name="source"></param>
	/// <param name="value"> Can be a <c>bool</c> or <see cref="ToggleCommand"/> to open/close/toggle. </param>
	public bool TryActivate( object source = null, object value = null )
	{
		if ( !GameObject.IsValid() )
			return false;

		if ( DebugLogicLogging )
			this.Log( $"TryActivate: {value}" );

		return value switch
		{
			true => TryOpen(),
			false => TryClose(),

			ToggleCommand.Enable => TryOpen(),
			ToggleCommand.Disable => TryClose(),
			ToggleCommand.Toggle => TryToggle(),

			_ => TryToggle()
		};
	}
}
