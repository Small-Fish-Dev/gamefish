using System.Diagnostics;
using System.Text.Json.Serialization;
using GameFish;
using ShrimpleCharacterController;

namespace Fishbox;

partial class FishboxController
{
	[Property]
	[Feature( PLAYER ), Order( WALLRUNNING_ORDER )]
	[ToggleGroup( nameof( AllowWallRunning ), Label = WALLRUNNING )]
	public bool AllowWallRunning { get; set; } = true;

	/// <summary>
	/// Multiply gravity by this amount while wall running.
	/// </summary>
	[Property]
	[Title( "Gravity" )]
	[Feature( PLAYER ), Order( WALLRUNNING_ORDER )]
	[ToggleGroup( nameof( AllowWallRunning ), Label = WALLRUNNING )]
	public float WallRunGravity { get; set; } = 0.9f;

	[Property]
	[Title( "Jump Speed" )]
	[Feature( PLAYER ), Order( WALLRUNNING_ORDER )]
	[ToggleGroup( nameof( AllowWallRunning ), Label = WALLRUNNING )]
	public Vector2 WallRunJumpSpeed { get; set; } = new( 600f, 200f );

	/// <summary>
	/// What are the angles we can start a wall run? <br />
	/// <c>90</c> degrees = straight wall
	/// </summary>
	[Property]
	[Feature( PLAYER )]
	[Title( "Wall Pitch" )]
	[Range( 0f, 180f, clamped: false )]
	[ToggleGroup( nameof( AllowWallRunning ), Label = WALLRUNNING )]
	public FloatRange WallRunPitchRange { get; set; } = new( 70f, 110f );

	/// <summary>
	/// The distance a wall we can be to stay stuck
	/// to it, otherwise we'll stop running.
	/// </summary>
	[Property]
	[Title( "Stick Distance" )]
	[Range( 1f, 64f, clamped: false )]
	[Feature( PLAYER ), Order( WALLRUNNING_ORDER )]
	[ToggleGroup( nameof( AllowWallRunning ), Label = WALLRUNNING )]
	public float WallRunStickDistance { get; set; } = 20f;

