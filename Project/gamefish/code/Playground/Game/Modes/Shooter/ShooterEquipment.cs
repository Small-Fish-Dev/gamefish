namespace Fishbox;

public class ShooterEquipment : Equipment
{
	protected override void Simulate( in float deltaTime )
	{
		IsAiming = Input.Down( "Attack2" );

		base.Simulate( deltaTime );
	}
}
