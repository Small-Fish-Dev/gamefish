namespace Playground;

public partial class PropellerTool : JointTool
{
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual PropellerSettings DeviceSettings { get; set; }

	public override bool TryAddPointAtTarget()
		=> TryAttach( PointTarget );

	public override bool TryAttach( in DeviceAttachPoint hitPoint, in DeviceAttachPoint _ )
		=> false;

	protected override bool TryAttach<TJoint>( in DeviceAttachPoint hitPoint, in DeviceAttachPoint _ )
		=> false;

	protected bool TryAttach( in DeviceAttachPoint hitPoint )
	{
		if ( !hitPoint.IsValid() || !ValidAttachment( hitPoint ) )
			return false;

		var tHit = hitPoint.Object.WorldTransform.WithOffset( hitPoint.Offset.Value );

		if ( !TrySpawnObject( JointPrefab, tWorld: tHit, out var e) )
		{
			this.Warn( $"Couldn't find/spawn {typeof( Propeller )} prefab:[{JointPrefab}]!" );
			return false;
		}

		var jointObj = e.GameObject;
		jointObj.NetworkInterpolation = false;

		if ( !jointObj.Components.TryGet<Propeller>( out var joint ) )
		{
			this.Warn( $"No {typeof( Propeller )} on obj:[{jointObj}]!" );
			jointObj.Destroy();
			return false;
		}

		joint.ParentPoint = hitPoint;
		joint.Settings = DeviceSettings;

		if ( !joint.TryAttachTo( hitPoint ) )
		{
			this.Warn( $"Couldn't attach joint:[{joint}]!" );
			jointObj.Destroy();
			return false;
		}

		ClearPoints();

		return true;
	}

	protected override void RenderJointHelpers()
	{
	}

	protected override void RenderJointPoint( in DeviceAttachPoint point )
	{
		if ( !point.Object.IsValid() || !point.Offset.HasValue )
			return;

		if ( !ValidAttachment( point ) )
			return;

		var c = Color.White.Desaturate( 0.4f ).WithAlpha( 0.3f );

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
	}

	public override bool TryClear( GameObject obj )
		=> false;

	protected override void RpcRemoveJoints( GameObject obj )
	{
	}
}
