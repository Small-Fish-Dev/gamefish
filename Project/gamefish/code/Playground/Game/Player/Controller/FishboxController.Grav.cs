using GameFish;
using ShrimpleCharacterController;
using SCC = ShrimpleCharacterController.ShrimpleCharacterController;

namespace Fishbox;

partial class FishboxController
{
	[Property]
	[Feature( PLAYER ), Group( PHYSICS )]
	[Range( 0f, 1f, clamped: false ), Step( 0.01f )]
	public float GravityFloating { get; set; } = 0.5f;

	/// <summary>
	/// Multiplies airborne gravity while duck is held.
	/// </summary>
	[Property]
	[Feature( PLAYER ), Group( PHYSICS )]
	[Range( 0f, 5f, clamped: false ), Step( 0.01f )]
	public float GravitySinking { get; set; } = 2f;

	[Sync]
	[Normal]
	public Vector3 GravityDirection
	{
		get => _gravDir;
		set
		{
			_gravDir = value.Normal;
			OnSetGravityDirection( in _gravDir );
		}
	}

	protected Vector3 _gravDir = Vector3.Down;

	protected virtual void OnSetGravityDirection( in Vector3 dir )
	{
		if ( !Pawn.IsValid() )
			return;

		var localCenter = GetLocalCenter();
		var oldCenter = WorldTransform.PointToWorld( localCenter );

		var tEye = Pawn.EyeTransform;
		var flatDir = Vector3.VectorPlaneProject( tEye.Forward, dir );

		// Perform the rotation.
		WorldRotation = Rotation.LookAt( flatDir, -dir );

		// Recenter us on our previous position.
		var newCenter = WorldTransform.PointToWorld( localCenter );
		WorldPosition += oldCenter - newCenter;

		// Set and correct our eye aim/origin.
		Pawn.EyePosition = tEye.Position;
		Pawn.EyeRotation = tEye.Rotation;

		Transform.ClearInterpolation();

		if ( ShrimpleController.IsValid() )
			ShrimpleController.MaxGroundAngle = 360f;
	}

	protected virtual void DoGravity( in float deltaTime )
	{
		// TODO: Make this a module.
		if ( Pawn.IsValid() && Input.Pressed( "Item" ) )
		{
			var trEye = Pawn.GetEyeTrace( dir: EyeForward, distance: 8192f ).Run();

			if ( trEye.Hit )
				GravityDirection = -trEye.Normal;
		}

		// We'll be making this orbital/field-based later.
		var gravSpeed = Scene?.PhysicsWorld?.Gravity.Length ?? 0f;
		var grav = GravityDirection * gravSpeed;

		// Modify gravity depending on input/state.
		if ( IsWallRunning )
		{
			grav *= WallRunGravity;
		}
		else if ( !_c.IsOnGround )
		{
			if ( ShouldJump )
				grav *= GravityFloating;

			if ( !IsSliding && ShouldDuck )
				grav *= GravitySinking;
		}

		if ( ShrimpleController.IsValid() )
		{
			_c.GravityEnabled = true;
			_c.UseVectorGravity = true;
			_c.VectorGravity = grav;
		}
		else
		{
			Velocity += grav * deltaTime;
		}
	}
}
