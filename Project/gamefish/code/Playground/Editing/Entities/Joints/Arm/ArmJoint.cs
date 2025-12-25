namespace Playground;

[Icon( "precision_manufacturing" )]
public partial class ArmJoint : JointEntity
{
	[Property]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Sandbox.BallJoint Joint { get; set; }

	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public ArmSettings Settings { get; set; }

	[Sync]
	[Property, ReadOnly]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Angles SteerAngles { get; set; } = Angles.Zero;

	[Sync]
	[Property]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Angles TargetAngles
	{
		get => _targetAngles;
		set => _targetAngles = value.Normal;
	}

	protected Angles _targetAngles;

	protected override void OnStart()
	{
		base.OnStart();

		// Snap to the offset without lag if we're the owner.
		if ( GameObject.Parent.IsValid() && !GameObject.Parent.IsProxy )
			TryAttachTo( a: LocalPoint, b: TargetPoint );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy )
			return;

		UpdateSteering( Time.Delta );
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

	protected void UpdateSteering( in float deltaTime )
	{
		var steer = Angles.Zero;

		// Pitch
		var pitchUp = !Settings.KeyPitchUp.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyPitchUp );

		var pitchDown = !Settings.KeyPitchDown.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyPitchDown );

		if ( pitchUp )
			steer.pitch += 1;

		if ( pitchDown )
			steer.pitch -= 1;

		SteerAngles = steer;
	}

	protected override void DrawJointGizmo()
	{
		var objParent = LocalPoint.Object;
		var objTarget = TargetPoint.Object;

		if ( !objParent.IsValid() || !objTarget.IsValid() )
			return;

		if ( LocalPoint.Offset is not Offset parentOffset )
			return;

		if ( TargetPoint.Offset is not Offset targetOffset )
			return;

		var c = Color.Green.WithAlpha( 0.3f );

		var tParentPoint = objParent.WorldTransform.WithOffset( parentOffset );
		var tTargetPoint = objTarget.WorldTransform.WithOffset( targetOffset );

		this.DrawArrow(
			from: tParentPoint.Position,
			to: tTargetPoint.Position,
			c: c, len: 7f, w: 2f, th: 4f,
			tWorld: global::Transform.Zero
		);
	}

	public override void ApplySettings()
	{
	}

	public override void UpdateJoint( in float deltaTime )
	{
		if ( !Joint.IsValid() )
			return;

		if ( SteerAngles.IsNearlyZero() )
			return;

		if ( !LocalPoint.Object.IsValid() )
			return;

		var dirYaw = SteerAngles.yaw.Clamp( -1f, 1f );
		var dirPitch = SteerAngles.pitch.Clamp( -1f, 1f );
		var dirRoll = SteerAngles.roll.Clamp( -1f, 1f );

		var addYaw = dirYaw * Settings.Speed * deltaTime;
		var addPitch = dirPitch * Settings.Speed * deltaTime;
		var addRoll = dirRoll * Settings.Speed * deltaTime;

		var angTarget = TargetAngles;

		const float angleLimit = -89.5f;

		angTarget.yaw = (angTarget.yaw + addYaw).Clamp( -angleLimit, angleLimit );
		angTarget.pitch = (angTarget.pitch + addPitch).Clamp( -angleLimit, angleLimit );
		angTarget.roll = (angTarget.roll + addRoll).Clamp( -angleLimit, angleLimit );

		TargetAngles = angTarget;

		Joint.TargetRotation = TargetAngles;
	}

	public override bool TryAttachTo( in DeviceAttachPoint a, in DeviceAttachPoint b )
	{
		// Must have a Ball(the entire point).
		if ( !Joint.IsValid() )
			return false;

		var objParent = a.Object;
		var objTarget = b.Object;

		if ( !objParent.IsValid() || !objTarget.IsValid() )
			return false;

		// Disgraceful..
		if ( objParent == objTarget )
			return false;

		var aPhys = Physics.FindBody( objParent );

		if ( !aPhys.IsValid() || a.Offset is not Offset parentOffset )
			return false;

		var bPhys = Physics.FindBody( objTarget );

		if ( !bPhys.IsValid() || b.Offset is not Offset targetOffset )
			return false;

		var tParentPoint = objParent.WorldTransform.WithOffset( parentOffset );
		var tTargetPoint = objTarget.WorldTransform.WithOffset( targetOffset );

		var aPhysLocal = aPhys.Transform.ToLocal( tParentPoint );
		var bPhysLocal = bPhys.Transform.ToLocal( tParentPoint );

		// var aPhysPoint = new PhysicsPoint( aPhys, aPhysLocal.Position, aPhysLocal.Rotation );
		// var bPhysPoint = new PhysicsPoint( bPhys, bPhysLocal.Position, bPhysLocal.Rotation );

		if ( !ITransform.IsValid( aPhysLocal ) || !ITransform.IsValid( bPhysLocal ) )
			return false;

		LocalTransform = parentOffset;

		GameObject.SetParent( objParent, keepWorldPosition: true );
		Transform.ClearInterpolation();

		// Let the engine's component handle it.
		Joint.Attachment = Sandbox.Joint.AttachmentMode.LocalFrames;

		Joint.LocalFrame1 = aPhysLocal;
		Joint.LocalFrame2 = bPhysLocal;

		ApplySettings();

		// Set the other body.
		Joint.Body = objTarget;

		return true;
	}
}
