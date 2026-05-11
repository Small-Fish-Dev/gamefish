using GameFish;

namespace Fishbox;

/// <summary>
/// /// A badass player controller. <br />
/// /// Shout out to <b>Cyber-Ninja: Ascension</b>.
/// </summary>
[Icon( "sports_esports" )]
public partial class ShooterController : FirstPersonController
{
	protected const int BADASS_ORDER = PAWN_ORDER - 1000;

	protected const string BOOTS = BOOT + "s";

	[Property]
	[InputAction]
	[Title( "Input" )]
	[Order( BADASS_ORDER )]
	[Feature( BADASS ), Group( FOCUS )]
	public virtual string FocusInput { get; set; } = "Attack2";

	[Property]
	[InputAction]
	[Title( "Boots" )]
	[Order( BADASS_ORDER )]
	[Feature( BADASS ), Group( INPUT )]
	public virtual string BootsInput { get; set; } = "Run";

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

	public override bool AimPitchClamping => false;

	public override Vector3 Gravity => Down * base.Gravity.Length * GravityMultiplier();

	protected bool IsFreeLooking => IsFocusing && !IsGrounded;

	[Sync]
	public bool IsFocusing { get; protected set; }

	// [Sync]
	// public Rotation? TargetRotation { get; protected set; }

	/// <summary>
	/// When do the gravity boots stop working?
	/// </summary>
	[Sync]
	public TimeSince? SinceBootsUsed { get; protected set; }

	protected override void UpdateInput( in float deltaTime )
	{
		base.UpdateInput( deltaTime );

		var isAlive = Pawn?.IsAlive is true;

		IsFocusing = isAlive && Input.Down( FocusInput );
	}

	protected override void ResetInput()
	{
		base.ResetInput();

		IsFocusing = false;
	}

	public override bool TryAim( in Rotation rLook, in float deltaTime )
	{
		if ( !ITransform.IsValid( in rLook ) )
			return false;

		if ( IsFreeLooking )
		{
			LocalEyeRotation *= rLook;
			return true;
		}

		var rAim = LocalEyeRotation;
		var rInverse = rAim.Inverse;

		rAim *= Rotation.FromAxis( rInverse.Up, rLook.Yaw() );
		rAim *= Rotation.FromPitch( rLook.Pitch() );

		LocalEyeRotation = rAim;

		return true;
	}

	protected override void UpdateEyeRotation( in float deltaTime )
	{
		if ( !IsFreeLooking )
			ResetEyeRoll( AimRollResetSpeed, in deltaTime );
	}

	protected override void ResetEyeRoll( in float speed, in float deltaTime )
	{
		var tEye = EyeTransform;
		var newUp = tEye.Up.SlerpTo( Up, speed * deltaTime );

		EyeRotation = Rotation.LookAt( tEye.Forward, newUp );
	}

	public override void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
		base.Simulate( deltaTime, isFixedUpdate );

		UpdateGravity( in deltaTime );
	}

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

	protected bool TrySetPerspective( in SceneTraceResult tr )
	{
		if ( !tr.Hit )
			return false;

		var rUp = Rotation.LookAt( tr.Normal, EyeForward );
		var rForward = Rotation.LookAt( rUp.Up, rUp.Forward );

		return TrySetPerspective( in rForward );
	}

	protected bool TrySetPerspective( in Rotation rForward )
	{
		if ( !ITransform.IsValid( in rForward ) )
			return false;

		// TargetRotation = rForward;
		Reorient( rForward );

		return true;
	}

	protected void Reorient( in Rotation rForward )
	{
		if ( !Pawn.IsValid() )
			return;

		var oldCenter = Center;

		var rEye = Pawn.EyeRotation;
		var eyePos = Pawn.EyePosition;

		Pawn.WorldRotation = rForward;
		Pawn.EyeRotation = rEye;

		Pawn.WorldPosition += oldCenter - Center;
		Pawn.EyePosition = eyePos;
	}
}
