using System;
using System.Text.Json.Serialization;

namespace Playground;

public partial class PrefabTool : EditorTool
{
	[Property]
	[ToolSetting]
	[Range( 0f, 4096f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual float Distance { get; set; } = 512;

	[Property]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public FloatRange DistanceRange { get; set; } = new( 16f, 1024f );

	[Property]
	[ToolSetting]
	[Range( 0f, 100f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual float ScrollSensitivity { get; set; } = 20f;

	[Property]
	[ToolSetting]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public PrefabFile Prefab
	{
		get => _prefab;
		protected set
		{
			_prefab = value;

			if ( _prefab.IsValid() )
				PrefabBounds = SceneUtility.GetPrefabScene( _prefab )?.GetBounds();
		}
	}

	protected PrefabFile _prefab;

	[Title( "Prefab Bounds" )]
	[Property, ReadOnly, JsonIgnore]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public BBox InspectorPrefabBounds => PrefabBounds ?? default;

	public BBox? PrefabBounds { get; protected set; }

	public Transform TargetTransform { get; protected set; }

	public override void FrameSimulate( in float deltaTime )
	{
		FindTarget( in deltaTime );

		if ( PressedPrimary )
			TrySpawnAtTarget( out _ );
	}

	public override bool TryMouseWheel( in Vector2 dir )
	{
		var scroll = dir.y != 0f ? -dir.y : dir.x;
		scroll *= ScrollSensitivity;

		OnScroll( in scroll );

		return true;
	}

	protected virtual void OnScroll( in float scroll )
		=> Distance = (Distance + scroll).Clamp( DistanceRange );

	public virtual Rotation GetPrefabRotation()
	{
		Rotation rLook;

		if ( Client.Local?.Pawn?.IsValid() is true )
			rLook = Client.Local.Pawn.EyeRotation;
		else if ( Scene?.Camera?.IsValid() is true )
			rLook = Scene.Camera.WorldRotation;
		else
			rLook = Rotation.Identity;

		var dir = rLook.Forward.Flatten( isNormal: true );

		return Rotation.LookAt( dir, Vector3.Up );
	}

	public override bool TryTrace( out SceneTraceResult tr )
	{
		if ( !PrefabBounds.HasValue )
			return base.TryTrace( out tr );

		if ( !Editor.TryGetAimRay( Scene, out var ray ) )
			return base.TryTrace( out tr );

		var bounds = PrefabBounds.Value;
		// var extents = bounds.Extents;

		tr = Scene.Trace.Box( bounds, ray, Editor.TRACE_DISTANCE_DEFAULT )
			.IgnoreGameObjectHierarchy( Client.Local?.Pawn?.GameObject )
			.Rotated( GetPrefabRotation() )
			.Run();

		return true;
	}

	protected override void FindTarget( in float deltaTime, bool clearPrevious = true )
	{
		if ( !Prefab.IsValid() || !PrefabBounds.HasValue )
		{
			ClearTarget();
			return;
		}

		base.FindTarget( deltaTime, clearPrevious );

		var c1 = Color.Black.WithAlpha( 0.5f );
		var c2 = Color.White.WithAlpha( 0.04f );

		if ( !TargetObject.IsValid() )
		{
			c1 = c1.WithAlphaMultiplied( 0.3f );
			c2 = c2.WithAlphaMultiplied( 0.3f );
		}

		var bounds = PrefabBounds.Value;

		this.DrawBox( bounds, c1, c2, tWorld: TargetTransform );
	}

	public override bool TrySetTarget( in SceneTraceResult tr, Component target )
	{
		if ( !base.TrySetTarget( in tr, target ) )
			return false;

		var pointDist = tr.Distance.Min( Distance );
		var hitPoint = tr.StartPosition + (tr.Direction * pointDist);

		TargetTransform = new Transform( hitPoint, GetPrefabRotation() );

		return true;
	}

	protected virtual bool TrySpawnAtTarget( out EditorObject e )
	{
		var parent = Editor.FindIsland( TargetObject );

		if ( parent.IsValid() )
			return TrySpawnObject( Prefab, TargetTransform, parent, out e );

		return TrySpawnObject( Prefab, TargetTransform, out e );
	}
}
