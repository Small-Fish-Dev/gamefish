namespace GameFish;

partial class BaseController
{
	protected virtual bool IsMovementAllowed()
		=> AllowMovement && Pawn?.IsAlive is true;

	public virtual float GetMovementSpeed()
	{
		if ( !IsMovementAllowed() )
			return 0f;

		return MoveSpeed;
	}

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
