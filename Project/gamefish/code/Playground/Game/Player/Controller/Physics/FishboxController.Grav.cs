using GameFish;

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

	[Sync]
	public TimeUntil NextGround { get; set; }

	public override Vector3 Gravity => GravityDirection * base.Gravity.Length;

	public override void OnSetIsGrounded( in bool isGrounded )
	{
		base.OnSetIsGrounded( isGrounded );

		if ( isGrounded && IsWallRunning )
			IsWallRunning = false;

		if ( !isGrounded && FollowObject.IsValid() )
			FollowObject = null;
	}

	/// <summary>
	/// Finds some kind of normal/up to walk along.
	/// </summary>
	protected bool TryGetGroundNormal( out Vector3 vUp )
	{
		if ( IsGrounded && !GroundNormal.AlmostEqual( 0f ) )
		{
			vUp = GroundNormal.Normal;
		}
		else if ( !GravityDirection.AlmostEqual( 0f ) )
		{
			vUp = -GravityDirection.Normal;
		}
		else
		{
			vUp = Up;
		}

		return !vUp.AlmostEqual( 0f );
	}

	protected virtual void OnSetGravityDirection( in Vector3 dir )
	{
		// SetUpDirection( -dir );
	}

	protected virtual void DoGravity( in float deltaTime )
	{
		// TODO: Make this a module.
		if ( Pawn.IsValid() && Input.Pressed( "Item" ) )
		{
			var trEye = Pawn.GetEyeTrace( dir: EyeForward, distance: 8192f ).Run();

			if ( trEye.Hit )
			{
				IsGrounded = false;
				GravityDirection = -trEye.Normal;
				// SetUpDirection( -GravityDirection );
			}
		}

		// We'll be making this orbital/field-based later.
		var grav = Gravity;

		// Modify gravity depending on input/state.
		if ( IsWallRunning )
		{
			grav *= WallRunGravity;
		}
		else if ( !IsGrounded )
		{
			if ( ShouldJump() )
				grav *= GravityFloating;

			if ( !IsSliding && IsDucking )
				grav *= GravitySinking;
		}

		Velocity += grav * deltaTime;
	}
}
