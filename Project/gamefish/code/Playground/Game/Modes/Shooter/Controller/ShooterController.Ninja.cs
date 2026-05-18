using System.Data;
using System.Text.Json.Serialization;
using GameFish;

namespace Fishbox;

partial class ShooterController
{
	protected const int NINJA_MOVEMENT_ORDER = NINJA_ORDER - 1;

	/// <summary>
	/// The button that enables auto-parkour.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Input" )]
	[Order( NINJA_MOVEMENT_ORDER )]
	[Feature( NINJA ), Group( MOVEMENT )]
	public virtual string ParkourInput { get; set; } = "Run";

	/// <summary>
	/// If true: you can parkour straight up walls and on ceilings.. if that weren't broken currently.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Sticking" )]
	[Order( NINJA_MOVEMENT_ORDER )]
	[Feature( NINJA ), Group( MOVEMENT )]
	public virtual bool StickingEnabled { get; set; } = false;

	[Property]
	[JsonIgnore]
	[Title( "Parkour State" )]
	[Feature( NINJA ), Group( DEBUG )]
	protected ParkourType InspectorParkourState
	{
		get => ParkourState;
		set => ParkourState = value;
	}

	/// <summary>
	/// If true: auto-parkour onto stuff.
	/// </summary>
	[Sync]
	public bool IsNinjaRunning { get; protected set; }

	[Sync]
	public ParkourType ParkourState
	{
		get => _parkourState; set
		{
			if ( _parkourState == value )
				return;

			_parkourState = value;
			OnSetParkour( in value );
		}
	}

	protected ParkourType _parkourState = ParkourType.None;

	/// <summary>
	/// The normal of the surface we're sticking to.
	/// </summary>
	[Sync]
	public Vector3 SurfaceNormal
	{
		get => _surfaceNormal;
		set
		{
			var oldNormal = _surfaceNormal;
			_surfaceNormal = value;

			OnSetSurfaceNormal( in _surfaceNormal, in oldNormal );
		}
	}

	protected Vector3 _surfaceNormal = Vector3.Up;

	protected virtual void OnSetParkour( in ParkourType state )
	{
		this.Log( state );

		if ( state is ParkourType.None )
			ResetOrientation();
	}

	protected virtual void OnSetSurfaceNormal( in Vector3 normal, in Vector3 old )
	{
		// this.Log( $"Wall run started on normal:[{normal}]" );
		UpdateWallRunVelocity( in normal, deltaTime: 0f );
	}

	public virtual void StopParkour()
	{
		SurfaceNormal = default;
		ParkourState = ParkourType.None;
	}

	public virtual bool IsParkourAllowed()
	{
		if ( !IsAlive )
			return false;

		if ( IsStuck )
			return false;

		return true;
	}

	protected virtual bool IsWishingParkour()
		=> Input.Down( ParkourInput );

	protected virtual bool ShouldParkour()
	{
		if ( !IsParkourAllowed() )
			return false;

		return IsWishingParkour();
	}

	/// <summary>
	/// Are we truly wall running actively(such as also being alive)?
	/// </summary>
	/// <returns> If we're supposed to be running on a wall. </returns>
	public virtual bool IsWallRunning()
	{
		if ( !IsParkourAllowed() )
			return false;

		if ( SurfaceNormal == default )
			return false;

		return ParkourState is ParkourType.Riding or ParkourType.Sticking;
	}

	protected virtual void UpdateParkour( in float deltaTime )
	{
		if ( !IsParkourAllowed() )
		{
			StopParkour();
			return;
		}

		switch ( ParkourState )
		{
			case ParkourType.None:

				if ( !IsWishingParkour() )
					break;

				// Try to start a new wall run.
				if ( TryStick( Velocity, deltaTime * 1.5f, out var trHit ) )
					UpdateWallRunMount( in deltaTime );

				break;

			case ParkourType.Riding:
			case ParkourType.Sticking:
				UpdateWallRunning( in deltaTime );
				break;
		}
	}

	/// <summary>
	/// Attempts to stick to a wall in that direction.
	/// </summary>
	/// <returns> If the wall was stuck to. </returns>
	protected virtual bool TryStick( in Vector3 dir, in float dist, out SceneTraceResult trHit )
	{
		var move = Physics.Project( in dir, in dist );
		trHit = move.Hits?.FirstOrDefault() ?? default;

		if ( !move.Hit || move.IsStuck )
			return false;

		if ( !IsWallRunnable( in trHit ) )
			return false;

		SurfaceNormal = trHit.Normal;

		MoveTo( move.Position );

		return true;
	}

	public virtual bool IsCeiling( in Vector3 normal )
	{
		if ( normal == default )
			return false;

		var angle = Down.Angle( in normal );
		return angle <= (CeilingAngle / 2f);
	}

	public virtual bool IsLookingAtWall( in Vector3 normal, in float? maxAngle = null )
	{
		var upDir = DefaultUp;

		if ( upDir == default )
			return false;

		var hAimDir = EyeForward.PlaneProject( in upDir ).Normal;
		var hWallDir = SurfaceNormal.PlaneProject( in upDir ).Normal;

		if ( hAimDir == default || hWallDir == default )
			return false;

		var yaw = hAimDir.Angle( -hWallDir );

		return yaw <= (maxAngle ?? 10f);
	}

	public virtual bool IsWallStickable( in Vector3 normal )
	{
		// TEMP: Sticky until you let go.
		if ( ParkourState is ParkourType.Sticking )
			return true;

		if ( IsGround( in normal ) )
			return false;

		if ( IsCeiling( in normal ) )
			return true;

		return IsLookingAtWall( in normal, 10f );
	}
}
