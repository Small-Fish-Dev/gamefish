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

	[Property]
	[Range( 0f, 2f, clamped: false )]
	[Title( "Anti-Roll Speed (free)" )]
	[ToggleGroup( nameof( AllowAiming ) )]
	[Feature( VIEW ), Order( AIMING_ORDER )]
	public virtual float AimRollFreeResetSpeed { get; set; } = 0.3f;

	public override bool AimPitchClamping => false;

	public override Vector3 Gravity => Down * base.Gravity.Length;

	protected bool IsFreeLooking => JumpInput.IsHeld;

	public override bool TryAim( in Rotation rLook, in float deltaTime )
	{
		if ( !ITransform.IsValid( in rLook ) )
			return false;

		if ( IsFreeLooking )
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

	protected override void UpdateEyeRotation( in float deltaTime )
	{
		var rollSpeed = IsFreeLooking
			? AimRollFreeResetSpeed
			: AimRollResetSpeed;

		ResetEyeRoll( in rollSpeed, in deltaTime );
	}

	protected override void ResetEyeRoll( in float speed, in float deltaTime )
	{
		var tEye = EyeTransform;
		var newUp = tEye.Up.SlerpTo( Up, speed * deltaTime );

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

		// Stick to what's beneath us.
		if ( Input.Down( "Run" ) )
		{
			var eyeDown = EyeRotation.Down;
			var tr = Pawn.GetEyeTrace( 1024f, dir: eyeDown ).Run();

			if ( tr.Hit )
				TrySetPerspective( in tr );
			else
				TrySetPerspective( EyeRotation );
		}

		// Stick to where we're looking.
		if ( Input.Pressed( "Item" ) )
		{
			var tr = Pawn.GetEyeTrace( 16384f ).Run();
			TrySetPerspective( in tr );
		}
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

		Pawn.WorldPosition += oldCenter - Center;
		Pawn.EyePosition = eyePos;

		return true;
	}
}
