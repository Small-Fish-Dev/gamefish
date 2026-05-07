namespace GameFish;

/// <summary>
/// A very basic controller with sprinting, ducking and jumping. <br />
/// Ideal for use with first-person shooters.
/// </summary>
public partial class FirstPersonController : PawnController
{
	public override void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
		if ( ShouldJump() )
			Jump();

		base.Simulate( deltaTime, isFixedUpdate );
	}

	protected override void UpdateInput( in float deltaTime )
	{
		base.UpdateInput( deltaTime );

		var isAlive = Pawn?.IsAlive is true;

		IsDucking = isAlive && ShouldDuck();
		IsSprinting = isAlive && ShouldSprint();
	}

	public override float GetMovementSpeed()
	{
		float moveSpeed;

		// Affect move speed smoothly between stances.
		if ( DuckingEnabled )
			moveSpeed = LocalEyePosition.z.Remap( EyeHeightDuck, EyeHeightStand, MoveSpeedDucked, MoveSpeed );
		else
			moveSpeed = MoveSpeed;

		// And runnin' runnin'.
		if ( ShouldSprint() )
			moveSpeed = GetSprintSpeed( moveSpeed );

		return moveSpeed;
	}
}
