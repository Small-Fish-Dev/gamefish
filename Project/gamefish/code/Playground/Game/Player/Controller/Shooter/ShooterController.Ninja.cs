using System.Text.Json.Serialization;
using GameFish;

namespace Fishbox;

partial class ShooterController
{
	/// <summary>
	/// The button that enables wall/ceiling running.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Input" )]
	[Order( BADASS_ORDER )]
	[Feature( NINJA ), Group( MOVEMENT )]
	public virtual string ParkourInput { get; set; } = "Run";

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

		return ParkourState is ParkourType.WallRiding or ParkourType.Sticky;
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

				// Try to start a new wall run.
				if ( !IsWishingParkour() )
					break;

				if ( TryStick( Velocity, deltaTime * 1.5f, out var trHit ) )
				{
					SurfaceNormal = trHit.Normal;
					UpdateWallRunMount( in deltaTime );
				}

				break;

			case ParkourType.WallRiding:
			case ParkourType.Sticky:
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
}
