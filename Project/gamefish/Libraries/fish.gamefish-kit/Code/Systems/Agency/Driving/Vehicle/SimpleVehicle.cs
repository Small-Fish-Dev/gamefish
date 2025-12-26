namespace GameFish;

public partial class SimpleVehicle : Vehicle
{
	protected override void ApplyForces( in float deltaTime, in bool isFixedUpdate )
	{
		if ( IsProxy || !Rigidbody.IsValid() )
			return;

		var rDrive = WorldRotation;

		if ( InputAcceleration != 0f )
			Velocity += rDrive.Forward * InputAcceleration * 1000f * deltaTime;

		if ( InputSteering != 0f )
			Rigidbody.AngularVelocity += rDrive.Up * InputSteering * 5f * deltaTime;
	}
}
