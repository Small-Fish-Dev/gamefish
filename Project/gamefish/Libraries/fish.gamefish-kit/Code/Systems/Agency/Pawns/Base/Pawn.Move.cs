namespace GameFish;

partial class Pawn : IMove, IGravity
{
	/// <summary>
	/// The component responsible for using input to aim and move.
	/// </summary>
	[Property]
	[Order( PAWN_ORDER )]
	[Feature( PAWN ), Group( MOVEMENT )]
	public virtual PawnController Controller
	{
		get => this?.GetCached( ref _controller );
		protected set => _controller = value;
	}

	protected PawnController _controller;

	public virtual Vector3 Gravity => Controller?.Gravity ?? default;

	public override Vector3 Velocity
	{
		get
		{
			if ( Controller.IsValid() )
				return Controller.Velocity;
			else if ( Rigidbody.IsValid() )
				return Rigidbody.Velocity;

			return default;
		}
		set
		{
			if ( Controller.IsValid() )
				Controller.Velocity = value;
			else if ( Rigidbody.IsValid() )
				Rigidbody.Velocity = value;
		}
	}

	public Vector3 WishVelocity
	{
		get => Controller?.WishVelocity ?? default;
		set
		{
			if ( Controller is var c && c.IsValid() )
				c.WishVelocity = value;
		}
	}

	/// <returns> The pawn's currently intended movement velocity. </returns>
	public virtual Vector3 CalculateWishVelocity()
	{
		if ( !IsAlive )
			return default;

		var c = Controller;

		if ( !c.IsValid() )
			return default;

		Vector3? inputDir = null;

		if ( Owner is Client cl )
		{
			if ( cl.TryGetMove( out var clMoveDir ) )
				inputDir = clMoveDir;
		}

		return c.CalculateWishVelocity( inputDir );
	}

	/// <summary>
	/// Directly tells this pawn to perform its movement logic.
	/// </summary>
	public virtual void Move( in float deltaTime, in bool isFixedUpdate )
	{
		if ( Seat.IsValid() )
		{
			WishVelocity = default;
			FollowSeat( Seat );
			return;
		}

		if ( !Controller.IsValid() )
			return;

		// Player-only input by default.
		Controller.WishVelocity = CalculateWishVelocity();

		Controller.Simulate( in deltaTime, in isFixedUpdate );
		Controller.TryMove( in deltaTime, in isFixedUpdate );
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
