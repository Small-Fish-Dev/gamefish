namespace Fishbox;

public class ShooterEquipment : Equipment
{
	public override Offset GetViewOffsetTarget()
	{
		if ( Input.Down( "Attack2" ) )
			return AimingOffset;

		return base.GetViewOffsetTarget();
	}
}
