using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// A state logic module for <see cref="GameManager"/>.
/// <br />
/// These let you switch your game's logic on the fly,
/// from a deathmatch gamemode to a voting screen or lobby.
/// </summary>
[Icon( "calculate" )]
[Title( "Game State" )]
public abstract partial class GameState : Module
{
	protected const int STATE_ORDER = DEFAULT_ORDER - 5000;
	protected const int PAWNS_ORDER = STATE_ORDER + 1000;

	/// <summary>
	/// The game manager this belongs to.
	/// </summary>
	public GameManager Manager => (Parent as GameManager).AsValid();

	public override bool IsParent( ModuleEntity comp )
		=> comp is GameManager;

	/// <summary>
	/// The main, actively selected game state.
	/// </summary>
	public static GameState Current => GameManager.Instance?.State;

	/// <returns> The currently active state(if any). </returns>
	public static bool TryGetCurrent( out GameState mode )
		=> (mode = Current).IsValid();

	/// <returns> The currently active <typeparamref name="TState"/>(if so). </returns>
	public static bool TryGetCurrent<TState>( out TState mode ) where TState : GameState
		=> (mode = GameManager.Instance?.State as TState).IsValid();

	public static bool InMenu => GameManager.InMenu;
	public static bool IsPlaying => !InMenu;

	/// <summary>
	/// Every currently loaded(yet not necessarily active) game state.
	/// </summary>
	public static IEnumerable<GameState> All => GameManager.Instance?.GetModules<GameState>();

	[Title( "Name" )]
	[Property, WideMode, JsonIgnore]
	[Feature( STATE ), Order( STATE_ORDER - 10 )]
	protected string InspectorName => Name;

	[TextArea]
	[Title( "Description" )]
	[Property, WideMode, JsonIgnore]
	[Feature( STATE ), Order( STATE_ORDER - 10 )]
	protected string InspectorDescription => Description;

	public virtual string Name { get; } = "State";
	public virtual string Description { get; } = "A game state.";

	/// <summary>
	/// Describes what this state is about.
	/// Might allow enabling certain features.
	/// </summary>
	public virtual TagSet StateTags { get; set; }

	/// <returns> If this is allowed to be set as the active state. </returns>
	public virtual bool CanEnter()
	{
		return true;
	}

	/// <returns> If the game manager should enter this state. </returns>
	public virtual bool TryEnter()
	{
		if ( !Networking.IsHost )
			return false;

		return CanEnter();
	}

	public virtual void OnEnter( GameManager gm, GameState oldState = null )
	{
	}

	public virtual void OnExit( GameManager gm, GameState newState = null )
	{
	}

	/// <summary>
	/// Called by the game manager to run this state's logic.
	/// </summary>
	public virtual void Simulate( in float deltaTime )
	{
	}
}
