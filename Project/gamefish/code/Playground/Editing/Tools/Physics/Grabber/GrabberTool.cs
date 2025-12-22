namespace Playground;

public partial class GrabberTool : EditorTool
{
	[Property]
	[Title( "Hand" )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public PrefabFile HandPrefab { get; set; }

	[Property]
	[Range( 0f, 100f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual float ScrollSensitivity { get; set; } = 30f;

	public GrabberHand Hand { get; set; }

	public bool IsGrabbing => Hand.IsValid() && Hand.BodyObject.IsValid();
	public float GrabDistance { get; set; }

	protected TimeUntil GrabCooldown { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !IsSelected )
			TryDropHeld();

		DrawGrabberGizmos();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		TryDropHeld();
	}

	public override void OnExit()
	{
		base.OnExit();

		// Auto-drop on swap.
		TryDropHeld();
	}

	public override void FrameSimulate( in float deltaTime )
	{
		base.FrameSimulate( deltaTime );

		UpdateGrab( in deltaTime );
	}

	public override void OnCursorToggled( in bool isOpen )
	{
		base.OnCursorToggled( isOpen );

		// Prevent bugging out from snapping between cursor/view angles.
		TryDropHeld();
	}

	public override bool TryLeftClick()
	{
		TryGrabTarget();
		return true;
	}

	public override bool TryRightClick()
	{
		TryFreeze();
		return true;
	}

	public override bool TryMouseDrag( in Vector2 delta )
	{
		TryGrabTarget();
		return true;
	}

	public override void OnMouseDragEnd()
	{
		base.OnMouseDragEnd();

		TryDropHeld();
	}

	public override void OnMouseUp( in MouseButtons mb )
	{
		base.OnMouseUp( mb );

		TryDropHeld();
	}

	public override bool TryMouseWheel( in Vector2 dir )
	{
		if ( !Hand.IsValid() )
			return false;

		var scrollDist = -dir.y * ScrollSensitivity;
		GrabDistance = (GrabDistance + scrollDist).Positive();

		return true;
	}

	protected virtual void DrawGrabberGizmos()
	{
		if ( !Hand.IsValid() )
			return;

		var bodyObj = Hand.BodyObject;
		var joint = Hand.Joint;

		if ( !bodyObj.IsValid() || !joint.IsValid() )
			return;

		if ( !joint.Body1.IsValid() || !joint.Body2.IsValid() )
			return;

		var tPoint1 = joint.WorldPosition; //joint.Point1.Transform.Position;
		var tPoint2 = joint.Point2.Transform.Position;

		var c = Color.White.WithAlpha( 0.3f );

		this.DrawArrow(
			from: tPoint1, to: tPoint2,
			c: c, len: 3f, w: 1f,
			tWorld: global::Transform.Zero
		);
	}

	protected virtual bool TryFreeze()
	{
		// Prefer to target what we're holding first.
		PhysicsBody body = Hand?.Joint?.Body2;

		// Then point at something.
		if ( !body.IsValid() )
		{
			if ( !TryTrace( out var tr ) || !CanTarget( Client.Local, in tr ) )
				return false;

			body = tr.Body;
		}

		if ( !body.IsValid() )
			return false;

		// Take network control if possible, otherwise it may not work.
		if ( body.Component.IsValid() && body.Component.IsProxy )
			if ( !body.Component.Network.TakeOwnership() )
				return false;

		body.MotionEnabled = !body.MotionEnabled;

		return true;
	}

	protected virtual void UpdateGrab( in float deltaTime )
	{
		if ( HoldingPrimary )
			TryGrabTarget();

		if ( ReleasedPrimary )
			TryDropHeld();

		if ( !IsGrabbing )
			return;

		if ( !TryTrace( out var tr ) )
			return;

		Hand.WorldPosition = tr.StartPosition + tr.Direction * GrabDistance;
	}

	protected virtual bool TryDropHeld()
	{
		if ( !Hand.IsValid() )
			return false;

		Hand.DestroyGameObject();
		Hand = null;

		GrabCooldown = 0.2f;

		return true;
	}

	protected virtual bool TryGrabTarget()
	{
		if ( Hand.IsValid() )
			return true;

		if ( !GrabCooldown )
			return false;

		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( !TryTrace( out var tr ) || !tr.GameObject.IsValid() )
			return false;

		if ( !CanTarget( Client.Local, in tr ) )
			return false;

		// TEMP: Can't grab frozen treats.
		if ( tr.Body.IsValid() && !tr.Body.MotionEnabled )
			return false;

		var obj = tr.GameObject;

		// TEMP: Can't ever grab unowned pawns.
		if ( Pawn.TryGet( obj, out var pawn ) )
		{
			if ( pawn.IsProxy )
				return false;

			obj = pawn.GameObject;
		}

		if ( obj.IsProxy && obj.Network.OwnerTransfer is OwnerTransfer.Takeover )
			if ( !obj.Network.TakeOwnership() )
				return false;

		GrabDistance = tr.Distance;

		var hitPos = tr.HitPosition;
		var rAim = Rotation.LookAt( tr.Direction );

		if ( !Hand.IsValid() )
		{
			if ( !HandPrefab.TrySpawn( in hitPos, in rAim, out var handObj ) )
				return false;

			handObj.NetworkInterpolation = false;

			if ( !handObj.Components.TryGet<GrabberHand>( out var hand ) )
			{
				handObj.Destroy();
				return false;
			}

			Hand = hand;
		}

		if ( !Hand.IsValid() )
			return false;

		Hand.WorldPosition = hitPos;
		Hand.WorldRotation = rAim;
		Hand.Transform.ClearInterpolation();

		Hand.BodyObject = tr.GameObject;

		return true;
	}

	public virtual bool CanTarget( Client cl, in SceneTraceResult tr )
	{
		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		if ( tr.Collider.IsValid() && tr.Collider.Static )
			return false;

		// Don't ever accidentally grab the map.
		if ( tr.GameObject.GetComponent<MapCollider>( includeDisabled: true ).IsValid() )
			return false;

		return true;
	}
}
