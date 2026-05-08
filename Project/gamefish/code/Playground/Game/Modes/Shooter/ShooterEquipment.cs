namespace Fishbox;

public class ShooterEquipment : Equipment
{
	public virtual bool IsSighted => Input.Down( "Attack2" );

	public override void UpdateOffset( in float speed, in float deltaTime )
	{
		base.UpdateOffset( speed, deltaTime );

		if ( !Pawn.IsValid() )
			return;

		var sway = IsSighted ? 0.01f : 0.02f;

		var vel = Pawn.EyeRotation.Inverse * Pawn.Velocity;
		vel = (vel * deltaTime * sway).ClampLength( 10f );

		var bobOffset = Offset;
		bobOffset.Position -= vel;

		Offset = bobOffset;
	}

	public override Offset GetViewOffsetTarget()
	{
		if ( IsSighted )
			return AimingOffset;

		return base.GetViewOffsetTarget();
	}
}
