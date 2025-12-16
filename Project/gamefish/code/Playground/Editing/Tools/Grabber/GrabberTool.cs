using System.ComponentModel;

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

	public bool IsRotating { get; set; }
	public Vector2? LastMousePosition { get; set; }

	/// <summary>
	/// Hack for buggy cursor/aim toggle shit.
	/// </summary>
	protected RealTimeSince? SinceRotated { get; set; }
	public bool IsLocked => SinceRotated.HasValue && SinceRotated.Value < 0.2f;

	public static bool GrabHeld => Input.Down( "Attack1" );
	public static bool RotationHeld => false; //Input.Down( "Use" );

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !IsRotating && Mouse.Active )
			LastMousePosition = Mouse.Position;

		if ( !IsSelected || !IsMenuOpen )
			TryDropHeld();

		// UpdateRotation( Time.Delta );

		DrawGrabberGizmos();
	}

	public override void OnExit()
	{
		base.OnExit();

		// Auto-drop on swap.
		TryDropHeld();
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

	protected virtual void UpdateRotation( in float deltaTime )
	{
		if ( !IsGrabbing )
			return;

		IsRotating = RotationHeld;

		Mouse.Visibility = IsRotating
			? MouseVisibility.Hidden
			: MouseVisibility.Visible;

		if ( IsRotating )
		{
			SinceRotated = 0.1f;

			var rInv = Hand.WorldRotation.Inverse;

			var rYaw = Rotation.FromAxis( rInv.Up, Input.AnalogLook.yaw );
			var rPitch = Rotation.FromPitch( Input.AnalogLook.pitch );

			Hand.WorldRotation *= rYaw;
			Hand.WorldRotation *= rPitch;

			Input.AnalogLook = Angles.Zero;

			if ( LastMousePosition.HasValue )
				Mouse.Position = LastMousePosition.Value;
		}
	}

	public override void FrameSimulate( in float deltaTime )
	{
		if ( !GrabHeld )
		{
			if ( !IsRotating )
				TryDropHeld();

			return;
		}

		if ( GrabHeld && !IsLocked )
			TryGrabTarget();

		if ( !IsGrabbing || IsRotating )
			return;

		if ( !Mouse.Active || !TryTrace( out var tr ) )
			return;

		var yScroll = Input.MouseWheel.y;
		var xScroll = Input.MouseWheel.x;

		if ( yScroll != 0f )
		{
			if ( Input.Keyboard.Down( "Shift" ) )
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

			var rAdd = Input.Keyboard.Down( "Shift" )
				? Rotation.FromRoll( xScroll * 10f )
				: Rotation.FromAxis( rInv.Up, xScroll * -10f );

			Hand.WorldRotation *= rAdd;
		}
	}

	protected virtual bool TryDropHeld()
	{
		IsRotating = false;

		if ( IsSelected && IsMenuOpen )
			Mouse.Visibility = MouseVisibility.Visible;

		if ( !Hand.IsValid() )
			return false;

		Hand.DestroyGameObject();
		Hand = null;

		return true;
	}

	protected virtual bool TryGrabTarget()
	{
		if ( Hand.IsValid() || IsRotating )
			return true;

		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( !TryTrace( out var tr ) || !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		if ( !CanGrab( Client.Local, in tr ) )
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

	public virtual bool CanGrab( Client cl, in SceneTraceResult tr )
	{
		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		// Don't accidentally grab the map.
		if ( tr.GameObject.GetComponent<MapCollider>( includeDisabled: true ).IsValid() )
			return false;

		if ( tr.Collider.IsValid() && tr.Collider.Static )
			return false;

		return true;
	}
}
