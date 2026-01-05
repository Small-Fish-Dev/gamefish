using GameFish;
using ShrimpleCharacterController;
using SCC = ShrimpleCharacterController.ShrimpleCharacterController;

namespace Playground;

public partial class FishboxController : ShooterController
{
	public const int PLAYER_ORDER = DEFAULT_ORDER - 1997;

	public const int DEFAULTS_ORDER = PLAYER_ORDER + 100;

	public const string SLIDING = "🏄 Sliding";
	public const int SLIDING_ORDER = 5000;

	public const string WALLRUNNING = "🧗 Wall Running";
	public const int WALLRUNNING_ORDER = 6000;

	// Jump while held on the ground but only if pressed while airborne.
	public override bool ShouldJump => AllowJumping && HasJumpButton && ShrimpleController.IsValid()
		&& (ShrimpleController.IsOnGround ? HoldingJump : PressedJump);

	protected bool HoldingJump => Input.Down( JumpButton );
	protected bool PressedJump => Input.Pressed( JumpButton );

	protected override void OnStart()
	{
		base.OnStart();

		if ( !ShrimpleController.IsValid() )
			this.Warn( $"needs a {typeof( SCC )} to function!" );
	}

	public override void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
		base.Simulate( deltaTime, isFixedUpdate );

		UpdateView( in deltaTime );
	}

	protected Vector3 _rEyeVel = Vector3.Zero;

	public override void UpdateView( in float deltaTime )
	{
		base.UpdateView( deltaTime );

		Rotation rEyeDest;

		var localUp = Vector3.Up;

		if ( IsWallRunning && !WallRunNormal.AlmostEqual( 0f ) )
		{
			var localWall = WorldTransform.NormalToLocal( WallRunNormal ).Normal;
			var upDir = localUp.SlerpTo( localWall, 0.3f );

			rEyeDest = Rotation.LookAt( LocalEyeRotation.Forward, upDir );
		}
		else
		{
			rEyeDest = Rotation.LookAt( LocalEyeRotation.Forward, localUp );
		}

		LocalEyeRotation = Rotation.SmoothDamp( LocalEyeRotation, rEyeDest, ref _rEyeVel, 2f, deltaTime * 4f );
		// LocalEyeRotation.SlerpTo( rEyeDest, deltaTime * 1.5f );
	}

	public override Vector3 GetLocalEyeTargetPosition()
	{
		var target = base.GetLocalEyeTargetPosition(); ;

		if ( IsSliding )
			target *= .75f;
		else if ( IsWallRunning )
			target += WallRunNormal * 8f;

		return target;
	}
}
