using GameFish;

namespace Fishbox;

/// <summary>
/// A badass player controller. <br />
/// Shout out to <b>Cyber-Ninja: Ascension</b>.
/// </summary>
[Icon( "sports_esports" )]
public partial class ShooterController : FirstPersonController
{
	protected const string BADASS = "😎 Badass";
	protected const int BADASS_ORDER = PAWN_ORDER - 1000;

	public override Vector3 Gravity => Down * base.Gravity.Length;

	public override void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
		base.Simulate( deltaTime, isFixedUpdate );

		if ( ShouldJump() )
			Jump();
	}

	protected override void PostMove( in float deltaTime )
	{
		base.PostMove( deltaTime );

		// Gravity.
		if ( !IsGrounded )
			Velocity += Gravity * deltaTime;
	}

	protected override void Move( in float deltaTime )
	{
		PreMove( in deltaTime );

		Physics?.Simulate( in deltaTime );

		PostMove( in deltaTime );
	}
}
