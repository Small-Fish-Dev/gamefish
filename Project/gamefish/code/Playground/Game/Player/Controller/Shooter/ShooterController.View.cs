using GameFish;

namespace Fishbox;

partial class ShooterController
{
	public override bool AimPitchClamping => false;

	protected bool IsFreeLooking => IsFocusing && !IsGrounded;

	// [Sync]
	// public Rotation? TargetRotation { get; protected set; }

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
		// Wall run leaning.
		if ( IsWallRunning( out var normal ) )
		{
			UpdateWallRunView( in normal, in deltaTime );
			return;
		}

		// Don't interrupt that John Woo action flow.
		if ( IsFreeLooking )
			return;

		ResetEyeRoll( AimRollResetSpeed, in deltaTime );
	}

	protected bool TrySetPerspective( in SceneTraceResult tr )
	{
		if ( !tr.Hit || tr.StartedSolid )
			return false;

		return TrySetPerspective( tr.Normal );
	}

	protected bool TrySetPerspective( in Vector3 up )
	{
		var rUp = Rotation.LookAt( up, EyeForward );
		var rForward = Rotation.LookAt( rUp.Up, rUp.Forward );

		return TrySetPerspective( in rForward );
	}

	protected bool TrySetPerspective( in Rotation rForward )
	{
		if ( !ITransform.IsValid( in rForward ) )
			return false;

		// TargetRotation = rForward;
		Reorient( rForward );

		return true;
	}

	protected void Reorient( in Rotation rForward )
	{
		if ( !Pawn.IsValid() )
			return;

		var oldCenter = Center;

		var rEye = Pawn.EyeRotation;
		var eyePos = Pawn.EyePosition;

		Pawn.WorldRotation = rForward;
		Pawn.EyeRotation = rEye;

		Pawn.WorldPosition += oldCenter - Center;
		Pawn.EyePosition = eyePos;
	}
}
