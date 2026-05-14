using GameFish;

namespace Fishbox;

partial class ShooterController
{
	/// <summary>
	/// Walls must be this angle away from a perfectly upright wall.
	/// </summary>
	[Property]
	[Title( "Lean" )]
	[Order( BADASS_ORDER )]
	[Feature( BADASS ), Group( WALLRUNNING )]
	public virtual Fraction WallRunLean { get; set; } = 0.2f;

	[Property]
	[Order( BADASS_ORDER )]
	[Title( "Move Speed" )]
	[Range( 0f, 250f, clamped: false )]
	[Feature( BADASS ), Group( WALLRUNNING )]
	public virtual float WallRunMoveSpeed { get; set; } = 100f;

	/// <summary>
	/// Walls must be this angle away from a perfectly upright wall.
	/// </summary>
	[Property]
	[Order( BADASS_ORDER )]
	[Title( "Wall Angle" )]
	[Range( 20f, 40f, clamped: false )]
	[Feature( BADASS ), Group( WALLRUNNING )]
	public virtual float WallRunAngle { get; set; } = 35f;

	/// <summary>
	/// The limit for the angle of jumps away from the wall's face.
	/// </summary>
	[Property]
	[Order( BADASS_ORDER )]
	[Title( "Jump Angle" )]
	[Range( 25f, 90f, clamped: false )]
	[Feature( BADASS ), Group( WALLRUNNING )]
	public virtual float WallRunJumpAngle { get; set; } = 65f;

	/// <summary>
	/// <c>Min</c> = horizontal speed <br />
	/// <c>Max</c> = vertical speed
	/// </summary>
	[Property]
	[Order( BADASS_ORDER )]
	[Title( "Jump Speed" )]
	[Range( 25f, 90f, clamped: false )]
	[Feature( BADASS ), Group( WALLRUNNING )]
	public virtual FloatRange WallRunJumpSpeed { get; set; } = new( 500f, 350f );

	/// <summary>
	/// The maximum distance a nearby wall can be while riding.
	/// </summary>
	[Property]
	[Order( BADASS_ORDER )]
	[Title( "Stick Distance" )]
	[Range( 8f, 64f, clamped: false )]
	[Feature( BADASS ), Group( WALLRUNNING )]
	public virtual float WallRunStickDistance { get; set; } = 32f;

	[Sync]
	public Vector3? WallRunNormal
	{
		get => _wallRunNormal;
		set
		{
			if ( _wallRunNormal == value )
				return;

			var oldNormal = _wallRunNormal;
			_wallRunNormal = value;

			OnWallRunNormalSet( in _wallRunNormal, in oldNormal );
		}
	}

	protected Vector3? _wallRunNormal;

	protected virtual void OnWallRunNormalSet( in Vector3? newNormal, in Vector3? oldNormal )
	{
		if ( newNormal is Vector3 normal )
		{
			// if ( !oldNormal.HasValue )
			// this.Log( $"Wall run started on normal:[{normal}]" );

			UpdateWallRunVelocity( in normal );
		}
	}

	public virtual bool IsWishingWallRun()
		=> IsAlive && JumpInput.IsHeld;

	public virtual bool IsWallRunningAllowed()
	{
		if ( IsGrounded )
			return false;

		if ( IsStuck )
			return false;

		return true;
	}

	public virtual bool ShouldWallRun()
	{
		if ( !IsWallRunningAllowed() )
			return false;

		return IsWishingWallRun();
	}

	protected virtual void UpdateWallRunView( in Vector3 normal, in float deltaTime )
	{
		var upDir = GetWallRunUp();
		upDir = upDir.SlerpTo( in normal, WallRunLean );

		var speed = AimRollResetSpeed * 2f;

		ResetEyeRotation( EyeForward, in upDir, in speed, in deltaTime );
	}

	protected virtual void DoWallRunJump( in Vector3 normal )
	{
		var upDir = GetWallRunUp();
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

		Velocity = hVel + upVel;
	}

	public virtual void StopWallRunning()
	{
		if ( WallRunNormal is null )
			return;

		WallRunNormal = null;
	}

	protected virtual Vector3 GetWallRunUp()
	{
		var upDir = -Gravity.Normal;

		if ( upDir == default )
			upDir = Up;

		return upDir;
	}

	public virtual bool IsWallRunnable( in SceneTraceResult tr )
	{
		if ( !tr.Hit )
			return false;

		var hitNormal = tr.Normal;

		if ( hitNormal == default )
			return false;

		if ( IsGround( in tr ) || IsGround( -hitNormal ) )
			return false;

		var upDir = GetWallRunUp();
		var wallAngle = upDir.Angle( in hitNormal ) - 90f;

		return wallAngle <= WallRunAngle;
	}

	/// <summary>
	/// Are we truly wall running actively(such as also being alive)? And if so on what surface.
	/// </summary>
	/// <param name="normal"> The surface direction we're running along(or default). </param>
	/// <returns> If we're supposed to be running on a wall. </returns>
	public virtual bool IsWallRunning( out Vector3 normal )
	{
		normal = default;

		if ( !IsAlive )
			return false;

		normal = WallRunNormal ?? default;

		return normal != default;
	}

	protected virtual void UpdateWallRunning( in float deltaTime )
	{
		if ( !IsWallRunningAllowed() )
		{
			StopWallRunning();
			return;
		}

		UpdateWallRunMount( in deltaTime );

		if ( IsWallRunning( out var normal ) )
			UpdateWallRunVelocity( in normal );
	}

	/// <summary>
	/// Redirects velocity along the wall.
	/// </summary>
	protected virtual void UpdateWallRunVelocity( in Vector3 normal )
	{
		Velocity = Velocity.Horizontal( in normal );
	}

	/// <summary>
	/// Tries to find/maintain a wall to run along.
	/// </summary>
	protected virtual void UpdateWallRunMount( in float deltaTime )
	{
		// Continuously stick to walls.
		if ( WallRunNormal is Vector3 wallNormal )
		{
			// If we failed to stick then stop wall running.
			if ( !TryWallRun( -wallNormal, WallRunStickDistance, out _ ) )
				StopWallRunning();
		}

		// Try to start a new wall run.
		if ( ShouldWallRun() )
		{
			var vel = Velocity;
			TryWallRun( vel.Normal, vel.Length * deltaTime * 1.5f, out _ );
		}
	}

	/// <summary>
	/// Tests if we'd collide with a runnable wall from here to there.
	/// </summary>
	/// <returns> If the wall was mounted. </returns>
	protected virtual bool TryWallRun( in Vector3 dir, in float dist, out SceneTraceResult trHit )
	{
		var move = Physics.Project( in dir, in dist );
		trHit = move.Hits?.FirstOrDefault() ?? default;

		if ( !move.Hit || move.IsStuck )
			return false;

		if ( !IsWallRunnable( in trHit ) )
			return false;

		WallRunNormal = trHit.Normal;

		MoveTo( move.Position );

		return true;
	}
}
