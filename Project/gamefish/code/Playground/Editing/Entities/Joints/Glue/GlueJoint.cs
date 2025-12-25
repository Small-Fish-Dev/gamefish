namespace Playground;

[Icon( "import_export" )]
public partial class GlueJoint : JointEntity
{
	[Property]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Sandbox.FixedJoint Joint { get; set; }

	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public GlueSettings Settings { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		// Snap to the offset without lag if we're the owner.
		if ( GameObject.Parent.IsValid() && !GameObject.Parent.IsProxy )
			TryAttachTo( a: ParentPoint, b: TargetPoint );
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

	protected override void DrawJointGizmo()
	{
		var objParent = ParentPoint.Object;
		var objTarget = TargetPoint.Object;

		if ( !objParent.IsValid() || !objTarget.IsValid() )
			return;

		if ( ParentPoint.Offset is not Offset parentOffset )
			return;

		if ( TargetPoint.Offset is not Offset targetOffset )
			return;

		var c = Color.Magenta.Desaturate( 0.4f ).WithAlpha( 0.3f );

		var tParentPoint = objParent.WorldTransform.WithOffset( parentOffset );
		var tTargetPoint = objTarget.WorldTransform.WithOffset( targetOffset );

		this.DrawArrow(
			from: tParentPoint.Position,
			to: tTargetPoint.Position,
			c: c, len: 16f, w: 4f, th: 3f,
			tWorld: global::Transform.Zero
		);
	}

	public override void ApplySettings()
	{
		if ( !Joint.IsValid() )
			return;

		var damping = Settings.Damping.Clamp( 0f, 100f );
		var strength = Settings.Strength.Clamp( 0f, 20f );

		Joint.LinearDamping = damping;
		Joint.LinearFrequency = strength;

		Joint.AngularDamping = damping;
		Joint.AngularFrequency = strength;
	}

	public override void UpdateJoint( in float deltaTime )
	{
	}

	public override bool TryAttachTo( in ToolAttachPoint a, in ToolAttachPoint b )
	{
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

		if ( !aPhys.IsValid() || b.Offset is not Offset targetOffset )
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
