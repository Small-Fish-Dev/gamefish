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

	protected TimeUntil DragCooldown { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !IsSelected || !IsMenuOpen )
			TryDropHeld();

		// UpdateRotation( Time.Delta );

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

	public override void OnMouseDragEnd()
	{
		base.OnMouseDragEnd();

		TryDropHeld();
	}

	public override void OnLeftClick()
	{
		base.OnLeftClick();

		TryGrabTarget();
	}

	public override void OnMouseDrag( in Vector2 delta )
	{
		base.OnMouseDrag( in delta );

		TryGrabTarget();
	}

	public override void OnRightClick()
	{
		base.OnRightClick();

		TryFreeze();
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
		if ( !IsGrabbing )
			return;

		if ( !Mouse.Active || !TryTrace( out var tr ) )
			return;

		var yScroll = Input.MouseWheel.y;
		var xScroll = Input.MouseWheel.x;

		if ( yScroll != 0f )
		{
			if ( HoldingShift )
			{
				var pitch = Rotation.FromPitch( yScroll * -5f );
				Hand.WorldRotation *= pitch;
			}
			else
			{
				var scrollDist = yScroll * ScrollSensitivity;
				GrabDistance = (GrabDistance + scrollDist).Positive();
			}
		}

		Hand.WorldPosition = tr.StartPosition + tr.Direction * GrabDistance;

		if ( xScroll != 0f )
		{
			var rInv = Hand.WorldRotation.Inverse;

			var rAdd = HoldingShift
				? Rotation.FromRoll( xScroll * 10f )
				: Rotation.FromAxis( rInv.Up, xScroll * -10f );

			Hand.WorldRotation *= rAdd;
		}
	}

	protected virtual bool TryDropHeld()
	{
		if ( IsSelected && IsMenuOpen )
			Mouse.Visibility = MouseVisibility.Visible;

		if ( !Hand.IsValid() )
			return false;

		Hand.DestroyGameObject();
		Hand = null;

		DragCooldown = 0.2f;

		return true;
	}

	protected virtual bool TryGrabTarget( in bool isDragging = false )
	{
		if ( Hand.IsValid() )
			return true;

		if ( isDragging && DragCooldown )
			return true;

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
