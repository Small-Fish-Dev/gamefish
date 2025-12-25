namespace Playground;

public partial class RopeTool : JointTool
{
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual RopeSettings RopeSettings { get; set; }

	public override bool TryAttach( in DeviceAttachPoint point1, in DeviceAttachPoint point2 )
		=> TryAttach<RopeJoint>( in point1, in point2 );

	protected override void DrawJointGizmos()
	{
		base.DrawJointGizmos();

		var a = Point1;
		var b = PointTarget;

		if ( !a.IsValid || !b.IsValid )
			return;

		var tParentPoint = a.Object.WorldTransform.WithOffset( a.Offset.Value );
		var tTargetPoint = b.Object.WorldTransform.WithOffset( b.Offset.Value );

		var c = Color.Orange.Desaturate( 0.3f ).WithAlpha( 0.3f );

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

		var c = Color.Orange.Desaturate( 0.3f ).WithAlpha( 0.3f );

		var tObj = point.Object.WorldTransform;
		var tArrow = tObj.ToWorld( point.Offset.Value );

		var dir = point.HitNormal ?? tArrow.Forward;

		this.DrawArrow(
			from: tArrow.Position + (dir * 7f),
			to: tArrow.Position,
			c: c, len: 7f, w: 2f, th: 3f,
			tWorld: global::Transform.Zero
		);
	}

	public override void ApplySettings<TJoint>( TJoint joint )
	{
		if ( joint is not RopeJoint rope )
			return;

		rope.Settings = RopeSettings;
	}

	public override bool TryClear( GameObject obj )
		=> TryClear<RopeJoint>( obj );

	[Rpc.Host]
	protected override void RpcRemoveJoints( GameObject obj )
	{
		if ( !obj.IsValid() || !TryUse( Rpc.Caller, out _ ) )
			return;

		const FindMode findMode = FindMode.EverythingInSelf | FindMode.InDescendants;

		var arms = obj.Components.GetAll<RopeJoint>( findMode );

		if ( !arms.Any() )
			return;

		foreach ( var th in arms.ToArray() )
			th.Destroy();
	}
}
