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

	public override bool AimPitchClamping => false;

	public override Vector3 Gravity => Down * base.Gravity.Length;

	public override bool TryAim( in Rotation rLook, in float deltaTime )
	{
		if ( !ITransform.IsValid( in rLook ) )
			return false;

		if ( !IsGrounded )
		{
			LocalEyeRotation *= rLook;
			return true;
		}

		var rAim = LocalEyeRotation;
		var rInverse = rAim.Inverse;

		rAim *= Rotation.FromAxis( rInverse.Up, rLook.Yaw() );
		rAim *= Rotation.FromPitch( rLook.Pitch() );

		LocalEyeRotation = rAim;

		return true;
	}

	protected override void ResetEyeRoll( in float speed, in float deltaTime )
	{
		var tEye = EyeTransform;

		var rollSpeed = IsGrounded ? speed : speed / 5f;
		var newUp = tEye.Up.SlerpTo( Up, rollSpeed * deltaTime );

		EyeRotation = Rotation.LookAt( tEye.Forward, newUp );
	}

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

		TrySetPerspective( in tr );
	}

	protected bool TrySetPerspective( in SceneTraceResult tr )
	{
		if ( !tr.Hit )
			return false;

		var rUp = Rotation.LookAt( tr.Normal, EyeForward );
		var rForward = Rotation.LookAt( rUp.Up, rUp.Forward );

		return TrySetPerspective( in rForward );
	}

	protected bool TrySetPerspective( in Rotation rForward )
	{
		if ( !ITransform.IsValid( in rForward ) )
			return false;

		if ( !Pawn.IsValid() )
			return false;

		var oldCenter = Center;

		var rEye = Pawn.EyeRotation;
		var eyePos = Pawn.EyePosition;

		Pawn.WorldRotation = rForward;

		Pawn.EyeRotation = rEye;
		Pawn.EyePosition = eyePos;

		Pawn.WorldPosition += oldCenter - Center;

		return true;
	}
}
