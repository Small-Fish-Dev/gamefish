namespace GameFish;

/// <summary>
/// Something that supports physics and has an <see cref="ActorModel"/>.
/// </summary>
[Title( "Actor" )]
[Icon( "theater_comedy" )]
public partial class BaseActor : PhysicsEntity
{
	public const string ACTOR = "🎭 Actor";

	/// <summary>
	/// The model of the actor, which may be <see cref="ActorSkinnedModel"/> or some other kind.
	/// </summary>
	[Property]
	[Feature( ACTOR ), Group( ActorModel.MODEL )]
	public ActorModel Model
	{
		get => _model.IsValid() ? _model
			: _model = Components?.Get<ActorModel>( FindMode.EverythingInSelfAndDescendants );

		set => _model = value;
	}

	protected ActorModel _model;
}
