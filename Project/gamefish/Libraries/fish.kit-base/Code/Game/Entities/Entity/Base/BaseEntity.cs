namespace GameFish;

/// <summary>
/// The most basic form of an object that can separately exist.
/// </summary>
public abstract partial class BaseEntity : Component, ITransform
{
	public const string ENTITY = "📦 Entity";
	public const int ENTITY_ORDER = -999999;

	protected const string NETWORKING = "📶 Networking";
	protected const int NETWORK_ORDER = ENTITY_ORDER + 1;

	public const string DEBUG = "🐞 Debug";
	public const int DEBUG_ORDER = NETWORK_ORDER + 1;

	public const string MODULES = "🧩 Modules";
	public const string CALLBACKS = "⚡ Callbacks";
	public const string ATTRIBUTES = "🍱 Attributes";

	public const string EFFECTS = "🎇 Effects";
	public const string FACTION = "🚩 Faction";
	public const string FLYING = "🦅 Flying";
	public const string HEALTH = "💖 Health";
	public const string COMBAT = "🔫 Combat";
	public const string EQUIP = "🏹 Equip";
	public const string INPUT = "🕹 Input";
	public const string MODEL = "🕺 Model";
	public const string VIEW = "🎥 View";

	public const string PLAYER = "😎 Player";
	public const string PAWN = "🐴 Pawn";
	public const string NPC = "🤖 NPC";

	public const string SPECTATOR = "👻 Spectator";
	public const int SPECTATOR_ORDER = 500;

	public const string TAG_EQUIP = "equip";
	public const string TAG_ENTITY = "entity";
	public const string TAG_PROJECTILE = "projectile";

	public const string TAG_SPECTATOR = "spectator";
	public const string TAG_PLAYER = "player";
	public const string TAG_PAWN = "pawn";
	public const string TAG_DEAD = "dead";
	public const string TAG_HULL = "hull";
	public const string TAG_NPC = "npc";

	/// <summary>
	/// Is this currently loaded in a valid editor scene? <br />
	/// You can use this with <see cref="HideIfAttribute"/> or <see cref="ShowIfAttribute"/>.
	/// </summary>
	public bool EditingScene => this.InEditor();

	/// <summary>
	/// Is this currently loaded in a valid play mode scene? <br />
	/// You can use this with <see cref="HideIfAttribute"/> or <see cref="ShowIfAttribute"/>.
	/// </summary>
	public bool PlayingScene => this.InGame();
}
