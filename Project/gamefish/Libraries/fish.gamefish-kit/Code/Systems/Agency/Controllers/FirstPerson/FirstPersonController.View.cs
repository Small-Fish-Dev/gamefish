namespace GameFish;

partial class FirstPersonController
{
	[Property]
	[Feature( VIEW ), Group( COLLISION )]
	[ToggleGroup( nameof( ViewCollisionEnabled ), Label = COLLISION )]
	public bool ViewCollisionEnabled { get; set; } = true;

	[Property]
	[Title( "Radius" )]
	[Feature( VIEW ), Group( COLLISION )]
	[ToggleGroup( nameof( ViewCollisionEnabled ) )]
	[Range( 0f, 1f, clamped: false ), Step( 0.01f )]
	public float ViewCollisionRadius { get; set; } = 14f;

	public override Vector3 GetLocalEyeTargetPosition()
		=> Vector3.Up * (IsDucking ? EyeHeightDuck : EyeHeightStand);

	protected override void UpdateEyePosition( in float deltaTime )
	{
		base.UpdateEyePosition( deltaTime );

		if ( ViewCollisionEnabled )
			UpdateEyeCollision( in deltaTime );
	}

	protected virtual void UpdateEyeCollision( in float deltaTime )
	{
		var radius = ViewCollisionRadius * WorldScale.x;
		var skin = SkinWidth;

		var tWorld = WorldTransform;
		var vUp = Up;

		var zMin = radius + skin; //.Max( EyeHeightDuck );
		var startPos = WorldPosition + (vUp * zMin);
		var eyePos = tWorld.PointToWorld( LocalEyePosition );

		var trHead = Trace( startPos, eyePos )
			.Radius( radius )
			.Run();

		if ( trHead.Hit )
			LocalEyePosition = tWorld.PointToLocal( trHead.EndPosition );
	}
}
