using GameFish;

namespace Fishbox;

/// <summary>
/// /// A badass player controller. <br />
/// /// Shout out to <b>Cyber-Ninja: Ascension</b>.
/// </summary>
[Icon( "sports_esports" )]
public partial class ShooterController : FirstPersonController
{
	protected const int BADASS_ORDER = PAWN_ORDER - 1000;

	[Property]
	[InputAction]
	[Title( "Input" )]
	[Order( BADASS_ORDER )]
	[Feature( BADASS ), Group( FOCUS )]
	public virtual string FocusInput { get; set; } = "Attack2";

	[Sync]
	public bool IsFocusing { get; protected set; }

	public override void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
		base.Simulate( deltaTime, isFixedUpdate );

		UpdateGravity( in deltaTime );
	}

	protected override void UpdateInput( in float deltaTime )
	{
		base.UpdateInput( deltaTime );

		var isAlive = Pawn?.IsAlive is true;

		IsFocusing = isAlive && Input.Down( FocusInput );
	}

	protected override void ResetInput()
	{
		base.ResetInput();

		IsFocusing = false;
	}
}
