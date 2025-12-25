using System.Drawing;

namespace Playground;

[Icon( "import_export" )]
public partial class SpringJoint : JointEntity
{
	[Property]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Sandbox.SpringJoint Joint { get; set; }

	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public SpringSettings Settings { get; set; }

	[Sync]
	[Property, ReadOnly]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public bool IsToggled { get; set; }

	[Sync]
	public float Length { get; set; }

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

	protected virtual void UpdateInput( in float deltaTime )
	{
		var bToggle = !Settings.KeyToggle.IsBlank()
			&& Input.Keyboard.Pressed( Settings.KeyToggle );

		if ( bToggle != IsToggled )
			IsToggled = bToggle;
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

		var c = Color.Yellow.Desaturate( 0.4f ).WithAlpha( 0.3f );

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

		Joint.Damping = Settings.Damping.Clamp( 0f, 1000f );
		Joint.Frequency = Settings.Springiness.Clamp( 0f, 1000f );

		Joint.RestLength = Length.Positive();

		Joint.MinLength = 0f;
		Joint.MaxLength = (Joint.RestLength + Settings.Slack).Positive();
	}

	public override void UpdateJoint( in float deltaTime )
	{
		if ( !Joint.IsValid() )
			return;

		if ( !LocalPoint.Object.IsValid() )
			return;

		/*
		if ( Direction.AlmostEqual( 0f ) )
			return;

		// var newLength = 
		*/

		ApplySettings();
	}

	public override bool TryAttachTo( in DeviceAttachPoint a, in DeviceAttachPoint b )
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

		if ( !bPhys.IsValid() || b.Offset is not Offset targetOffset )
			return false;

		var tParentPoint = objParent.WorldTransform.WithOffset( parentOffset );
		var tTargetPoint = objTarget.WorldTransform.WithOffset( targetOffset );

		Length = tParentPoint.Position.Distance( tTargetPoint.Position );

		var aPhysLocal = aPhys.Transform.ToLocal( tParentPoint );
		var bPhysLocal = bPhys.Transform.ToLocal( tTargetPoint );

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
