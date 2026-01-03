namespace Playground;

public partial class PhysicsWheelTool : JointTool
{
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual PhysicsWheelSettings JointSettings { get; set; }

	protected override void OnUse( in SceneTraceResult tr )
	{
		TryToggleSteering();
	}

	protected override void OnReload( in SceneTraceResult tr )
	{
		TryToggleReverse();
	}

	protected override bool TryGetPointer( in SceneTraceResult tr, out Transform tPointer )
	{
		if ( !tr.Hit || !base.TryGetPointer( tr, out tPointer ) )
		{
			tPointer = default;
			return false;
		}

		if ( tr.GameObject.IsValid() )
		{
			var rObj = tr.GameObject.WorldRotation;
			var vNearestUp = rObj.ClosestAxis( LocalUp );
			tPointer.Rotation = Rotation.LookAt( tr.Normal, vNearestUp );
		}

		return true;
	}

	protected bool TryGetTargetWheel( out PhysicsWheel w )
	{
		w = null;

		if ( !HasTarget || !TargetTrace.GameObject.IsValid() )
			return false;

		return TargetTrace.GameObject.Components.TryGet( out w );
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

	protected override bool TryAddPoint( in DeviceAttachPoint point )
		=> false;

	public override bool TryAttach( in DeviceAttachPoint hitPoint, in DeviceAttachPoint _ )
		=> false;

	protected override bool TryAttach<TJoint>( in DeviceAttachPoint hitPoint, in DeviceAttachPoint _ )
		=> false;

	protected bool TryAttach( DeviceAttachPoint point )
	{
		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( !point.IsValid() || !ValidAttachment( point ) )
			return false;

		if ( point.Offset is not Offset offset )
			return false;

		var tHit = point.Object.WorldTransform.WithOffset( offset );

		if ( !TrySpawnObject( JointPrefab, tHit, out var e ) )
		{
			this.Warn( $"Couldn't find/spawn {typeof( PhysicsWheel )} prefab:[{JointPrefab}]!" );
			return false;
		}

		var jointObj = e.GameObject;
		jointObj.NetworkInterpolation = false;

		if ( !jointObj.Components.TryGet<PhysicsWheel>( out var joint ) )
		{
			this.Warn( $"No {typeof( PhysicsWheel )} on obj:[{jointObj}]!" );
			jointObj.Destroy();
			return false;
		}

		joint.ParentPoint = point;

		joint.TrySetNetworkOwner( Connection.Local, allowProxy: true );

		if ( !joint.TryAttachTo( point ) )
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

	public override void ApplySettings<TJoint>( TJoint joint )
	{
	}

	public override bool TryClear( GameObject obj )
		=> false;

	protected override void RpcRemoveJoints( GameObject obj )
	{
	}
}
