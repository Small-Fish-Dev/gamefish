namespace Fishbox;

public class ShooterGravityModule : GravityModule
{
	public override void ApplyGravity( in float deltaTime )
	{
		var grav = GetGravity();

		if ( grav == default )
			return;

		Velocity += grav * deltaTime;

		var gravUp = -grav.Normal;

		if ( !Field.IsValid() )
			gravUp = -(Scene?.PhysicsWorld?.Gravity.Normal ?? Vector3.Up);

		var sc = (Parent as Pawn)?.Controller as ShooterController;

		if ( sc.IsValid() && sc.Up != gravUp )
			sc.TryReorient( gravUp );
	}
}
