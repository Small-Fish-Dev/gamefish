using GameFish;

namespace Fishbox;

partial class ShooterController
{
	public override bool AimPitchClamping => false;

	protected virtual bool IsFreeLooking => IsFocusing && !IsGrounded;

	public virtual Vector3 DefaultUp => Gravity == default ? Vector3.Up : -Gravity.Normal;

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
		if ( IsWallRunning() )
		{
			UpdateWallRunView( SurfaceNormal, in deltaTime );
			return;
		}

		// Don't interrupt that John Woo action flow.
		if ( IsFreeLooking )
			return;

		ResetEyeRoll( AimRollResetSpeed, in deltaTime );
	}

	protected bool TryReorient( in SceneTraceResult tr )
	{
		if ( !tr.Hit || tr.StartedSolid )
			return false;

		return TryReorient( tr.Normal );
	}

	public bool TryReorient( in Vector3 up )
	{
		var rUp = Rotation.LookAt( up, EyeForward );
		var rForward = Rotation.LookAt( rUp.Up, rUp.Forward );

		return TryReorient( in rForward );
	}

	protected bool TryReorient( in Rotation rForward )
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

	protected void ResetOrientation()
	{
		var fwd = EyeForward;
		var up = DefaultUp;

		if ( up == default )
			up = Vector3.Up;

		if ( Up == up )
			return;

		var rUp = Rotation.LookAt( up, fwd );

		var rForward = Rotation.LookAt( rUp.Up, rUp.Forward );

		Reorient( in rForward );
	}

	protected virtual void UpdateWallRunView( in Vector3 normal, in float deltaTime )
	{
		var upDir = DefaultUp;

		if ( ParkourState is ParkourType.Sticking )
		{
			upDir = normal;

			if ( Up != upDir )
				TryReorient( in normal );
		}
		else if ( ParkourState is ParkourType.Riding )
		{
			if ( Up != upDir )
				TryReorient( in upDir );

			upDir = upDir.SlerpTo( in normal, WallRunLean );
		}

		var speed = AimRollResetSpeed * 2f;

		ResetEyeRotation( EyeForward, in upDir, in speed, in deltaTime );
	}
}
