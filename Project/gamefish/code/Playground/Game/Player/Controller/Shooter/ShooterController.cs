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

		UpdateGravityBoots();
	}

	protected virtual void UpdateGravityBoots()
	{
		if ( !Pawn.IsValid() )
			return;

		if ( !Input.Pressed( "Item" ) )
			return;

		var tr = Pawn.GetEyeTrace( 8096f ).Run();

		if ( !tr.Hit )
			return;

		var oldCenter = Center;

		var rEye = Pawn.EyeRotation;
		var eyePos = Pawn.EyePosition;

		var rUp = Rotation.LookAt( tr.Normal, tr.Direction );
		WorldRotation = Rotation.LookAt( rUp.Up, rUp.Forward );

		Pawn.WorldPosition += oldCenter - Center;

		Pawn.EyeRotation = rEye;
		Pawn.EyePosition = eyePos;
	}
}
