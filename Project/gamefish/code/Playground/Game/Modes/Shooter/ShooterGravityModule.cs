namespace Fishbox;

public class ShooterGravityModule : GravityModule
{
	public override void ApplyGravity( in float deltaTime )
	{
		var grav = GetGravity();

		if ( grav == default )
			return;

		Velocity += grav * deltaTime;

		var sc = (Parent as Pawn)?.Controller as ShooterController;

		if ( sc.IsValid() )
		{
			var gravUp = -grav.Normal;

			if ( sc.Up != gravUp )
				sc.TryReorient( gravUp );
		}
	}
}
