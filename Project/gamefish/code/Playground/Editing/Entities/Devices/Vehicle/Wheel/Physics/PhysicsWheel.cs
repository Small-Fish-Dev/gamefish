namespace Playground;

[Icon( "precision_manufacturing" )]
public partial class PhysicsWheel : Device
{
	[Property]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Sandbox.WheelJoint Joint { get; set; }

	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public PhysicsWheelSettings Settings { get; set; }

	[Sync]
	[Property, ReadOnly]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Vector2 DriveInput { get; set; } = Vector2.Zero;

	[Sync]
	public DeviceAttachPoint ParentPoint { get; set; }

	[Sync]
	public bool IsSteering { get; set; }

	[Sync]
	public bool IsReversed { get; set; }

	protected override void OnEnabled()
	{
		Tags?.Add( TAG_WHEEL );

		base.OnEnabled();
	}

	protected override void OnStart()
	{
		base.OnStart();

		// Snap to the offset without lag if we're the owner.
		if ( GameObject.Parent.IsValid() && !GameObject.Parent.IsProxy )
			TryAttachTo( point: ParentPoint );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		UpdateInput( Time.Delta );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		UpdateWheel( Time.Delta );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( !this.InGame() || !GameObject.IsValid() )
			return;

		// Auto-cleanup empty objects that only had a spiner.
		var comps = GameObject.Components.GetAll( FindMode.EverythingInSelf )
			.Where( comp => comp.IsValid() );

		if ( !comps.Any() )
			GameObject.Destroy();
	}

	protected virtual void UpdateInput( in float deltaTime )
	{
		if ( IsProxy )
			return;

		var drive = Vector2.Zero;

		// Pitch
		var bForward = !Settings.KeyForward.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyForward );

		var bReverse = !Settings.KeyReverse.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyReverse );

		if ( bForward )
			drive.y += 1;

		if ( bReverse )
			drive.y -= 1;

		var bLeft = !Settings.KeyLeft.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyLeft );

		var bRight = !Settings.KeyRight.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyRight );

		if ( bLeft )
			drive.x += 1;

		if ( bRight )
			drive.x -= 1;

		DriveInput = drive;
	}

	public void UpdateWheel( in float deltaTime )
	{
		if ( !Joint.IsValid() )
			return;

		// Steering
		if ( Joint.EnableSteering != IsSteering )
			Joint.EnableSteering = IsSteering;

		var steeringAngle = DriveInput.x * 40f;
		Joint.TargetSteeringAngle = steeringAngle;
		// Joint.SteeringLimits = new( -steeringAngle, steeringAngle );

		// Acceleration
		var motorSpeed = DriveInput.y * Joint.MaxSpinTorque;
		Joint.SpinMotorSpeed = motorSpeed * (IsReversed ? -1f : 1f);
	}

	public bool TryAttachTo( in DeviceAttachPoint point )
	{
		// Must have a Wheel(the entire point).
		if ( !Joint.IsValid() )
			return false;

		var objTarget = point.Object;

		if ( !objTarget.IsValid() || point.Offset is not Offset offset )
			return false;

		// Disgraceful..
		if ( objTarget == GameObject )
			return false;

		var aPhys = Physics.FindBody( GameObject );

		if ( !aPhys.IsValid() )
			return false;

		var bPhys = Physics.FindBody( objTarget );

		if ( !bPhys.IsValid() )
			return false;

		var tParent = objTarget.WorldTransform.WithOffset( offset );
		tParent.Rotation *= Rotation.FromPitch( -90f );

		var aPhysLocal = new Transform( Vector3.Zero, Rotation.From( -90f, 90f, 0f ) );
		var bPhysLocal = bPhys.Transform.ToLocal( tParent );

		if ( !ITransform.IsValid( aPhysLocal ) || !ITransform.IsValid( bPhysLocal ) )
			return false;

		var tWheel = tParent
			.ToWorld( aPhysLocal )
			.WithScale( WorldScale );

		tWheel.Rotation *= Rotation.From( -90f, 0f, 90f );

		WorldTransform = tWheel;

		Transform.ClearInterpolation();

		// Let the engine's component handle it.
		Joint.Attachment = Sandbox.Joint.AttachmentMode.LocalFrames;

		Joint.LocalFrame1 = aPhysLocal;
		Joint.LocalFrame2 = bPhysLocal;

		// Set the other body.
		Joint.Body = objTarget;

		return true;
	}

	protected void DrawWheelGizmos()
	{
		var objParent = ParentPoint.Object;

		if ( !objParent.IsValid() )
			return;

		if ( ParentPoint.Offset is not Offset parentOffset )
			return;

		var c = Color.White.WithAlpha( 0.3f );

		var tParentPoint = objParent.WorldTransform.WithOffset( parentOffset );

		this.DrawArrow(
			from: WorldPosition,
			to: tParentPoint.Position,
			c: c, len: 7f, w: 2f, th: 4f,
			tWorld: global::Transform.Zero
		);
	}

	[Rpc.Owner( NetFlags.Reliable | NetFlags.SendImmediate )]
	public void RpcToggleSteering()
	{
		if ( !Server.TryFindClient( Rpc.Caller, out var _ ) )
			return;

		IsSteering = !IsSteering;
	}

	[Rpc.Owner( NetFlags.Reliable | NetFlags.SendImmediate )]
	public void RpcToggleReverse()
	{
		if ( !Server.TryFindClient( Rpc.Caller, out var _ ) )
			return;

		IsReversed = !IsReversed;
	}
}
