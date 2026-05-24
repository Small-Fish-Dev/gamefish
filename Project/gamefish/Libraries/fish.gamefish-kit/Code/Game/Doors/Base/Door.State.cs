namespace GameFish;

partial class Door
{
	/// <summary>
	/// If enabled: the door starts locked.
	/// </summary>
	[Property]
	[Title( "Locked" )]
	[Feature( DOOR ), Order( DOOR_ORDER )]
	public virtual bool StartsLocked { get; set; } = false;

	/// <summary>
	/// The state the door starts in.
	/// </summary>
	[Property]
	[Title( "Starts" )]
	[WideMode, EnumButtonGroup]
	[Feature( DOOR ), Order( DOOR_ORDER )]
	public virtual DoorState InitialState { get; set; } = DoorState.Closed;

	/// <summary>
	/// If the door is open, closed or in-between.
	/// </summary>
	[Sync]
	public DoorState State
	{
		get => _state ?? DoorState.Unset;
		protected set
		{
			if ( _state is DoorState ds && ds == value )
				return;

			var wasOpen = _state;
			_state = value;

			OnSetState( in value, in wasOpen );
		}
	}

	protected DoorState? _state;

	/// <summary>
	/// If the door is fully open.
	/// </summary>
	public bool IsOpened => State is DoorState.Opened;

	/// <summary>
	/// If the door is actively opening(not yet fully opened).
	/// </summary>
	public bool IsOpening => State is DoorState.Opening;

	/// <summary>
	/// If the door is fully closed.
	/// </summary>
	public bool IsClosed => State is DoorState.Closed;

	/// <summary>
	/// If the door is active closing(not yet fully closed).
	/// </summary>
	public bool IsClosing => State is DoorState.Closing;

	/// <summary>
	/// If the door is actively opening or closing(still moving).
	/// </summary>
	public bool IsMoving => State is DoorState.Opening or DoorState.Closing;

	protected virtual void OnStateStart()
	{
		if ( IsProxy )
			return;

		State = InitialState;

		if ( StartsLocked )
			IsLocked = true;
	}

	protected virtual void OnSetState( in DoorState state, in DoorState? oldState )
	{
		if ( !InGame )
			return;

		if ( DebugStateLogging )
			this.Log( $"State: {state}" );

		switch ( state )
		{
			case DoorState.Opened:
				OnOpened();
				break;
			case DoorState.Opening:
				OnOpening();
				break;
			case DoorState.Closed:
				OnClosed();
				break;
			case DoorState.Closing:
				OnClosing();
				break;
		}
	}

	/// <summary>
	/// The door has fully opened.
	/// </summary>
	protected virtual void OnOpened()
	{
		OnAnimationOpened();

		foreach ( var m in DoorModules )
			m.OnOpened();

		OnLogicOpened();
		PlayOpenedEffects();
	}

	/// <summary>
	/// The door has started opening.
	/// </summary>
	protected virtual void OnOpening()
	{
		OnAnimationOpening();

		foreach ( var m in DoorModules )
			m.OnOpening();

		OnLogicOpening();
		PlayOpeningEffects();
	}

	/// <summary>
	/// The door has fully closed.
	/// </summary>
	protected virtual void OnClosed()
	{
		OnAnimationClosed();

		foreach ( var m in DoorModules )
			m.OnClosed();

		OnLogicClosed();
		PlayClosedEffects();
	}

	/// <summary>
	/// The door is starting to close.
	/// </summary>
	protected virtual void OnClosing()
	{
		OnAnimationClosing();

		foreach ( var m in DoorModules )
			m.OnClosing();

		OnLogicClosing();
		PlayClosingEffects();
	}

	protected virtual void SetOpen( in bool isOpen )
	{
		if ( DebugStateLogging )
			this.Log( $"SetOpen: isOpen" );

		if ( isOpen )
		{
			if ( State is DoorState.Opened or DoorState.Opening )
				return;

			State = DoorState.Opening;
		}
		else
		{
			if ( State is DoorState.Closed or DoorState.Closing )
				return;

			State = DoorState.Closing;
		}
	}

	public virtual bool CanOpen()
	{
		if ( !GameObject.IsValid() )
			return false;

		if ( IsLocked )
			return false;

		return IsClosed || IsClosing;
	}

	public virtual bool TryOpen()
	{
		if ( IsProxy )
			return false;

		if ( IsOpened )
			return false;

		if ( !CanOpen() )
		{
			if ( DebugStateLogging )
				this.Log( $"Failed to open." );

			return false;
		}

		Open();

		return true;
	}

	protected virtual void Open()
	{
		if ( IsClosed )
			PlayOpeningEffects();

		SetOpen( true );
	}

	public virtual bool CanClose()
	{
		if ( !GameObject.IsValid() )
			return false;

		return IsOpened || IsOpening;
	}

	public virtual bool TryClose()
	{
		if ( IsProxy )
			return false;

		if ( !CanClose() )
		{
			if ( DebugStateLogging )
				this.Log( $"Failed to close." );

			return false;
		}

		Close();

		return true;
	}

	public virtual bool TryToggle()
	{
		if ( IsProxy )
			return false;

		if ( IsOpened || IsOpening )
			return TryClose();

		if ( IsClosed || IsClosing )
			return TryOpen();

		return false;
	}

	protected virtual void Close()
	{
		SetOpen( false );
	}
}
