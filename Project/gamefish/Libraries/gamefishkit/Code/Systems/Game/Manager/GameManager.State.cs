using System;

namespace GameFish;

partial class GameManager
{
	protected const int STATE_ORDER = GAME_ORDER + 20;
	protected const int STATE_DEFAULTS_ORDER = STATE_ORDER - 1;

	/// <summary>
	/// Log state changes and such?
	/// </summary>
	[Property]
	[Title( "Logging" )]
	[Group( DEBUG ), Order( DEBUG_ORDER )]
	[Feature( STATE, Description = "Modular gameplay logic." )]
	public virtual bool StateLogging { get; set; } = false;

	/// <summary>
	/// Automatically select a state if we don't have one? <br />
	/// Will choose depending on the context. <br />
	/// <br />
	/// <b> TIP: </b> Make a custom <see cref="GameState"/> component
	/// and put it on the root of a prefab, then drag it below.
	/// </summary>
	[Property]
	[Title( "Auto Select" )]
	[Feature( STATE ), Group( DEFAULTS ), Order( STATE_DEFAULTS_ORDER )]
	public virtual bool AutoStateSelection { get; set; } = true;

	/// <summary>
	/// The default <see cref="GameState"/> prefab to use in menu scenes.
	/// <br /> <br />
	/// <b> TIP: </b> Add a <see cref="SceneSettings"/> component to a scene.
	/// There's a <see cref="SceneSettings.InMainMenu"/> option.
	/// </summary>
	[Property]
	[Title( "Menu State" )]
	[Feature( STATE ), Group( DEFAULTS ), Order( STATE_DEFAULTS_ORDER )]
	public virtual PrefabFile DefaultMenuState { get; set; }

	/// <summary>
	/// The default <see cref="GameState"/> prefab to use outside of menu scenes.
	/// </summary>
	[Property]
	[Title( "Game State" )]
	[Feature( STATE ), Group( DEFAULTS ), Order( STATE_DEFAULTS_ORDER )]
	public virtual PrefabFile DefaultGameState { get; set; }

	[Property]
	[Title( "State" )]
	[ShowIf( nameof( InGame ), true )]
	[Feature( STATE ), Group( DEBUG ), Order( DEBUG_ORDER )]
	protected GameState InspectorState
	{
		get => State;
		set => State = value;
	}

	[Sync( SyncFlags.FromHost )]
	public GameState State
	{
		get => _state.AsValid();

		protected set
		{
			if ( _state == value )
				return;

			var old = _state;
			_state = value;

			if ( this.InGame() )
				OnStateSet( _state, old );
		}
	}

	protected GameState _state;

	protected virtual void OnStateSet( GameState newState, GameState oldState )
	{
		if ( StateLogging )
		{
			if ( !newState.IsValid() )
			{
				this.Log( $"State exited, old:[{oldState}]" );
				return;
			}

			if ( oldState.IsValid() )
				this.Log( $"State entered, new:[{newState}] from old:[{oldState}]" );
			else
				this.Log( $"State entered, new:[{newState}]" );
		}

		if ( !Networking.IsHost )
			return;

		try
		{
			// Tell previous state we exited.
			if ( oldState.IsValid() )
				oldState.OnExit( this, newState: newState );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( oldState.OnExit )} exception: {e}" );
		}

		try
		{
			// Tell new state we entered it.
			if ( newState.IsValid() )
				newState.OnEnter( this, oldState: oldState );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( newState.OnEnter )} exception: {e}" );
		}
	}

	public virtual bool TrySetState( GameState newState )
	{
		if ( !Networking.IsHost )
			return false;

		if ( !newState.IsValid() )
		{
			this.Warn( "Tried to enter invalid/null state:" + newState );
			return false;
		}

		if ( !newState.TryEnter() )
			return false;

		var prevState = State;

		State = newState;

		return true;
	}

	public virtual bool TrySetState( PrefabFile prefab )
	{
		if ( !Networking.IsHost )
			return false;

		if ( !prefab.IsValid() || !prefab.TrySpawn( out var objState ) )
		{
			this.Warn( $"Tried to enter invalid state prefab:[{prefab}]" );
			return false;
		}

		if ( !objState.Components.TryGet<GameState>( out var state ) )
		{
			this.Warn( $"Tried to enter state prefab:[{prefab}] with no {typeof( GameState )}!" );
			objState.Destroy();
			return false;
		}

		if ( !TrySetState( state ) )
		{
			this.Warn( $"Failed to enter spawned state:[{state}] from prefab:[{prefab}]" );
			objState.Destroy();
			return false;
		}

		objState.SetParent( GameObject, keepWorldPosition: false );
		objState.LocalTransform = global::Transform.Zero;

		return true;
	}

	/// <summary>
	/// Tries to assign a state if we don't have one.
	/// </summary>
	public virtual void SelectState()
	{
		if ( !Networking.IsHost )
			return;

		if ( State.IsValid() || !InGame )
			return;

		var state = InMenu ? DefaultMenuState : DefaultGameState;

		if ( SceneSettings.TryGetInstance( out var s ) )
			if ( s.GameManagerStateOverride.IsValid() )
				state = s.GameManagerStateOverride;

		if ( state.IsValid() )
			TrySetState( state );
	}

	protected virtual void SimulateState( in float deltaTime )
	{
		if ( !State.IsValid() )
			return;

		try
		{
			State.Simulate( in deltaTime );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( SimulateState )} exception: {e}" );
		}
	}
}
