using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Playground;

public partial class WiringTool : EditorTool
{
	public Entity Target { get; set; }
	public Vector3? TargetLocalPosition { get; set; }

	public override void OnExit()
	{
		base.OnExit();

		ClearTarget();
	}

	public override void FrameSimulate( in float deltaTime )
	{
		if ( !Mouse.Active )
			return;

		UpdateTarget( in deltaTime );

		UpdatePlacing();
		UpdateBreaking();
	}

	public override bool TryLeftClick()
		=> true;

	/*
	public override bool TryTrace( out SceneTraceResult tr )
	{
		if ( !Editor.TryGetAimRay( Scene, out var ray ) )
			return base.TryTrace( out tr );

		tr = Scene.Trace.Box( bounds.Extents, ray, Editor.TRACE_DISTANCE_DEFAULT )
			.IgnoreGameObjectHierarchy( Client.Local?.Pawn?.GameObject )
			.Rotated( GetPrefabRotation() )
			.Run();

		return true;
	}
	*/

	protected void UpdatePlacing()
	{
		if ( !PressedPrimary )
			return;

	}

	protected void UpdateBreaking()
	{
		if ( !PressedReload )
			return;

		if ( !TryTrace( out var tr ) || !tr.Hit )
			return;

		if ( !Target.IsValid() )
			return;
	}

	protected virtual void UpdateTarget( in float deltaTime )
	{
		ClearTarget();

		if ( !IsClientAllowed( Client.Local ) )
			return;

		if ( !TryTrace( out var tr ) )
			return;

		if ( !TryFindWireable( tr.GameObject, out var ent ) )
			return;

		if ( !TrySetTarget( ent, in tr ) )
			return;

		this.Log( $"Set target:[{ent}]" );

		var cSphere = Color.White.WithAlpha( 0.4f );

		this.DrawSphere( 2f, tr.HitPosition, Color.Transparent, cSphere, global::Transform.Zero );
	}

	public virtual bool TryFindWireable( GameObject obj, out Entity ent )
	{
		ent = null;

		if ( !obj.IsValid() || !obj.Active )
			return false;

		const FindMode findMode = FindMode.EnabledInSelf
			| FindMode.InDescendants
			| FindMode.InAncestors;

		ent = obj.Components.GetAll<Entity>( findMode )
		   .FirstOrDefault( ent => ent is IWired );

		// this.Log( ent );

		return ent.IsValid();
	}

	public bool TrySetTarget( Entity ent, in SceneTraceResult tr )
	{
		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		if ( ent is not IWired )
			return false;

		Target = ent;

		TargetLocalPosition = ent.WorldTransform.PointToLocal( tr.HitPosition );

		return true;
	}

	public bool IsValidTarget( Entity ent )
	{
		if ( !ent.IsValid() || ent is not IWired )
			return false;

		return true;
	}

	protected void ClearTarget()
	{
		Target = null;
		TargetLocalPosition = null;
	}

	[Rpc.Host]
	protected void RpcHostRequestWire()
	{
		if ( !TryUse( Rpc.Caller, out _ ) )
			return;

	}

	[Rpc.Host]
	protected void RpcHostRequestClear( Entity ent )
	{
		if ( !TryUse( Rpc.Caller, out _ ) )
			return;

	}
}
