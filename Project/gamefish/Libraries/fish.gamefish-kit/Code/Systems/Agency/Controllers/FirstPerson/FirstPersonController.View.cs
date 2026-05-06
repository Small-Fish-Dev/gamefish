namespace GameFish;

partial class FirstPersonController
{
	[Property]
	[Feature( VIEW ), Group( COLLISION )]
	[ToggleGroup( nameof( ViewCollisionEnabled ), Label = COLLISION )]
	public bool ViewCollisionEnabled { get; set; } = true;

	[Property]
	[Title( "Radius" )]
	[Range( 0f, 32f, clamped: false )]
	[Feature( VIEW ), Group( COLLISION )]
	[ToggleGroup( nameof( ViewCollisionEnabled ) )]
	public float ViewCollisionRadius { get; set; } = 8f;

	public override Vector3 GetLocalEyeTargetPosition()
		=> Vector3.Up * (IsDucking ? EyeHeightDuck : EyeHeightStand);

	protected override void UpdateEyePosition( in float deltaTime )
	{
		var currentPos = LocalEyePosition;
		var eyeTargetPos = GetLocalEyeTargetPosition();

		// Move smoothly to the destination.
		var localDest = currentPos;

		localDest = Vector3.SmoothDamp( in localDest, in eyeTargetPos,
			ref _eyeVel, EyeMoveSmoothing, EyeMoveSpeed * deltaTime );

		// Test for collisions.
		localDest = TestEyeCollision( in currentPos, localDest, in deltaTime );

		// Apply the position.
		LocalEyePosition = localDest;
	}

	protected virtual Vector3 TestEyeCollision( in Vector3 currentPos, Vector3 localDest, in float deltaTime )
	{
		if ( !ViewCollisionEnabled )
			return localDest;

		var radius = ViewCollisionRadius * WorldScale.x;

		var tWorld = WorldTransform;
		var worldDest = tWorld.PointToWorld( localDest );

		var trHead = Trace( Center, worldDest )
			.Radius( radius )
			.Run();

		// If both are stuck then don't move.
		if ( trHead.StartedSolid )
			return currentPos;

		if ( trHead.Hit )
			localDest = tWorld.PointToLocal( trHead.EndPosition );

		return localDest;
	}
}
