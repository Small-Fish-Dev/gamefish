using GameFish;

namespace Fishbox;

partial class ShooterController
{
	protected const string BOOTS = BOOT + "s";

	/// <summary>
	/// Multiplier of gravity while holding jump.
	/// </summary>
	[Property]
	[InputAction]
	[Order( BADASS_ORDER )]
	[Title( "Gravity (float)" )]
	[Feature( BADASS ), Group( FORCES )]
	[Range( 0.2f, 1.0f, clamped: false )]
	public virtual float JumpGravityScale { get; set; } = 0.7f;

	/// <summary>
	/// Multiplier of gravity while holding duck.
	/// </summary>
	[Property]
	[InputAction]
	[Order( BADASS_ORDER )]
	[Title( "Gravity (sink)" )]
	[Feature( BADASS ), Group( FORCES )]
	[Range( 1.0f, 3.0f, clamped: false )]
	public virtual float DuckGravityScale { get; set; } = 2f;

	[Property]
	[InputAction]
	[Title( "Boots" )]
	[Order( BADASS_ORDER )]
	[Feature( BADASS ), Group( INPUT )]
	public virtual string BootsInput { get; set; } = "Run";

	/// <summary>
	/// The delay after you stop using boots that they automatically detatch.
	/// </summary>
	[Property]
	[InputAction]
	[Order( BADASS_ORDER )]
	[Title( "Auto-Detach" )]
	[Feature( BADASS ), Group( BOOTS )]
	[Range( 0.0f, 4.0f, clamped: false )]
	public virtual float BootsAutoDetach { get; set; } = 1.5f;

	/// <summary>
	/// If defined: the last time the boots stuck to something.
	/// </summary>
	[Sync]
	public TimeSince? SinceBootsUsed { get; protected set; }

	public override Vector3 Gravity => Down * base.Gravity.Length * GravityMultiplier();

	protected virtual float GravityMultiplier()
	{
		var mult = 1f;

		if ( JumpInput.IsHeld )
			mult *= JumpGravityScale;

		if ( Input.Down( DuckInput ) )
			mult *= DuckGravityScale;

		return mult;
	}

	protected virtual void UpdateGravity( in float deltaTime )
	{
		if ( !Pawn.IsValid() )
			return;

		if ( Input.Down( BootsInput ) )
		{
			var down = EyeRotation.Down;

			var tr = Pawn.GetEyeTrace( 512f, dir: down )
				.Radius( ViewCollisionRadius )
				.Run();

			if ( TrySetPerspective( in tr ) || IsGrounded )
				SinceBootsUsed = 0f;
		}

		UpdateBootsCooldown( in deltaTime );

		/*
		if ( TargetRotation is Rotation rTarget )
		{
			var rWorld = Pawn.WorldRotation;

			if ( rWorld == rTarget )
				return;

			var speed = deltaTime * 20f;
			var rLerped = rWorld.LerpTo( rTarget, speed );

			Reorient( in rLerped );
		}
		*/
	}

	protected virtual void UpdateBootsCooldown( in float deltaTime )
	{
		if ( SinceBootsUsed is not TimeSince sinceBoots )
			return;

		if ( sinceBoots < BootsAutoDetach )
			return;

		SinceBootsUsed = null;

		var fwd = EyeForward;
		var up = -(Scene?.PhysicsWorld?.Gravity.Normal) ?? Vector3.Up;

		var rUp = Rotation.LookAt( up, fwd );
		var rForward = Rotation.LookAt( rUp.Up, rUp.Forward );

		Reorient( rForward );
	}
}
