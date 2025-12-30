using System.Runtime.CompilerServices;

namespace Playground;

public partial class WiringTool : EditorTool
{
	public Vector3? TargetLocalPosition { get; protected set; }

	public (Component Parent, Vector3 LocalPos) Point1 { get; protected set; }
	public (Component Parent, Vector3 LocalPos) Point2 { get; protected set; }

	public override void OnExit()
	{
		base.OnExit();

		Clear();
	}

	public override void FrameSimulate( in float deltaTime )
	{
		base.FrameSimulate( in deltaTime );

		if ( !IsMenuOpen )
		{
			if ( TargetComponent is Device device )
				device.RenderHelpers();
		}
	}

	protected override void RenderHelpers()
	{
		base.RenderHelpers();

		RenderWireHelper();
	}

	protected void RenderWireHelper()
	{
		if ( !TryGetCursorPosition( out var cursorPos ) )
			return;

		var c1 = Point1.Parent;
		var c2 = TargetComponent;

		if ( !c1.IsValid() || !c2.IsValid() )
			return;

		var startPoint = c1.WorldTransform.PointToWorld( Point1.LocalPos );

		var c = Color.Black.WithAlpha( 1.6f );

		if ( !CanWire( c1, c2 ) )
			c = c.WithAlpha( 0.3f );

		this.DrawArrow(
			from: startPoint, to: cursorPos,
			c: c, len: 7f, w: 2f, th: 4f,
			tWorld: global::Transform.Zero
		);
	}

	protected override void OnPrimary( in SceneTraceResult tr )
	{
		if ( !IsValidTarget( TargetComponent ) )
			return;

		if ( TargetLocalPosition is not Vector3 localPos )
			return;

		if ( !Point1.Parent.IsValid() )
		{
			Point1 = (TargetComponent, localPos);
			return;
		}

		if ( CanWire( Point1.Parent, TargetComponent ) )
			Point2 = (TargetComponent, localPos);

		RpcHostRequestWire( Point1.Parent, Point2.Parent, Point1.LocalPos, Point2.LocalPos );

		Clear();
	}

	protected override void OnReload( in SceneTraceResult tr )
	{
		base.OnReload( tr );

		if ( TargetComponent is Device device )
			RpcHostRequestClear( device );
	}

	protected override void Clear()
	{
		base.Clear();

		ClearPoints();
	}

	protected void ClearPoints()
	{
		Point1 = default;
		Point2 = default;
	}

	protected override void ClearTarget()
	{
		base.ClearTarget();

		TargetLocalPosition = null;
	}

	public override bool IsValidTarget( Component c )
		=> base.IsValidTarget( c ) && c is IWire;

	public override bool TrySetTarget( in SceneTraceResult tr, Component target )
	{
		if ( !base.TrySetTarget( in tr, target ) )
			return false;

		TargetLocalPosition = target.WorldTransform.PointToLocal( tr.HitPosition );

		return true;
	}

	public override bool TryGetTarget( in SceneTraceResult tr, out Component target )
	{
		target = null;

		if ( !tr.Collider.IsValid() || !tr.Collider.GameObject.IsValid() )
			return false;

		var obj = tr.Collider.GameObject;

		const FindMode findSelf = FindMode.EnabledInSelf | FindMode.InDescendants;
		const FindMode findAbove = FindMode.Enabled | FindMode.InAncestors;

		if ( obj.Components.TryGet( out IWire wire, findSelf )
			|| obj.Components.TryGet( out wire, findAbove ) )
		{
			target = wire as Component;
			return true;
		}

		return base.TryGetTarget( tr, out target );
	}

	public static bool CanWire( Component c1, Component c2 )
	{
		// Can't target itself.
		if ( c1 == c2 )
			return false;

		// They both must support wiring.
		if ( c1 is not IWire w1 || c2 is not IWire w2 )
			return false;

		// One of them must be a device.
		if ( w1 is not Device && w2 is not Device )
			return false;

		if ( w1?.CanWire( w2 ) is not true || w2?.CanWire( w1 ) is not true )
			return false;

		return true;
	}

	[Rpc.Host]
	protected void RpcHostRequestWire( Component c1, Component c2, Vector3 localPos1, Vector3 localPos2 )
	{
		if ( !TryUse( Rpc.Caller, out _ ) )
			return;

		if ( !IsValidTarget( c1 ) || !IsValidTarget( c2 ) )
			return;

		if ( c1 is not Entity ent1 || c2 is not Entity ent2 )
			return;

		if ( ent1 is Device device1 )
			device1.TryWire( ent2, localPos2 );

		if ( ent2 is Device device2 )
			device2.TryWire( ent1, localPos1 );
	}

	[Rpc.Host]
	protected void RpcHostRequestClear( Device device )
	{
		if ( !device.IsValid() )
			return;

		if ( !TryUse( Rpc.Caller, out _ ) )
			return;

		device.Wires?.Clear();
	}
}
