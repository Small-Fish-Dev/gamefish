using GameFish;

namespace Fishbox;

partial class ShooterController
{
	/// <summary>
	/// Multiplier of gravity while holding jump.
	/// </summary>
	[Property]
	[InputAction]
	[Order( BADASS_ORDER )]
	[Title( "Gravity (float)" )]
	[Feature( BADASS ), Group( GRAVITY )]
	[Range( 0.2f, 1.0f, clamped: false )]
	public virtual float JumpGravityScale { get; set; } = 0.7f;

	/// <summary>
	/// Multiplier of gravity while holding duck.
	/// </summary>
	[Property]
	[InputAction]
	[Order( BADASS_ORDER )]
	[Title( "Gravity (sink)" )]
	[Feature( BADASS ), Group( GRAVITY )]
	[Range( 1.0f, 3.0f, clamped: false )]
	public virtual float DuckGravityScale { get; set; } = 2f;

	[Property]
	[Order( BADASS_ORDER )]
	[Title( "Gravity (wall run)" )]
	[Feature( BADASS ), Group( GRAVITY )]
	[Range( 0.2f, 1.0f, clamped: false )]
	public virtual float WallRunGravityScale { get; set; } = 0.8f;

	public override Vector3 Gravity => SceneGravity * GravityMultiplier();

	protected virtual float GravityMultiplier()
	{
		var mult = 1f;

		if ( JumpInput.IsHeld )
			mult *= JumpGravityScale;

		if ( Input.Down( DuckInput ) )
			mult *= DuckGravityScale;

		if ( IsWallRunning() )
			mult *= WallRunGravityScale;

		return mult;
	}

	public override float GetMovementSpeed()
	{
		if ( IsWallRunning() )
			return WallRunMoveSpeed;

		return base.GetMovementSpeed();
	}

	protected override void PreMove( in float deltaTime )
	{
		base.PreMove( deltaTime );

		UpdateParkour( in deltaTime );
	}
}
