namespace GameFish;

/// <summary>
/// An auto-registering module for <see cref="ModularActor"/>.
/// </summary>
public abstract partial class ActorModule : Module
{
	protected const int NPC_ORDER = DEFAULT_ORDER - 1997;

	public override bool IsParent( ModuleEntity comp )
		=> comp is ModularActor;

	public virtual ModularActor Actor => Parent as ModularActor;

	public bool IsAlive => Actor?.IsAlive is true;

	public ActorMind Mind => Actor?.Mind;
	public ActorDetection Detection => Actor?.Detection;
	public ActorNavigation Navigation => Actor?.Navigation;

	/// <summary> The movement controller. </summary>
	public BaseController Controller => Actor?.Controller;
	public bool IsGrounded => Controller?.IsGrounded is true;

	/// <summary> The actively select behavior(if any). </summary>
	public ActorBehavior Behavior => Mind?.Behavior;


	public int Seed => Actor?.Seed ?? 0;

	/// <summary>
	/// Tells the game to use this guy's randomization offset.
	/// </summary>
	public void RestoreSeed() => Actor?.RestoreSeed();


	/// <summary> The pawn we're aiming for. </summary>
	public Pawn Target
	{
		get => Mind?.Target;
		set => Mind?.SetTarget( value );
	}

	/// <summary> When the target was last directly within our vision. </summary>
	public TimeSince? LastSeenTarget => Mind?.LastSeenTarget;

	/// <summary> The origin of the target when they were last seen. </summary>
	public Vector3? LastKnownTargetPosition => Mind?.LastKnownTargetPosition;

	/// <summary> Where we're trying to look at. </summary>
	public Vector3? TargetAimPosition => Mind?.TargetAimPosition;

	/// <summary> Do we have a valid target? </summary>
	public bool IsTargetValid() => Mind?.Target.IsValid() is true;

	/// <summary> Is the target valid and currently visible? </summary>
	public bool IsTargetVisible() => IsTargetValid() && Detection?.TargetVisible is true;


	public Vector3 WishVelocity
	{
		get => Actor?.GetWishVelocity() ?? default;
		set => Actor?.SetWishVelocity( in value );
	}

	public Vector3 Velocity
	{
		get => Actor?.Velocity ?? default;
		set
		{
			if ( Actor.IsValid() )
				Actor.Velocity = value;
		}
	}


	/// <summary> The current equipment. </summary>
	public Equipment ActiveEquip
	{
		get => Actor?.ActiveEquip;
		set
		{
			if ( Actor.IsValid() )
				Actor.ActiveEquip = value;
		}
	}

	public Vector3 EyePosition
	{
		get => Actor?.EyePosition ?? WorldPosition;
		set
		{
			if ( Actor.IsValid() )
				Actor.EyePosition = value;
		}
	}

	public Rotation EyeRotation
	{
		get => Actor?.EyeRotation ?? Rotation.Identity;
		set
		{
			if ( Actor.IsValid() )
				Actor.EyeRotation = value;
		}
	}

	public Transform EyeTransform => new( EyePosition, EyeRotation, Actor?.WorldScale ?? Vector3.One );
	public Vector3 EyeForward => EyeRotation.Forward;

	public virtual bool CanSimulate()
		=> !IsProxy && Actor.IsValid();

	public bool IsEnemy( Pawn target )
		=> Actor?.IsEnemy( target ) is true;

	public SceneTrace GetEyeTrace()
		=> Actor?.GetEyeTrace() ?? default;
}
