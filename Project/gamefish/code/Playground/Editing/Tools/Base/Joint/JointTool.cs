namespace Playground;

public abstract class JointTool : EditorTool
{
	[Property]
	[Title( "Joint" )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public PrefabFile JointPrefab { get; set; }

	[Property]
	[Title( "Attach" )]
	[Feature( EDITOR ), Group( SOUNDS ), Order( SOUNDS_ORDER )]
	public virtual SoundEvent AttachingSound { get; set; }

	public ToolAttachPoint PointTarget { get; set; }

	public ToolAttachPoint Point1 { get; set; }
	public ToolAttachPoint Point2 { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !IsSelected )
			return;

		DrawJointGizmos();
	}

	public override void FrameSimulate( in float deltaTime )
	{
		if ( !IsClientAllowed( Client.Local ) )
			return;

		if ( !TryTrace( out var tr ) )
			return;

		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return;

		if ( PressedReload )
		{
			if ( !TryClear( tr.GameObject ) )
				ClearPoints();
		}

		PointTarget = new( tr );
	}

	public override bool TryLeftClick()
		=> TryAddPoint( PointTarget );

	protected virtual bool TryAddPoint( in ToolAttachPoint point )
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

	protected virtual void ClearPoints()
	{
		Point1 = default;
		Point2 = default;
	}

	protected abstract void RpcRemoveJoints( GameObject obj );

	/// <summary>
	/// Lets you tell the tool what type of joint it should clear.
	/// </summary>
	/// <returns> If any of its joint could be removed. </returns>
	public abstract bool TryClear( GameObject obj );

	protected virtual bool TryClear<TJoint>( GameObject obj )
		where TJoint : JointEntity
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

		return true;
	}

	public virtual bool ValidAttachment( in ToolAttachPoint point )
	{
		if ( Pawn.TryGet( point.Object, out _ ) )
			return false;

		return true;
	}

	public abstract bool TryAttach( in ToolAttachPoint point1, in ToolAttachPoint point2 );

	protected virtual bool TryAttach<TJoint>( in ToolAttachPoint point1, in ToolAttachPoint point2 )
		where TJoint : JointEntity
	{
		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( !point1.IsValid() || !ValidAttachment( point1 ) )
			return false;

		if ( !point2.IsValid() || !ValidAttachment( point2 ) )
			return false;

		if ( !JointPrefab.TrySpawn( out var jointObj ) )
		{
			this.Warn( $"Couldn't find/spawn {typeof( TJoint )} prefab:[{JointPrefab}]!" );
			return false;
		}

		jointObj.NetworkInterpolation = false;

		if ( !jointObj.Components.TryGet<TJoint>( out var joint ) )
		{
			this.Warn( $"No {typeof( TJoint )} on obj:[{jointObj}]!" );
			jointObj.Destroy();
			return false;
		}

		joint.ParentPoint = point1;
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
		where TJoint : JointEntity
	{
	}

	protected virtual void DrawJointGizmos()
	{
		DrawPointGizmo( Point1 );
		DrawPointGizmo( Point2 );

		DrawPointGizmo( PointTarget );
	}

	protected virtual void DrawPointGizmo( in ToolAttachPoint point )
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
}