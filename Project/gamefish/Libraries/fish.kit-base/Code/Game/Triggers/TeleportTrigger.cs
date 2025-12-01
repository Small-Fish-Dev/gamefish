namespace GameFish;

public class TeleportTrigger : FilterTrigger
{
	protected const string TELEPORT = "🧬 Teleport";
	protected const int TELEPORT_ORDER = TRIGGER_ORDER - 1997;

	public enum VelocityMode
	{
		/// <summary>
		/// All speed is redirected to the destination's forward facing direction.
		/// </summary>
		Redirect = 0,
		/// <summary>
		/// All velocity is reset upon teleporting.
		/// </summary>
		Reset = 1,
		/// <summary>
		/// Don't affect velocity(inadvisable in most cases).
		/// </summary>
		None = 2,
	}

	[Title( "Destination" )]
	[Property, Feature( TELEPORT ), Order( TELEPORT_ORDER )]
	public GameObject TeleportDestination { get; set; }

	[Title( "Velocity Mode" )]
	[Property, Feature( TELEPORT ), Order( TELEPORT_ORDER )]
	public VelocityMode TeleportVelocityMode { get; set; } = VelocityMode.Redirect;

	/// <summary>
	/// Should pawns have their teleport position offset
	/// so that their center is at the destination origin?
	/// </summary>
	[Title( "Center Pawns" )]
	[Property, Feature( TELEPORT ), Order( TELEPORT_ORDER )]
	public bool CenterPawns { get; set; } = true;

	/// <summary>
	/// The sound to play on this side when teleporting to the other side.
	/// </summary>
	[Title( "Teleport Sound" )]
	[Property, Feature( TELEPORT ), Order( TELEPORT_ORDER ), Group( SOUNDS )]
	public SoundEvent TeleportEnterSound { get; set; }

	/// <summary>
	/// The sound to play on the other side.
	/// </summary>
	[Title( "Object Sound" )]
	[Property, Feature( TELEPORT ), Order( TELEPORT_ORDER ), Group( SOUNDS )]
	public SoundEvent TeleportExitSound { get; set; }

	public override Color DefaultGizmoColor => Color.Magenta.Desaturate( 0.3f );

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( TeleportVelocityMode is not VelocityMode.Redirect )
			return;

		if ( !TeleportDestination.IsValid() )
			return;

		var tDest = TeleportDestination.WorldTransform.WithScale( 1f );

		this.DrawArrow(
			Vector3.Zero, Vector3.Forward * 128f,
			tWorld: tDest,
			th: 2f, len: 32f, w: 16f,
			c: GizmoColor
		);
	}

	protected override void OnTouchStart( GameObject obj )
	{
		if ( !obj.IsValid() || !TeleportDestination.IsValid() )
			return;

		if ( !TryTeleport( obj ) )
			return;

		// this.Log( "teleported: ", obj );

		// TODO: GameObject extension for playing sounds at positions.
		if ( TeleportEnterSound.IsValid() )
			BroadcastSound( TeleportEnterSound );

		if ( TeleportExitSound.IsValid() )
			BroadcastSound( TeleportExitSound, new SoundSettings( TeleportDestination.WorldPosition ) );
	}

	/// <returns> If the object belonged to us and could be teleported. </returns>
	protected virtual bool TryTeleport( GameObject obj )
	{
		if ( !obj.IsValid() || obj.IsProxy )
			return false;

		if ( !TeleportDestination.IsValid() )
			return false;

		var tDest = TeleportDestination.WorldTransform;

		// Look for the interface explicitly designed for this behavior.
		foreach ( var iTele in obj.Components.GetAll<ITransform>( FindMode.EnabledInSelf | FindMode.InAncestors ) )
		{
			if ( iTele is not Component c || !c.IsValid() || c.IsProxy )
				continue;

			// Center pawns on the destination.
			if ( CenterPawns && Pawn.TryGet<Pawn>( obj, out var pawn ) )
				tDest.Position -= pawn.Center - pawn.WorldPosition;

			if ( iTele.TryTeleport( tDest ) )
			{
				// If we don't force a snap then it may not work.
				c.Transform?.ClearInterpolation();
				ApplyVelocityMode( c.GameObject, TeleportVelocityMode );

				return true;
			}
		}

		// Look for Rigidbodies on root.
		if ( obj.Components.TryGet<Rigidbody>( out var rb, FindMode.EnabledInSelf | FindMode.InAncestors ) )
		{
			if ( !rb.IsValid() || rb.IsProxy )
				return false;

			rb.WorldPosition = tDest.Position;
			rb.WorldRotation = tDest.Rotation;

			rb.Transform?.ClearInterpolation();

			ApplyVelocityMode( rb.GameObject, TeleportVelocityMode );

			return true;
		}

		return false;
	}

	protected virtual void ApplyVelocityMode( GameObject obj, VelocityMode mode )
	{
		if ( !obj.IsValid() || obj.IsProxy )
			return;

		if ( mode is VelocityMode.None )
			return;

		// Support Game Fish components(such as pawns with no Rigidbody) first.
		var iVelAny = false;

		foreach ( var physEnt in obj.Components.GetAll<PhysicsEntity>( FindMode.EnabledInSelf | FindMode.InAncestors ) )
		{
			if ( !physEnt.IsValid() || physEnt.IsProxy )
				continue;

			iVelAny = true;

			ApplyVelocityMode( physEnt.Velocity, out var newVel, mode );
			physEnt.Velocity = newVel;
		}

		if ( iVelAny )
			return;

		// Find the Rigidbody component otherwise.
		if ( obj.Components.TryGet<Rigidbody>( out var rb, FindMode.EnabledInSelf | FindMode.InAncestors ) )
		{
			if ( !rb.IsValid() || rb.IsProxy || !rb.MotionEnabled )
				return;

			ApplyVelocityMode( rb.Velocity, out var newVel, mode );
			rb.Velocity = newVel;
		}
	}

	protected virtual void ApplyVelocityMode( in Vector3 inVel, out Vector3 outVel, VelocityMode mode )
	{
		if ( mode is VelocityMode.Reset )
		{
			outVel = Vector3.Zero;
			return;
		}
		else if ( mode is VelocityMode.Redirect )
		{
			var dir = TeleportDestination?.WorldRotation.Forward ?? Vector3.Forward;
			outVel = dir.Normal * inVel.Length;
			return;
		}

		outVel = inVel;
	}
}
