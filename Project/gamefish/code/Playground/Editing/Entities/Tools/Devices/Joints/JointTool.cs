using System.Text.Json.Serialization;

namespace Fishbox;

public abstract class JointTool : EditorTool
{
	/// <summary>
	/// Should the objects be snapped together? (WIP)
	/// </summary>
	[Title( "Tele-Snap" )]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual bool PointSnapping { get; set; }

	[Property]
	[Title( "Joint" )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public PrefabFile JointPrefab { get; set; }

	[Property]
	[Title( "Attach" )]
	[Feature( EDITOR ), Group( SOUNDS ), Order( SOUNDS_ORDER )]
	public SoundEvent AttachmentSound { get; set; }

	[InlineEditor]
	[Property, JsonIgnore, ReadOnly]
	[ShowIf( nameof( InGame ), true )]
	[Feature( EDITOR ), Group( DEBUG )]
	public DeviceAttachPoint PointTarget { get; set; }

	public DeviceAttachPoint Point1 { get; set; }
	public DeviceAttachPoint Point2 { get; set; }

	protected override void Clear()
	{
		base.Clear();

		ClearPoints();
	}

	protected virtual void ClearPoints()
	{
		Point1 = default;
		Point2 = default;
	}

	protected override void ClearTarget()
	{
		base.ClearTarget();

		PointTarget = default;
	}

	protected override void OnPrimary( in SceneTraceResult tr )
	{
		base.OnPrimary( tr );

		TryAddPointAtTarget();
	}

	protected override void SetTarget( GameObject obj = null, Component target = null, in SceneTraceResult tr = default )
	{
		base.SetTarget( obj, target, tr );

		if ( TryGetAttachPoint( obj, target, in tr, out var point ) )
			PointTarget = point;
	}

	protected virtual bool TryGetAttachPoint( GameObject obj, Component target, in SceneTraceResult tr, out DeviceAttachPoint point )
	{
		if ( !obj.IsValid() || !TryGetPointer( in tr, out var tPointer ) )
		{
			point = default;
			return false;
		}

		var tObj = obj.WorldTransform;
		var tOffset = tObj.ToLocal( tPointer );

		point = new( obj, tOffset );

		return point.IsValid;
	}

	protected override void RenderHelpers()
	{
		base.RenderHelpers();

		RenderJointHelpers();
	}

	protected virtual void RenderJointHelpers()
	{
		RenderJointPoint( Point1 );
		RenderJointPoint( Point2 );
	}

	protected virtual void RenderJointPoint( in DeviceAttachPoint point )
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

	public virtual bool TryAddPointAtTarget()
		=> TryAddPoint( PointTarget );

	protected virtual bool TryAddPoint( in DeviceAttachPoint point )
	{
		if ( !point.IsValid() )
			return false;

		if ( Point1.IsValid() )
		{
			Point2 = point;
			TryAttach( Point1, Point2 );

			return true;
		}

		Point1 = point;

		return true;
	}

	protected abstract void RpcRemoveJoints( GameObject obj );

	/// <summary>
	/// Lets you tell the tool what type of joint it should clear.
	/// </summary>
	/// <returns> If any of its joint could be removed. </returns>
	public abstract bool TryClear( GameObject obj );

	protected virtual bool TryClear<TJoint>( GameObject obj )
		where TJoint : JointDevice
	{
		if ( !obj.IsValid() )
			return false;

		var joints = obj.Components.GetAll<TJoint>( FindMode.EverythingInSelfAndDescendants );

		if ( !joints.Any( th => th.IsValid() ) )
			return false;

		RpcRemoveJoints( obj );
		return true;
	}

	public virtual bool ValidTarget( Client cl, in SceneTraceResult tr )
	{
		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		if ( Pawn.TryGet( tr.GameObject, out _ ) )
			return false;

		const FindMode findMode = FindMode.EnabledInSelf | FindMode.InAncestors;

		if ( !tr.GameObject.Components.TryGet<Rigidbody>( out var _, findMode ) )
			return false;

		return true;
	}

	public virtual bool ValidAttachment( in DeviceAttachPoint point )
	{
		if ( Pawn.TryGet( point.Object, out _ ) )
			return false;

		return true;
	}

	public abstract bool TryAttach( in DeviceAttachPoint point1, in DeviceAttachPoint point2 );

	protected virtual bool TryAttach<TJoint>( in DeviceAttachPoint point1, in DeviceAttachPoint point2 )
		where TJoint : JointDevice
	{
		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( !point1.IsValid() || !ValidAttachment( point1 ) )
			return false;

		if ( !point2.IsValid() || !ValidAttachment( point2 ) )
			return false;

		var tWorld = (point1.Object ?? point1.Object)?.WorldTransform
			?? global::Transform.Zero;

		if ( !TrySpawnObject( JointPrefab, tWorld, e: out var e ) )
		{
			this.Warn( $"Couldn't find/spawn {typeof( TJoint )} prefab:[{JointPrefab}]!" );
			return false;
		}

		var jointObj = e.GameObject;
		jointObj.NetworkInterpolation = false;

		if ( !jointObj.Components.TryGet<TJoint>( out var joint ) )
		{
			this.Warn( $"No {typeof( TJoint )} on obj:[{jointObj}]!" );
			jointObj.Destroy();
			return false;
		}

		joint.LocalPoint = point1;
		joint.TargetPoint = point2;

		ApplySettings( joint );

		joint.TrySetNetworkOwner( Connection.Local, allowProxy: true );

		if ( !joint.TryAttachTo( point1, point2 ) )
		{
			this.Warn( $"Couldn't attach joint:[{joint}]!" );
			jointObj.Destroy();
			return false;
		}

		ClearPoints();

		return true;
	}

	public virtual void ApplySettings<TJoint>( TJoint joint )
		where TJoint : JointDevice
	{
	}
}