namespace Playground;

[Icon( "precision_manufacturing" )]
public partial class PhysicsWheel : EditorEntity
{
	protected const int PHYSICS_ORDER = EDITOR_ORDER + 10;

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
	public ToolAttachPoint ParentPoint { get; set; }

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

	protected void UpdateInput( in float deltaTime )
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
			&& Input.Keyboard.Down( Settings.KeyLeft );

		if ( bLeft )
			drive.x -= 1;

		if ( bRight )
			drive.x += 1;

		DriveInput = drive;
	}

	public void UpdateWheel( in float deltaTime )
	{
		if ( !Joint.IsValid() )
			return;

		Joint.TargetSteeringAngle = DriveInput.x * 30f;
		Joint.SpinMotorSpeed = DriveInput.y * 360f;
	}

	public bool TryAttachTo( in ToolAttachPoint point )
	{
		// Must have a Wheel(the entire point).
		if ( !Joint.IsValid() )
			return false;

		var objTarget = point.Object;

		if ( !objTarget.IsValid() )
			return false;

		// Disgraceful..
		if ( objTarget == GameObject )
			return false;

		var aPhys = Physics.FindBody( objTarget );

		if ( !aPhys.IsValid() || point.Offset is not Offset offset )
			return false;

		var bPhys = Physics.FindBody( GameObject );

		if ( !bPhys.IsValid() )
			return false;

		var tPoint = objTarget.WorldTransform.WithOffset( offset );

		var aPhysLocal = aPhys.Transform.ToLocal( tPoint );
		var bPhysLocal = bPhys.Transform.ToLocal( tPoint );

		// var aPhysPoint = new PhysicsPoint( aPhys, aPhysLocal.Position, aPhysLocal.Rotation );
		// var bPhysPoint = new PhysicsPoint( bPhys, bPhysLocal.Position, bPhysLocal.Rotation );

		if ( !ITransform.IsValid( aPhysLocal ) || !ITransform.IsValid( bPhysLocal ) )
			return false;

		WorldTransform = tPoint;

		Transform.ClearInterpolation();

		// Let the engine's component handle it.
		Joint.Attachment = Sandbox.Joint.AttachmentMode.LocalFrames;

		Joint.LocalFrame2 = aPhysLocal;
		Joint.LocalFrame1 = bPhysLocal;

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
}
