namespace GameFish;

/// <summary>
/// An autonomous pawn. An NPC, basically.
/// </summary>
[Icon( "theater_comedy" )]
[EditorHandle( Icon = "🤖" )]
public abstract partial class Actor : Pawn
{
	protected const int ACTOR_ORDER = PAWN_ORDER - 1000;

	public override bool IsPlayer { get; } = false;

	/// <summary>
	/// Is this NPC meant to be thinking?
	/// It probably shouldn't if it's dead.
	/// </summary>
	public virtual bool IsThinking => GameObject.IsValid() && IsAlive;

	protected override void OnStart()
	{
		base.OnStart();

		ShuffleRandomSeed();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( Owner.IsValid() )
			return;

		OnActorUpdate( Time.Delta );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( Owner.IsValid() )
			return;

		OnActorFixedUpdate( Time.Delta );
	}

	protected virtual void OnActorUpdate( in float deltaTime )
	{
		if ( CanSimulate() )
			FrameSimulate( deltaTime );
	}

	protected virtual void OnActorFixedUpdate( in float deltaTime )
	{
		if ( CanSimulate() )
			FixedSimulate( deltaTime );
	}

	// Prevent NPCs from taking button input.
	public override bool AllowInput()
		=> false;

	public override bool CanSimulate()
	{
		if ( !GameObject.IsValid() )
			return false;

		return !IsProxy;
	}

	public override void FrameSimulate( in float deltaTime )
	{
		if ( IsThinking )
			Think( in deltaTime, isFixedUpdate: false );

		base.FrameSimulate( in deltaTime );
	}

	protected virtual void Think( in float deltaTime, in bool isFixedUpdate )
	{
		UpdateDetection( in deltaTime );
	}

	public override void Move( in float deltaTime, in bool isFixedUpdate )
	{
		if ( !Controller.IsValid() )
			return;

		Controller.WishVelocity = Controller.CalculateWishVelocity();

		Controller.Simulate( in deltaTime, in isFixedUpdate );
		Controller.TryMove( in deltaTime, in isFixedUpdate );
	}

	/// <summary>
	/// Tells the actor where and how to go.
	/// </summary>
	protected virtual void UpdateNavigation( in float deltaTime )
	{
	}

	// NPCs don't take player input.
	public override void UpdateView( in float deltaTime ) { }
	protected override void DoAiming( in float deltaTime ) { }
	protected override void UpdateInput( in float deltaTime ) { }
}
