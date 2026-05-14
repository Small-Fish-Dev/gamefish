using GameFish;

namespace Fishbox;

/// <summary>
/// A badass player controller. <br />
/// Shout out to <b>Cyber-Ninja: Ascension</b>.
/// </summary>
[Icon( "sports_esports" )]
public partial class ShooterController : FirstPersonController
{
	protected const int BADASS_ORDER = PAWN_ORDER - 1000;

	public override void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
		base.Simulate( deltaTime, isFixedUpdate );

		UpdateGravity( in deltaTime );
	}

	protected override void UpdateInput( in float deltaTime )
	{
		base.UpdateInput( deltaTime );

		IsFocusing = IsAlive && Input.Down( FocusInput );
	}

	protected override void ResetInput()
	{
		base.ResetInput();

		IsFocusing = false;
	}
}
