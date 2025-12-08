namespace GameFish;

public partial class FreeCamViewMode : ViewMode
{
	/*
	/// <summary>
	/// Radius of the sphere collider that will prevent the camera from fazing through walls.
	/// Set to <c>null</c> to disable the camera collisions.  
	/// </summary>
	[Property]
	[Feature( MODES )]
	[Group( FREE_CAM ), Order( FREE_CAM_ORDER )]
	public float? FreeRoamColliderRadius { get; set; } = 16f;

	/// <summary>
	/// Tags of objects that will obstruct the camera view.
	/// </summary>
	[Property]
	[Feature( MODES )]
	[ShowIf( nameof( HasFreeCamMode ), true )]
	[Group( FREE_CAM ), Order( FREE_CAM_ORDER )]
	public TagSet FreeRoamColliderTags { get; set; } = new();

	/// <summary>
	/// Sticks the player to ground, until they aim their camera at the specified angle away from surface.
	/// </summary>
	[Property]
	[Feature( MODES )]
	[ShowIf( nameof( HasFreeCamMode ), true )]
	[Group( FREE_CAM ), Order( FREE_CAM_ORDER )]
	[Range( 0, 90, clamped: true )]
	public float? StickToGroundAngle { get; set; } = 30f;

	/// <summary>
	/// Distance (minus the radius) to ground, at which the player will be locked until they press Space or
	/// aim at an angle of unsticking from the ground (see <see cref="StickToGroundAngle"/>).
	/// <br/>
	/// Ideally should be set to the eye level of a player.
	/// </summary>
	[Property]
	[Feature( MODES )]
	[ShowIf( nameof( HasFreeCamMode ), true )]
	[Group( FREE_CAM ), Order( FREE_CAM_ORDER )]
	[Range( 0, 200, clamped: false )]
	[HideIf( nameof( StickToGroundAngle ), null )]
	public float StickToGroundDistance { get; set; } = 30f;

	/// <summary>
	/// Should the free roam controller limit itself to the <see cref="Bounds"/>.
	/// </summary>
	[Property]
	[Feature( MODES )]
	[ShowIf( nameof( HasFreeCamMode ), true )]
	[Group( FREE_CAM ), Order( FREE_CAM_ORDER )]
	public bool HasBounds
	{
		get => _hasBounds && HasFreeCamMode;
		set => _hasBounds = value;
	}

	protected bool _hasBounds = false;

	/// <summary>
	/// Hard bounds for the free roam mode. Uses global transform.
	/// </summary>
	[Property]
	[Feature( MODES )]
	[ShowIf( nameof( HasBounds ), true )]
	[Group( FREE_CAM ), Order( FREE_CAM_ORDER )]
	public BBox Bounds { get; set; }
	*/

	protected virtual void OnFreeCamModeSet()
	{
	}

	protected virtual void SetFreeCamModeTransform()
	{
	}

	protected virtual void OnFreeCamModeUpdate( in float deltaTime )
	{
		var pawn = TargetPawn;

		if ( !pawn.IsValid() )
			return;

		/*var angles = new Angles( WorldRotation * Input.AnalogLook ) with { roll = 0.0f };

		if ( PitchClamp > 0f )
			angles.pitch = angles.pitch.Clamp( -PitchClamp, PitchClamp );

		CurrentAngles = angles;
		var rotation = CurrentAngles.ToRotation();

		var newPosition = WorldPosition;

		var speed = SprintButton != null && Input.Down( SprintButton )
			? SprintSpeed
			: Speed;
		var input = new Vector3( Input.AnalogMove,
			(Input.Down( AscendButton ) ? 1f : 0f) + (Input.Down( DescentButton ) ? -1f : 0f) );
		var movement = input * speed;
		var rotatedMovement = rotation.Forward * movement.x + rotation.Left * movement.y + Vector3.Up * movement.z;

		if ( FreeRoamColliderRadius is { } radius )
		{
			// DebugOverlay.Sphere( new Sphere( WorldPosition, radius ) );
			var trace = Scene.Trace
				.Radius( radius )
				.IgnoreGameObject( GameObject )
				.UsePhysicsWorld()
				.WithAnyTags( FreeRoamColliderTags );

			var helper = new CharacterControllerHelper
			{
				Trace = trace,
				Bounce = 0,
				Position = WorldPosition,
				MaxStandableAngle = 90,
				Velocity = rotatedMovement
			};

			helper.TryMove( deltaTime );

			newPosition = helper.Position;
		}
		else
		{
			newPosition += rotatedMovement * deltaTime;
		}

		if ( HasBounds )
		{
			var extents = Rigidbody?.PhysicsBody?.GetBounds().Extents ?? Vector3.Zero;
			var bounds = new BBox( Bounds.Mins + extents, Bounds.Maxs - extents );
			// DebugOverlay.Box( Bounds, color: Color.Green );
			// DebugOverlay.Box( bounds, color: Color.Red );
			newPosition = newPosition.Clamp( bounds );
		}
		*/
	}
}
