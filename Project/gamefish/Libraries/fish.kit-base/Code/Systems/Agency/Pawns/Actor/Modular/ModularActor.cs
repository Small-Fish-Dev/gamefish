namespace GameFish;

/// <summary>
/// A highly modular autonomous pawn with a wide variety of features supported by default.
/// <br />
/// <b> NOTE: </b> This modular version is very much a work in progress.
/// It is suggested that you use <see cref="SimpleActor"/> meanwhile.
/// </summary>
[Hide]
public partial class ModularActor : Actor
{
	protected const int MIND_ORDER = ACTOR_ORDER - 1000;
	protected const int MOVEMENT_ORDER = MIND_ORDER + 100;
	protected const int DETECTION_ORDER = MIND_ORDER + 200;

	/// <summary>
	/// The core automation and processing center responsible for perception and behavior.
	/// </summary>
	[Property]
	[Feature( ACTOR ), Group( MIND ), Order( MIND_ORDER )]
	public virtual ActorMind Mind
	{
		get => _mind.GetCached( this );
		set => _mind = value;
	}

	protected ActorMind _mind;

	/// <summary>
	/// The navigation module that tells this where/how to move.
	/// </summary>
	[Property]
	[Feature( ACTOR ), Group( MOVEMENT ), Order( MOVEMENT_ORDER )]
	public virtual ActorNavigation Navigation
	{
		get => _nav.GetCached( this );
		set => _nav = value;
	}

	protected ActorNavigation _nav;

	/// <summary>
	/// The detection module that checks sight, hearing, etc.
	/// </summary>
	[Property]
	[Feature( ACTOR ), Group( DETECTION ), Order( DETECTION_ORDER )]
	public virtual ActorDetection Detection
	{
		get => _detection.GetCached( this );
		set => _detection = value;
	}

	protected ActorDetection _detection;

	public override void FrameSimulate( in float deltaTime )
	{
		if ( Mind.IsValid() && Mind.CanSimulate() )
			Mind?.Simulate( in deltaTime, isFixedUpdate: false );
	}

	public override void FixedSimulate( in float deltaTime )
	{
	}

	protected override void Move( in float deltaTime, in bool isFixedUpdate )
	{
		if ( !Controller.IsValid() )
			return;

		Vector3 wishVel = default;
		Mind?.PreMove( in deltaTime, Controller.GetWishSpeed(), ref wishVel );

		Controller.TryMove( in deltaTime, in isFixedUpdate, in wishVel );
	}

	protected override void Think( in float deltaTime, in bool isFixedUpdate )
		=> Mind?.Think( in deltaTime, in isFixedUpdate );
}
