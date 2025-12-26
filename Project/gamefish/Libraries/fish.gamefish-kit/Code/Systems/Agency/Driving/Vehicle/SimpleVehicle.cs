namespace GameFish;

public partial class SimpleVehicle : Vehicle
{
	[Sync]
	public float DriveDirection { get; set; }

	[Sync]
	public float SteerDirection { get; set; }

	protected override void ApplyForces( in float deltaTime, in bool isFixedUpdate )
	{
		if ( IsProxy || !Rigidbody.IsValid() )
			return;

		var rDrive = WorldRotation;

		Velocity += rDrive.Forward * DriveDirection * 1000f * deltaTime;
		Rigidbody.AngularVelocity += rDrive.Up * SteerDirection * 5f * deltaTime;
	}

	public override void UpdateInput( Seat seat, Pawn sitter, in float deltaTime, in bool isFixedUpdate )
	{
		if ( IControls.BlockMoving )
			return;

		var vMove = Input.AnalogMove;

		var driveDir = vMove.x;
		var steerDir = vMove.y;

		if ( Input.Down( "Forward" ) )
			driveDir += 1f;

		if ( Input.Down( "Backward" ) )
			driveDir -= 1f;

		if ( Input.Down( "Left" ) )
			steerDir += 1f;

		if ( Input.Down( "Right" ) )
			steerDir -= 1f;

		driveDir = driveDir.Clamp( -1f, 1f );
		steerDir = steerDir.Clamp( -1f, 1f );

		if ( driveDir != DriveDirection || steerDir != SteerDirection )
			RpcOperate( driveDir, steerDir );
	}

	[Rpc.Owner( NetFlags.UnreliableNoDelay )]
	protected void RpcOperate( float driveDir, float steerDir )
	{
		DriveDirection = driveDir.Clamp( -1f, 1f );
		SteerDirection = steerDir.Clamp( -1f, 1f );

		// this.Log( $"drive: {DriveDirection}, steer: {SteerDirection}" );
	}
}
