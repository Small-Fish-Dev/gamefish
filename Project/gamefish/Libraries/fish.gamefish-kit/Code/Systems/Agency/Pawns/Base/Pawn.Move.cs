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
		get => _controller.GetCached( GameObject );
		protected set => _controller = value;
	}

	protected BaseController _controller;

	/// <returns> The intended movement speed for this pawn. </returns>
	public virtual float GetWishSpeed()
	{
		if ( !IsAlive || !Controller.IsValid() )
			return 0f;

		return Controller.GetWishSpeed();
	}

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
	/// Directly tells this pawn to perform its movement logic.
	/// </summary>
	protected override void Move( in float deltaTime, in bool isFixedUpdate )
	{
		if ( Seat.IsValid() )
		{
			FollowSeat( Seat );

			if ( Controller.IsValid() )
				Controller.WishVelocity = default;

			return;
		}

		if ( !Controller.IsValid() )
			return;

		Controller.Simulate( in deltaTime, in isFixedUpdate );

		// Player-only input by default.
		Vector3 wishVel = Vector3.Zero;

		if ( Owner is Client cl && cl.TryGetMove( out var moveDir ) )
			wishVel = Controller.GetWishVelocity( moveDir );

		Controller.TryMove( in deltaTime, in isFixedUpdate, in wishVel );
	}

	public override bool TryTeleport( in Transform tDest )
	{
		if ( IsProxy )
			return false;

		WorldPosition = tDest.Position;

		// Don't rotate the object itself.
		EyeRotation = tDest.Rotation;

		return true;
	}
}
