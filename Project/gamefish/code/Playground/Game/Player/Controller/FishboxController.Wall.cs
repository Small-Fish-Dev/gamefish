using System.Text.Json.Serialization;
using GameFish;
using ShrimpleCharacterController;
using SCC = ShrimpleCharacterController.ShrimpleCharacterController;

namespace Playground;

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
	public float WallRunStickDistance { get; set; } = 24f;

	/// <summary>
	/// The distance to stay away from a wall while running on it.
	/// </summary>
	[Property]
	[Title( "Wall Skin" )]
	[Range( 0.5f, 16f, clamped: false )]
	[Feature( PLAYER ), Order( WALLRUNNING_ORDER )]
	[ToggleGroup( nameof( AllowWallRunning ), Label = WALLRUNNING )]
	public float WallRunStickSkin { get; set; } = 3f;

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

	/// <summary>
	/// What's up?
	/// </summary>
	public virtual Vector3 Up => WorldRotation.Up;

	protected virtual void OnSetIsWallRunning( in bool isEnabled )
	{
		if ( IsProxy )
			return;

		if ( isEnabled )
			IsSlipping = false;
	}

	protected virtual void OnSetWallRunNormal( in Vector3 wallNormal )
	{
		if ( IsProxy )
			return;

		if ( IsWallRunning )
			Velocity = Velocity.ProjectAndScale( WallRunNormal );
	}

	protected virtual void StopWallRunning()
	{
		if ( !IsWallRunning )
			return;

		IsWallRunning = false;
		WallRunNormal = default;
	}

	protected virtual void StartWallRunning( in SceneTraceResult trWall )
	{
		if ( IsWallRunning )
			return;

		// this.Log( $"Started wall running. Hit object:[{trWall.GameObject}]" );

		IsWallRunning = true;
		WallRunNormal = trWall.Normal;
	}

	public virtual bool IsValidForWallRunning( in SceneTraceResult tr )
	{
		if ( !tr.Hit )
			return false;

		if ( !IsValidWallAngle( in tr.Normal, WishVelocity.Normal ) )
			return false;

		if ( IsSlippery( tr ) )
			return false;

		return true;
	}

	public virtual bool IsValidWallAngle( in Vector3 wallNormal, in Vector3 moveDir )
	{
		var pitch = Up.Angle( wallNormal );

		if ( !WallRunPitchRange.Within( in pitch ) )
			return false;

		return true;
	}

	protected virtual void StickToWall( in SceneTraceResult trWall )
	{
		if ( trWall.StartedSolid || !trWall.Hit )
			return;

		if ( trWall.Normal.AlmostEqual( 0f ) || !ITransform.IsValid( trWall.Normal ) )
			return;

		WallRunNormal = trWall.Normal;

		// The actual distance from the wall.
		var wallDist = trWall.Distance;

		// The distance to actually move.
		var moveDist = wallDist - SkinWidth.Max( WallRunStickSkin );

		if ( moveDist.AlmostEqual( 0f ) )
			return;

		// Are we moving away from the wall?
		var moveDelta = trWall.Direction * moveDist;

		if ( moveDist < 0f )
		{
			var trSkin = TraceDelta( moveDelta ).Run();

			if ( trSkin.StartedSolid )
				return;

			// What distance should we move backwards?
			var backDist = trSkin.Distance - SkinWidth;

			if ( backDist <= 0.01f )
				return;

			moveDelta = trSkin.Direction * backDist;
		}

		WorldPosition += moveDelta;
	}

	protected virtual void DoWallRunJump()
	{
		if ( !IsWallRunning )
			return;

		IsWallRunning = false;

		if ( !ShrimpleController.IsValid() )
			return;

		var upWall = Vector3.VectorPlaneProject( Up, WallRunNormal );

		_c.Velocity.Separate( upWall, out var upVel, out var hVel );

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

		_c.Velocity = upVel + hVel;
	}

	protected virtual void DoWallRunning( in float deltaTime )
	{
		if ( !ShrimpleController.IsValid() )
			return;

		if ( _c.IsOnGround )
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
			var traceVector = velMove.Normal * traceDist;

			var trMove = Trace( WorldPosition, WorldPosition + traceVector, TraceOffset ).Run();

			if ( !trMove.StartedSolid && trMove.Hit )
				if ( IsValidForWallRunning( trMove ) )
					StartWallRunning( in trMove );

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
		var trStick = TraceDelta( WallRunNormal * -WallRunStickDistance, TraceOffset ).Run();

		if ( !trStick.Hit || !IsValidForWallRunning( trStick )
			|| trStick.Normal.Angle( WallRunNormal ) > 45f )
		{
			StopWallRunning();
			return;
		}

		StickToWall( trStick );

		// Run onto the next wall.
		var deltaAhead = Velocity * deltaTime.Min( 1f ) * 1.5f;
		var trWall = TraceDelta( deltaAhead, TraceOffset ).Run();

		if ( IsValidForWallRunning( trWall ) )
			StickToWall( in trWall );
	}
}
