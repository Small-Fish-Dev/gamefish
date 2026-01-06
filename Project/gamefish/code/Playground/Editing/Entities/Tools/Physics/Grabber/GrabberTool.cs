namespace Fishbox;

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

	public override bool ShowPointerTransform => false;

	public GrabberHand Hand { get; set; }

	protected RealTimeUntil GrabCooldown { get; set; }

	public bool IsGrabbing => Hand.IsValid() && Hand.BodyObject.IsValid();
	public float GrabDistance { get; set; }

	public bool IsRotating => IsGrabbing && HoldingUse && !Mouse.Active;

	public override bool HasScrollFocus => base.HasScrollFocus || IsGrabbing;
	public override bool HasAimingFocus => base.HasAimingFocus || IsRotating;

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

	protected override void RenderPointer()
	{
		base.RenderPointer();

		if ( Hand.IsValid() )
			RenderTransform( Hand.WorldTransform );
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

	protected override void OnSecondary( in SceneTraceResult tr )
	{
		base.OnSecondary( tr );

		TryToggleMotion( in tr );
	}

	public override bool TryMouseDrag( in Vector2 delta )
	{
		TryGrabTarget();
		return true;
	}

	public override void OnMouseUp( in MouseButtons mb )
	{
		base.OnMouseUp( mb );

		// TryDropHeld();
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

	protected virtual bool TryToggleMotion( in SceneTraceResult tr )
	{
		Rigidbody rb;

		// If we're holding something then only consider that.
		if ( Hand.IsValid() && Hand.BodyObject.IsValid() )
		{
			rb = Hand.BodyObject.Components.Get<Rigidbody>( FindMode.EnabledInSelf | FindMode.InAncestors );
			goto Freeze;
		}

		// Try to freeze what we're looking at.
		if ( !tr.GameObject.IsValid() )
			return false;

		rb = tr.GameObject.Components.Get<Rigidbody>( FindMode.EnabledInSelf | FindMode.InAncestors );

		Freeze:

		return TryToggleMotion( rb );
	}

	protected bool TryToggleMotion( Rigidbody rb )
	{
		if ( !rb.IsValid() || !rb.Network.Active )
			return false;

		RpcSetMotionEnabled( rb, !rb.MotionEnabled );

		return true;
	}

	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.SendImmediate )]
	protected void RpcSetMotionEnabled( Rigidbody rb, bool isEnabled )
	{
		if ( !rb.IsValid() || !rb.Active )
			return;

		if ( !TryUse( Rpc.Caller, out _ ) )
			return;

		rb.MotionEnabled = isEnabled;
	}

	protected virtual void UpdateGrab( in float deltaTime )
	{
		if ( HoldingPrimary )
			TryGrabTarget();
		else if ( ReleasedPrimary )
			TryDropHeld();

		if ( !IsGrabbing )
			return;

		if ( IsRotating && Hand.IsValid() )
		{
			var tHand = Hand.WorldTransform;
			var rInv = tHand.Rotation.Inverse;
			var angLook = Input.AnalogLook;

			var yaw = Rotation.FromAxis( rInv.Up, angLook.yaw );
			var pitch = Rotation.FromAxis( rInv * tHand.Right, -angLook.pitch );
			var roll = Rotation.FromAxis( rInv * tHand.Forward, angLook.roll );

			Hand.WorldRotation *= pitch * yaw * roll;
		}

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

		var obj = tr.GameObject;

		// TEMP: Can't ever grab pawns not explicitly ours.
		if ( Pawn.TryGet( obj, out var pawn ) )
		{
			if ( !pawn.IsOwner() )
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

		// If it has no collider then at least be a point entity.
		if ( !tr.Collider.IsValid() )
			if ( !tr.GameObject.Components.TryGet<EditorObject>( out var e ) )
				return false;

		// If it's not meant to move then don't.
		if ( tr.Collider.Static )
			return false;

		// Don't ever accidentally grab the map.
		if ( tr.GameObject.GetComponent<MapCollider>( includeDisabled: true ).IsValid() )
			return false;

		return true;
	}
}
