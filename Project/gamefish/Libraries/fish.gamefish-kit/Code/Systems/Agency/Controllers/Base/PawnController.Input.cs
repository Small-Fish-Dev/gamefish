namespace GameFish;

partial class PawnController
{
	/// <summary>
	/// The velocity this is intending to move.
	/// </summary>
	[Sync]
	public Vector3 WishVelocity
	{
		get => _wishVel;
		set
		{
			if ( _wishVel == value )
				return;

			_wishVel = value;

			OnSetWishVelocity( in value );
		}
	}

	protected Vector3 _wishVel = Vector3.Zero;

	public virtual void OnSetWishVelocity( in Vector3 wishVel )
	{
	}

	/// <summary>
	/// Registers active inputs.
	/// </summary>
	protected virtual void UpdateInput( in float deltaTime )
	{
	}

	/// <summary>
	/// Clears all active inputs.
	/// </summary>
	protected virtual void ResetInput()
	{
	}

	/// <returns> If input should move this. </returns>
	protected virtual bool IsMovementAllowed()
	{
		if ( !MovementEnabled )
			return false;

		if ( !Pawn.IsValid() )
			return false;

		return Pawn.IsAlive;
	}

	/// <returns> The intended movement speed(if allowed). </returns>
	public virtual float GetMovementSpeed()
		=> MoveSpeed;

	public virtual Vector3 CalculateWishDirection( in Vector3? inputDir = null )
	{
		if ( inputDir is not Vector3 moveInput )
			return default;

		return Perspective * moveInput;
	}

	public virtual Vector3 CalculateWishVelocity( in Vector3? inputDir = null )
	{
		var wishSpeed = GetMovementSpeed();

		if ( wishSpeed.AlmostEqual( 0f ) )
			return Vector3.Zero;

		return CalculateWishDirection( in inputDir ) * wishSpeed;
	}
}
