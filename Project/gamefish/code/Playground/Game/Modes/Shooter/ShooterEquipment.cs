namespace Fishbox;

public class ShooterEquipment : Equipment
{
	public override Offset GetViewRendererOffset()
	{
		if ( Input.Down( "Attack2" ) )
			return AimingOffset;

		return base.GetViewRendererOffset();
	}
}
