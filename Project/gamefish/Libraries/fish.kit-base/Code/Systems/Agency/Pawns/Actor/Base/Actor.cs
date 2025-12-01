namespace GameFish;

/// <summary>
/// An autonomous pawn. An NPC, basically.
/// </summary>
[Icon( "theater_comedy" )]
[EditorHandle( Icon = "🤖" )]
public abstract partial class Actor : Pawn
{
	protected const int ACTOR_ORDER = PAWN_ORDER - 100;

	public override bool IsPlayer { get; } = false;

	protected override Vector3 InputMoveDirection => default;

	/// <summary>
	/// Is this NPC meant to be thinking?
	/// It probably shouldn't if it's dead.
	/// </summary>
	public virtual bool IsThinking => this.IsValid() && IsAlive;

	protected override void OnStart()
	{
		base.OnStart();

		// ...
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !Owner.IsValid() && CanSimulate() )
			FrameSimulate( Time.Delta );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( !Owner.IsValid() && CanSimulate() )
			FixedSimulate( Time.Delta );
	}

	public override bool AllowInput()
		=> false;

	public override bool CanSimulate()
	{
		if ( !this.IsValid() || IsProxy )
			return false;

		return true;
	}

	public override void FrameSimulate( in float deltaTime )
	{
		if ( IsThinking )
			Think( in deltaTime, isFixedUpdate: false );

		DoMovement( in deltaTime, isFixedUpdate: false );
	}

	protected abstract void Think( in float deltaTime, in bool isFixedUpdate );

	/// <summary>
	/// Tells the actor where and how to go.
	/// </summary>
	protected virtual void UpdateNavigation( in float deltaTime )
	{
	}

	/// <summary>
	/// Look out for things of interest.
	/// </summary>
	public virtual void UpdatePerception( in float deltaTime )
	{
	}

	protected override void DoAiming( in float deltaTime )
	{
	}

	protected override void Move( in float deltaTime, in bool isFixedUpdate )
	{
		if ( Controller.IsValid() )
			Controller.TryMove( in deltaTime, in isFixedUpdate, GetWishVelocity() );
	}
}
