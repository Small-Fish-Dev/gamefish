using GameFish;

namespace Fishbox;

partial class ShooterController
{
	protected const string BOOTS = BOOT + "s";

	[Property]
	[InputAction]
	[Title( "Input" )]
	[Order( BADASS_ORDER )]
	[Feature( BADASS ), Group( BOOTS )]
	public virtual string BootsInput { get; set; } = "Run";

	/// <summary>
	/// The delay after you stop using boots that they automatically detatch.
	/// </summary>
	[Property]
	[Order( BADASS_ORDER )]
	[Title( "Auto-Detach" )]
	[Feature( BADASS ), Group( BOOTS )]
	[Range( 0.0f, 4.0f, clamped: false )]
	public virtual float BootsAutoDetach { get; set; } = 0.05f;

	/// <summary>
	/// If defined: the last time the boots stuck to something.
	/// </summary>
	[Sync]
	public TimeSince? SinceBootsUsed { get; protected set; }

	protected virtual void UpdateGravity( in float deltaTime )
	{
		if ( !Pawn.IsValid() )
			return;

		if ( Physics.IsValid() && Input.Down( BootsInput ) )
		{
			var moveDir = EyeRotation * Input.AnalogMove;
			var vel = moveDir * GetMovementSpeed();

			if ( vel.AlmostEqual( 0f ) )
				vel = Velocity;

			var tFrom = Physics.TraceOrigin;
			var to = tFrom.Position + (vel * deltaTime * 1.5f);

			var tr = Physics.Trace( in tFrom, in to, skin: -SkinWidth ).Run();

			if ( !tr.Hit || tr.StartedSolid )
			{
				to = tFrom.Position + (EyeForward * 128f);
				tr = TracePhysics( in tFrom, in to, -SkinWidth ).Run();
			}

			if ( TrySetPerspective( in tr ) || IsGrounded )
				SinceBootsUsed = 0f;

			// DebugOverlay.Trace( tr, duration: 5f );
		}

		UpdateBootsCooldown( in deltaTime );
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
