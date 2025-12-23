namespace Playground;

public partial class ArmTool : JointTool
{
	/// <summary>
	/// The key you press to activate the thruster you're placing.
	/// </summary>
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual ArmSettings ArmSettings { get; set; }

	public override bool TryAttach( in ToolAttachPoint point1, in ToolAttachPoint point2 )
		=> TryAttach<Arm>( in point1, in point2 );

	protected override void DrawPointGizmo( in ToolAttachPoint point )
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
			c: c, len: 0.1f, w: 5f, th: 4f,
			tWorld: global::Transform.Zero
		);
	}

	public override void ApplySettings<TJoint>( TJoint joint )
	{
		if ( joint is not Arm arm )
			return;

		arm.Settings = ArmSettings;
	}

	public override bool TryClear( GameObject obj )
		=> TryClear<Arm>( obj );

	[Rpc.Host]
	protected override void RpcRemoveJoints( GameObject obj )
	{
		if ( !obj.IsValid() || !TryUse( Rpc.Caller, out _ ) )
			return;

		const FindMode findMode = FindMode.EverythingInSelf | FindMode.InDescendants;

		var arms = obj.Components.GetAll<Arm>( findMode );

		if ( !arms.Any() )
			return;

		foreach ( var th in arms.ToArray() )
			th.Destroy();
	}
}
