namespace GameFish;

/// <summary>
/// The most basic form of an object that can separately exist.
/// </summary>
public partial class BaseEntity : Component, ITransform
{
	public const string DEBUG = "🐞 Debug";
	public const string MODULES = "🧩 Modules";

	public const string FEATURE_ENTITY = "📦 Entity";
	public const string FEATURE_NPC = "🤖 NPC";

	public const string TAG_ENTITY = "entity";
	public const string TAG_PROJECTILE = "projectile";

	public const string TAG_ACTOR = "actor";
	public const string TAG_PLAYER = "player";
	public const string TAG_NPC = "npc";
}
