namespace GameFish;

/// <summary>
/// Something that supports physics and has an <see cref="ActorModel"/>.
/// </summary>
public partial class BaseActor : PhysicsEntity
{
	public const string FEATURE_ACTOR = "🎭 Actor";
	public const string GROUP_ACTOR = FEATURE_ACTOR;

	/// <summary>
	/// The model of the actor, which may be <see cref="ActorSkinnedModel"/> or some other kind.
	/// </summary>
	[Property, Feature( FEATURE_ACTOR )]
	public ActorModel Model { get; set; }
}