	[Feature( PLAYER )]
	[Title( "Is Wall Running" )]
	[Property, ReadOnly, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[ToggleGroup( nameof( AllowWallRunning ), Label = WALLRUNNING )]
	protected bool InspectorIsWallRunning
	{
		get => IsWallRunning;
		set => IsWallRunning = value;
	}

	[Sync]
	public bool IsWallRunning
	{
		get => _isWallRunning;
		set
		{
			_isWallRunning = value;
			OnSetIsWallRunning( in _isWallRunning );
		}
	}

	protected bool _isWallRunning;

	[Sync]
	[Normal]
	public Vector3 WallRunNormal
	{
		get => _wallRunNormal;
		set
		{
			_wallRunNormal = value.Normal;
			OnSetWallRunNormal( in _wallRunNormal );
		}
	}

	protected Vector3 _wallRunNormal;

	protected virtual void OnSetIsWallRunning( in bool isEnabled )
	{
		if ( IsProxy )
			return;

		if ( isEnabled )
			IsSlipping = false;
	}

	protected virtual void OnSetWallRunNormal( in Vector3 wallNormal )
	{
	}

	protected virtual void StopWallRunning()
	{
		if ( !IsWallRunning )
			return;

		IsWallRunning = false;
		WallRunNormal = default;
	}

	protected virtual bool TryStartWallRunning( in TraceResult trWall )
	{
		if ( IsWallRunning )
			return false;

		if ( !trWall.Hit )
			return false;

		// this.Log( $"Started wall running. Hit object:[{trWall.GameObject}]" );

		IsWallRunning = true;
		WallRunNormal = trWall.Normal;

		return true;
	}

	protected virtual bool IsValidForWallRunning( in TraceResult tr )
	{
		if ( !tr.Hit )
			return false;

		if ( !IsValidWallAngle( tr.Normal, WishVelocity.Normal ) )
			return false;

		if ( IsSlippery( tr ) )
			return false;

		return true;
	}

	public virtual bool IsValidWallAngle( in Vector3 wallNormal, in Vector3 moveDir )
	{
		if ( wallNormal.AlmostEqual( 0f ) )
			return false;

		var pitch = Up.Angle( wallNormal );

		if ( !WallRunPitchRange.Within( in pitch ) )
			return false;

		return true;
	}

	protected virtual void DoWallRunJump()
	{
		if ( !IsWallRunning )
			return;

		IsWallRunning = false;

		var upWall = Vector3.VectorPlaneProject( Up, WallRunNormal );

		Velocity.Separate( upWall, out var upVel, out var hVel );

		// Vertical Wall Jumping
		var upSpeed = upVel.Dot( upWall ).Positive();
		upSpeed += WallRunJumpSpeed.y;
		upVel = upWall * upSpeed;

		// Horizontal Wall Jumping
		hVel = hVel.ProjectAndScale( WallRunNormal );

		var wishVel = WishVelocity;
		Vector3 hJumpDir;

		if ( wishVel.AlmostEqual( 0f ) )
		{
			// Jump where we're aiming.
			hJumpDir = EyeForward.Horizontal( upWall );
		}
		else
		{
			// Jump where we're inputting movement towards.
			var moveDir = wishVel.Normal.Horizontal( upWall );
			hJumpDir = moveDir;
		}

		const float minDot = 0.4f;

		// Horizontal Jump
		hVel += hJumpDir.Normal * WallRunJumpSpeed.x;

		// Bounce the direction off if it would be into the wall.
		var hVelDot = hVel.Normal.Dot( WallRunNormal );

		if ( hVelDot < 0f )
		{
			hVel = Vector3.Reflect( hVel, WallRunNormal ).Horizontal( upWall );
			hVelDot = hVel.Normal.Dot( WallRunNormal );
		}

		// Prevent jumping too close into the wall.
		if ( hVelDot < minDot )
		{
			hVel = hVel.ProjectAndScale( WallRunNormal );
			hVel = hVel.Normal.LerpTo( WallRunNormal, minDot ) * hVel.Length;
		}

		Velocity = upVel + hVel;
	}

	protected virtual void DoWallRunning( in float deltaTime )
	{
		if ( IsGrounded )
		{
			if ( IsWallRunning )
			{
				// this.Log( "Stopped wall running. Reason: \"Grounded.\"" );
				StopWallRunning();
			}

			return;
		}

		if ( !IsWallRunning )
		{
			if ( !HoldingJump )
				return;

			// Where are we trying to move to?
			Velocity.Separate( Up, out var upVel, out var hVel );

			upVel = Up * upVel.Dot( Up ).Positive();

			if ( hVel.AlmostEqual( 0f ) )
				hVel = WishVelocity.Horizontal( Up );

			var velMove = upVel + hVel;

			if ( velMove.AlmostEqual( 0f ) )
				return;

			// Not wall running and trying to run.
			var traceDist = (velMove.Length * deltaTime).Max( 1f );
			var vDelta = velMove.Normal * traceDist;

			var trMove = TraceColliders( WorldPosition, vDelta );

			// DebugOverlay.Trace( trMove );

			if ( !trMove.StartedSolid && trMove.Hit )
				if ( IsValidForWallRunning( trMove ) )
					TryStartWallRunning( in trMove );

			// Debug Trace Visualizer
			// DebugOverlay.Trace( trMove );
		}

		if ( !IsWallRunning )
			return;

		// Wall normal sanity check.
		if ( WallRunNormal.AlmostEqual( 0f ) )
		{
			this.Log( "Stopped wall running. Reason: \"Weird/no wall.\"" );
			StopWallRunning();
			return;
		}

		// Stick to the current wall.
		var trStick = TraceColliders( WorldPosition, -WallRunNormal * WallRunStickDistance );

		// DebugOverlay.Trace( trStick );

		if ( !trStick.Hit || !IsValidForWallRunning( trStick )
			|| trStick.Normal.Angle( WallRunNormal ) > 45f )
		{
			StopWallRunning();
			return;
		}

		WallRunNormal = trStick.Normal;
		TryStickToSurface( trStick );

		// Run onto the next wall.
		var deltaAhead = Velocity * deltaTime.Min( 1f ) * 1.5f;
		var trWall = TraceColliders( WorldPosition, deltaAhead );

		if ( IsValidForWallRunning( trWall ) )
		{
			WallRunNormal = trWall.Normal;
			TryStickToSurface( in trWall );
		}

		// Negate into-wall velocity.
		Velocity = Velocity.Horizontal( WallRunNormal );
	}
}
