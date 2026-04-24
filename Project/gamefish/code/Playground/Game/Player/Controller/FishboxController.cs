using GameFish;
using ShrimpleCharacterController;

namespace Fishbox;

public partial class FishboxController : ShooterController
{
	public const int PLAYER_ORDER = DEFAULT_ORDER - 1997;

	public const int DEFAULTS_ORDER = PLAYER_ORDER + 100;

	public const string SLIDING = "🏄 Sliding";
	public const int SLIDING_ORDER = 5000;

	public const string WALLRUNNING = "🧗 Wall Running";
	public const int WALLRUNNING_ORDER = 6000;

	public override bool ShouldJump => AllowJumping && HasJumpButton && HoldingJump;

	protected bool HoldingJump => Input.Down( JumpButton );
	protected bool PressedJump => Input.Pressed( JumpButton );

	protected override void OnStart()
	{
		base.OnStart();

		if ( !Rigidbody.IsValid() )
			this.Warn( $"needs a {typeof( Rigidbody )} to function!" );

		UpdateCollision();
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();
	}

	protected Vector3 _rEyeVel = Vector3.Zero;

	protected override void UpdateEyeRotation( in float deltaTime )
	{
		base.UpdateEyeRotation( deltaTime );

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
	}

	public override Vector3 GetLocalEyeTargetPosition()
	{
		var target = base.GetLocalEyeTargetPosition();

		if ( IsSliding )
			target *= .7f;
		else if ( IsWallRunning )
			target += WallRunNormal * 8f;

		return target;
	}

	protected override void OnSetLocalEyePosition( in Vector3 pos )
	{
		base.OnSetLocalEyePosition( pos );

		UpdateCollision();
	}

	protected virtual void RenderColliders( in float? totalHeight = null, Transform? tWorld = null )
	{
		var height = totalHeight ?? GetTotalHeight();

		var t = tWorld ?? WorldTransform;
		var tBody = new Transform( GetWorldBodyCenter( t, height ), WorldRotation );
		var tHead = new Transform( GetWorldHeadCenter( t, height ), WorldRotation );

		this.DrawCylinder(
			Radius, GetBodyHeight( height ),
			Color.Cyan, Color.Transparent,
			tWorld: tBody
		);

		this.DrawSphere(
			Radius, default,
			Color.Cyan, Color.Transparent,
			tWorld: tHead
		);
	}
}
