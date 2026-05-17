using System;
using GameFish;

namespace Fishbox;

partial class ShooterController
{
	/// <summary>
	/// The maximum angle from the direction of gravity that it's a ceiling.
	/// </summary>
	[Property]
	[Order( NINJA_ORDER )]
	[Title( "Ceiling Angle" )]
	[Feature( NINJA ), Group( WALL )]
	[Range( 20f, 40f, clamped: false )]
	public virtual float CeilingAngle { get; set; } = 30f;

	/// <summary>
	/// Walls must be this angle away from a perfectly upright wall.
	/// </summary>
	[Property]
	[Title( "Lean" )]
	[Order( NINJA_ORDER )]
	[Feature( NINJA ), Group( WALL )]
	public virtual Fraction WallRunLean { get; set; } = 0.2f;

	[Property]
	[Order( NINJA_ORDER )]
	[Title( "Move Speed" )]
	[Feature( NINJA ), Group( WALL )]
	[Range( 0f, 250f, clamped: false )]
	public virtual float WallRunMoveSpeed { get; set; } = 100f;

	/// <summary>
	/// The limit for the angle of jumps away from the wall's face.
	/// </summary>
	[Property]
	[Order( NINJA_ORDER )]
	[Title( "Jump Angle" )]
	[Feature( NINJA ), Group( WALL )]
	[Range( 25f, 90f, clamped: false )]
	public virtual float WallRunJumpAngle { get; set; } = 65f;

	/// <summary>
	/// <c>Min</c> = horizontal speed <br />
	/// <c>Max</c> = vertical speed
	/// </summary>
	[Property]
	[Order( NINJA_ORDER )]
	[Title( "Jump Speed" )]
	[Feature( NINJA ), Group( WALL )]
	[Range( 25f, 90f, clamped: false )]
	public virtual FloatRange WallRunJumpSpeed { get; set; } = new( 500f, 350f );

	/// <summary>
	/// The maximum distance a nearby wall can be while riding.
	/// </summary>
	[Property]
	[Order( NINJA_ORDER )]
	[Title( "Stick Distance" )]
	[Feature( NINJA ), Group( WALL )]
	[Range( 8f, 64f, clamped: false )]
	public virtual float WallRunStickDistance { get; set; } = 32f;

	/// <summary>
	/// Tries to find/maintain a wall to run along.
	/// </summary>
	protected virtual void UpdateWallRunMount( in float deltaTime )
	{
		// If we failed to stick then stop wall running.
		if ( !TryStick( -SurfaceNormal, WallRunStickDistance, out _ ) )
		{
			StopParkour();
			return;
		}

		// Always riding walls if not holding the button.
		if ( IsWishingParkour() && IsLookingIntoWall( SurfaceNormal ) )
		{
			// Always sticking if it's a ceiling.
			ParkourState = ParkourType.Sticky;
		}
		else if ( !IsGrounded )
		{
			ParkourState = ParkourType.Riding;
		}
		else
		{
			ParkourState = ParkourType.None;
		}
	}

	protected virtual void DoWallRunJump( in Vector3 normal )
	{
		var upDir = Up;
		var jumpDir = EyeForward.PlaneProject( upDir ).Normal;

		if ( jumpDir.Dot( normal ) <= 0f )
			jumpDir = jumpDir.Reflect( in normal );

		var wallAngle = jumpDir.Angle( normal );

		if ( wallAngle > WallRunJumpAngle )
		{
			var sideDir = jumpDir.PlaneProject( normal );
			var frac = WallRunJumpAngle.Remap( 0f, 90, 0f, 1f );

			jumpDir = normal.SlerpTo( sideDir, frac );
		}

		Velocity.Separate( in upDir, out var upVel, out var hVel );

		var hJumpSpeed = WallRunJumpSpeed.Min;
		var minSpeed = hJumpSpeed * normal.Dot( jumpDir.Normal ).Abs();
		var hSpeed = hVel.Length.Max( in minSpeed );

		hVel = jumpDir * hSpeed.Max( hJumpSpeed );

		upVel *= upDir.Dot( in upVel ).Sign().Positive();
		upVel += upDir * WallRunJumpSpeed.Max;

		OnPreJump();

		RechargeAirJumps( count: 1 );

		Velocity = hVel + upVel;
	}

	public virtual bool IsWallRunnable( in SceneTraceResult tr )
	{
		if ( !tr.Hit )
			return false;

		var normal = tr.Normal;

		if ( normal == default )
			return false;

		if ( IsGround( in normal ) )
			return false;

		if ( IsCeiling( in normal ) )
			return false;

		return true;
	}

	protected virtual void UpdateWallRunning( in float deltaTime )
	{
		UpdateWallRunMount( in deltaTime );

		if ( IsWallRunning() )
			UpdateWallRunVelocity( SurfaceNormal, in deltaTime );
	}

	/// <summary>
	/// Redirects velocity along the wall.
	/// </summary>
	protected virtual void UpdateWallRunVelocity( in Vector3 normal, in float deltaTime )
	{
		if ( normal == default )
			return;

		var upDir = Up;

		if ( !IsNinjaRunning )
			goto Side;

		var aimDir = EyeForward;

		if ( IsCeiling( normal ) || IsLookingIntoWall( in normal ) )
		{
			Velocity = Velocity.PlaneProject( in normal, Velocity.Length );

			var vel = Velocity;
			var ninjaSpeed = 2000f;
			var runSpeed = vel.Length.Max( 100f );

			var aimFlat = aimDir.PlaneProject( in normal ).Normal;

			vel += aimFlat * ninjaSpeed * deltaTime;

			Velocity = vel.PlaneProject( in normal, vel.Length.Min( runSpeed ) );

			return;
		}

		Side:

		Velocity.Separate( in upDir, out var upVel, out var hVel );

		hVel = hVel.PlaneProject( in normal ).PlaneProject( in upDir, hVel.Length );
		upVel = upVel.PlaneProject( in normal );

		Velocity = (hVel + upVel).Horizontal( in normal );
	}
}
