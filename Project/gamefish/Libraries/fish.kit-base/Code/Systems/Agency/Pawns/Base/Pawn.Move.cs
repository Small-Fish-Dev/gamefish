namespace GameFish;

partial class Pawn
{
	/// <summary>
	/// The component responsible for using input to aim and move.
	/// </summary>
	[Property]
	[Order( PAWN_ORDER )]
	[Feature( PAWN ), Group( MOVEMENT )]
	public virtual BaseController Controller
	{
		get => _controller.IsValid() ? _controller
			: _controller = Components?.Get<BaseController>( FindMode.EverythingInSelf );

		set => _controller = value;
	}

	protected BaseController _controller;

	/// <summary>
	/// Could be an animated model or a sprite.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( BODY )]
	public virtual PawnBody BodyComponent { get; set; }

	/// <summary>
	/// The pawn's input direction(as if on an analogue stick).
	/// </summary>
	protected virtual Vector3 InputMoveDirection => Input.AnalogMove.ClampLength( 1f );

	/// <returns> The intended movement speed for this pawn. </returns>
	public virtual float GetWishSpeed()
		=> IsAlive && Controller.IsValid() ? Controller.GetWishSpeed() : 0f;

	/// <returns> The pawn's currently intended movement velocity. </returns>
	public virtual Vector3 GetWishVelocity()
		=> Controller.IsValid() ? Controller.WishVelocity : Vector3.Zero;

	/// <summary>
	/// Sets the pawn's intended movement velocity.
	/// </summary>
	public virtual void SetWishVelocity( in Vector3 wishVel )
	{
		if ( Controller.IsValid() )
			Controller.WishVelocity = wishVel;
	}

	/// <summary>
	/// Sets up and executes this pawn's movement(if it has any).
	/// </summary>
	protected virtual void DoMovement( in float deltaTime, in bool isFixedUpdate )
	{
		if ( Controller.IsValid() )
			Controller.Simulate( in deltaTime, in isFixedUpdate );

		Move( in deltaTime, in isFixedUpdate );
	}

	/// <summary>
	/// Directly tells this pawn to perform its movement logic.
	/// </summary>
	protected virtual void Move( in float deltaTime, in bool isFixedUpdate )
	{
		if ( !Controller.IsValid() )
			return;

		// Player input by default.
		var inputDir = InputMoveDirection;
		var wishVel = Controller.GetWishVelocity( inputDir );

		Controller.TryMove( in deltaTime, in isFixedUpdate, in wishVel );
	}

	public override bool TryTeleport( in Transform tDest )
	{
		WorldPosition = tDest.Position;

		// Don't rotate the object itself.
		EyeRotation = tDest.Rotation;

		return true;
	}
}
