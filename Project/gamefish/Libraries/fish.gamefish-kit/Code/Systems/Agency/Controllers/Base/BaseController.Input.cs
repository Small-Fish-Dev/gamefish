namespace GameFish;

partial class BaseController
{
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
		if ( !Pawn.IsValid() )
			return false;

		return Pawn.IsAlive;
	}

	/// <returns> The intended movement speed(if allowed). </returns>
	public virtual float GetMovementSpeed()
		=> MoveSpeed;

	public virtual Vector3 GetWishDirection( in Vector3? inputDir = null )
	{
		if ( inputDir is not Vector3 moveInput )
			return default;

		var up = WorldRotation.Up;

		var flatAim = Vector3.VectorPlaneProject( EyeForward, up );
		var rMove = Rotation.LookAt( flatAim, up );

		return rMove * moveInput;
	}

	public virtual Vector3 GetWishVelocity( in Vector3? inputDir = null )
	{
		var wishSpeed = GetMovementSpeed();

		if ( wishSpeed.AlmostEqual( 0f ) )
			return Vector3.Zero;

		return GetWishDirection( in inputDir ) * wishSpeed;
	}
}
