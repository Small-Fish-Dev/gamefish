namespace Playground;

public partial class PhysicsWheelTool : JointTool
{
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual PhysicsWheelSettings JointSettings { get; set; }

	public override void FrameSimulate( in float deltaTime )
	{
		base.FrameSimulate( deltaTime );

		if ( PressedUse )
			TryToggleSteering();

		if ( PressedReload )
			TryToggleReverse();
	}

	protected bool TryGetTargetWheel( out PhysicsWheel w )
	{
		w = null;

		if ( !TryTrace( out var tr ) || !tr.Hit )
			return false;

		if ( !tr.GameObject.IsValid() )
			return false;

		return tr.GameObject.Components.TryGet<PhysicsWheel>( out w );
	}

	protected void TryToggleSteering()
	{
		if ( TryGetTargetWheel( out var w ) )
			w.RpcToggleSteering();
	}

	protected void TryToggleReverse()
	{
		if ( TryGetTargetWheel( out var w ) )
			w.RpcToggleReverse();
	}

	public override bool TryAddPointAtTarget()
		=> TryAttach( PointTarget );

	protected override DeviceAttachPoint GetAttachmentPoint( in SceneTraceResult tr )
	{
		// Pull it out a bit.
		var vPos = tr.EndPosition;
		vPos += tr.Normal * 1f;

		return new DeviceAttachPoint( tr, vPos );
	}

	public override bool TryAttach( in DeviceAttachPoint hitPoint, in DeviceAttachPoint _ )
		=> false;

	protected override bool TryAttach<TJoint>( in DeviceAttachPoint hitPoint, in DeviceAttachPoint _ )
		=> false;

	protected bool TryAttach( in DeviceAttachPoint hitPoint )
	{
		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( !hitPoint.IsValid() || !ValidAttachment( hitPoint ) )
			return false;

		// var tHit = hitPoint.Object.WorldTransform.WithOffset( hitPoint.Offset.Value );

		if ( !JointPrefab.TrySpawn( out var jointObj ) )
		{
			this.Warn( $"Couldn't find/spawn {typeof( PhysicsWheel )} prefab:[{JointPrefab}]!" );
			return false;
		}

		jointObj.NetworkInterpolation = false;

		if ( !jointObj.Components.TryGet<PhysicsWheel>( out var joint ) )
		{
			this.Warn( $"No {typeof( PhysicsWheel )} on obj:[{jointObj}]!" );
			jointObj.Destroy();
			return false;
		}

		joint.ParentPoint = hitPoint;

		joint.TrySetNetworkOwner( Connection.Local, allowProxy: true );

		if ( !joint.TryAttachTo( hitPoint ) )
		{
			this.Warn( $"Couldn't attach joint:[{joint}]!" );
			jointObj.Destroy();
			return false;
		}

		ClearPoints();

		return true;
	}

	protected override void DrawJointGizmos()
	{
	}

	protected override void DrawPointGizmo( in DeviceAttachPoint point )
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
