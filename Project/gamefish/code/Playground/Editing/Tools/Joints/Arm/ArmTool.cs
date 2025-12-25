namespace Playground;

public partial class ArmTool : JointTool
{
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual ArmSettings ArmSettings { get; set; }

	public override bool TryAttach( in DeviceAttachPoint point1, in DeviceAttachPoint point2 )
		=> TryAttach<ArmJoint>( in point1, in point2 );

	protected override void DrawJointGizmos()
	{
		base.DrawJointGizmos();

		var a = Point1;
		var b = PointTarget;

		if ( !a.IsValid || !b.IsValid )
			return;

		var tParentPoint = a.Object.WorldTransform.WithOffset( a.Offset.Value );
		var tTargetPoint = b.Object.WorldTransform.WithOffset( b.Offset.Value );

		var c = Color.Green.WithAlpha( 0.3f );

		this.DrawArrow(
			from: tParentPoint.Position,
			to: tTargetPoint.Position,
			c: c, len: 7f, w: 2f, th: 4f,
			tWorld: global::Transform.Zero
		);
	}

	protected override void DrawPointGizmo( in DeviceAttachPoint point )
	{
		if ( !point.Object.IsValid() || !point.Offset.HasValue )
			return;

		if ( !ValidAttachment( point ) )
			return;

		var c = Color.Green.WithAlpha( 0.3f );

		var tObj = point.Object.WorldTransform;
		var tArrow = tObj.ToWorld( point.Offset.Value );

		var dir = point.HitNormal ?? tArrow.Forward;

		this.DrawArrow(
			from: tArrow.Position,
			to: tArrow.Position + (dir * 6f),
			c: c, len: 5f, w: 5f, th: 4f,
			tWorld: global::Transform.Zero
		);
	}

	public override void ApplySettings<TJoint>( TJoint joint )
	{
		if ( joint is not ArmJoint arm )
			return;

		arm.Settings = ArmSettings;
	}

	public override bool TryClear( GameObject obj )
		=> TryClear<ArmJoint>( obj );

	[Rpc.Host]
	protected override void RpcRemoveJoints( GameObject obj )
	{
		if ( !obj.IsValid() || !TryUse( Rpc.Caller, out _ ) )
			return;

		const FindMode findMode = FindMode.EverythingInSelf | FindMode.InDescendants;

		var arms = obj.Components.GetAll<ArmJoint>( findMode );

		if ( !arms.Any() )
			return;

		foreach ( var th in arms.ToArray() )
			th.Destroy();
	}
}
