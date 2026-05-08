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

	[Property]
	[InputAction]
	[Title( "Input" )]
	[Feature( BADASS ), Group( FOCUS )]
	public virtual string FocusInput { get; set; } = "Attack2";

	/// <summary>
	/// Multiplier of gravity while holding jump.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Gravity (float)" )]
	[Feature( BADASS ), Group( FORCES )]
	[Range( 0.2f, 1.0f, clamped: false )]
	public virtual float JumpGravityScale { get; set; } = 0.7f;

	/// <summary>
	/// Multiplier of gravity while holding duck.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Gravity (sink)" )]
	[Feature( BADASS ), Group( FORCES )]
	[Range( 1.0f, 3.0f, clamped: false )]
	public virtual float DuckGravityScale { get; set; } = 2f;

	public override bool AimPitchClamping => false;

	public override Vector3 Gravity => Down * base.Gravity.Length * GravityMultiplier();

	protected bool IsFreeLooking => IsFocusing;

	[Sync]
	public bool IsFocusing { get; protected set; }

	protected virtual float GravityMultiplier()
	{
		var mult = 1f;

		if ( JumpInput.IsHeld )
			mult *= JumpGravityScale;

		if ( Input.Down( DuckInput ) )
			mult *= DuckGravityScale;

		return mult;
	}

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

		UpdateGravityBoots();
	}

	protected virtual void UpdateGravityBoots()
	{
		if ( !Pawn.IsValid() )
			return;

		// Stick to what's beneath us.
		if ( Input.Down( "Run" ) )
		{
			var dir = EyeRotation.Down;
			var tr = Pawn.GetEyeTrace( 1024f, dir: dir ).Run();

			if ( tr.Hit )
				TrySetPerspective( in tr );
		}

		// Stick to where we're looking.
		if ( Input.Pressed( "Item" ) )
		{
			var tr = Pawn.GetEyeTrace( 16384f ).Run();
			TrySetPerspective( in tr );
		}
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

		if ( !Pawn.IsValid() )
			return false;

		var oldCenter = Center;

		var rEye = Pawn.EyeRotation;
		var eyePos = Pawn.EyePosition;

		Pawn.WorldRotation = rForward;
		Pawn.EyeRotation = rEye;

		Pawn.WorldPosition += oldCenter - Center;
		Pawn.EyePosition = eyePos;

		return true;
	}
}
