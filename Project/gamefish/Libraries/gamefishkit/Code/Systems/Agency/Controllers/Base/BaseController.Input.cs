namespace GameFish;

partial class BaseController
{
	protected override void OnStart()
	{
		base.OnStart();

		LocalEyePosition = GetLocalEyeTargetPosition();
	}

	public virtual float GetWishSpeed()
	{
		if ( !AllowMovement || Pawn?.IsAlive is not true )
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
		var wishSpeed = GetWishSpeed();

		if ( wishSpeed == 0f )
			return Vector3.Zero;

		return GetWishDirection( in inputDir ) * wishSpeed;
	}

	public virtual void UpdateView( in float deltaTime )
	{
		if ( IsProxy )
			return;

		UpdateEyePosition( deltaTime );
	}

	/// <summary>
	/// Performs eye position target transitioning.
	/// </summary>
	public virtual void UpdateEyePosition( in float deltaTime )
	{
		LocalEyePosition = Vector3.SmoothDamp( LocalEyePosition, GetLocalEyeTargetPosition(),
			ref _eyeVel, EyeMoveSmoothing, EyeMoveSpeed * deltaTime );
	}
}
