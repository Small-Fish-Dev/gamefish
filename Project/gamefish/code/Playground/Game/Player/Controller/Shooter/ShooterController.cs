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

	public override bool AimPitchClamping => false;

	public override Vector3 Gravity => Down * base.Gravity.Length;

	protected bool IsFreeLooking => IsFocusing;

	[Sync]
	public bool IsFocusing { get; protected set; }

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
			var eyeDown = EyeRotation.Down;
			var tr = Pawn.GetEyeTrace( 1024f, dir: eyeDown ).Run();

			if ( tr.Hit )
				TrySetPerspective( in tr );
			else
				TrySetPerspective( EyeRotation );
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
