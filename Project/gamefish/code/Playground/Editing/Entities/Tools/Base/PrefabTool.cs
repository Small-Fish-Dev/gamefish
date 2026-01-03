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

	/// <summary>
	/// The size of the prefab that will be spawned.
	/// </summary>
	public BBox? PrefabBounds { get; protected set; }

	public Transform TargetPrefabTransform { get; protected set; }

	protected override void OnPrimary( in SceneTraceResult tr )
		=> TrySpawnAtTarget( out _ );

	public override bool TryMouseWheel( in Vector2 dir )
	{
		var scroll = dir.y != 0f ? -dir.y : dir.x;
		scroll *= ScrollSensitivity;

		OnScroll( in scroll );

		return true;
	}

	protected virtual void OnScroll( in float scroll )
		=> Distance = (Distance + scroll).Clamp( DistanceRange );

	protected override void RenderHelpers()
	{
		base.RenderHelpers();

		var c1 = Color.Black.WithAlpha( 0.5f );
		var c2 = Color.White.WithAlpha( 0.04f );

		if ( !HasTarget )
		{
			c1 = c1.WithAlphaMultiplied( 0.3f );
			c2 = c2.WithAlphaMultiplied( 0.3f );
		}

		var bounds = PrefabBounds.Value;

		this.DrawBox( bounds, c1, c2, tWorld: TargetPrefabTransform );
	}

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
		{
			tr = default;
			return false;
		}

		return base.TryTrace( out tr );
	}

	protected override SceneTraceResult RunTrace( in Ray ray )
	{
		if ( !PrefabBounds.HasValue )
			return default;

		var bounds = PrefabBounds.Value;

		var tr = Scene.Trace.Box( bounds, ray, Editor.TRACE_DISTANCE_DEFAULT )
			.IgnoreGameObjectHierarchy( Client.Local?.Pawn?.GameObject )
			.Rotated( GetPrefabRotation() )
			.Run();

		return tr;
	}

	protected override void UpdateTarget( bool clearPrevious = true )
	{
		if ( !Prefab.IsValid() || !PrefabBounds.HasValue )
		{
			ClearTarget();
			return;
		}

		base.UpdateTarget( clearPrevious );
	}

	protected override bool TryGetPointer( in SceneTraceResult tr, out Transform tPointer )
	{
		if ( !base.TryGetPointer( tr, out tPointer ) )
			return false;

		var up = tPointer.Forward;
		var fwd = tPointer.Up;

		tPointer.Rotation = Rotation.LookAt( fwd, up );

		return true;
	}

	protected override void SetTarget( GameObject obj = null, Component target = null, in SceneTraceResult tr = default )
	{
		base.SetTarget( obj, target, in tr );

		if ( !TryGetPointer( in tr, out var tPointer ) )
		{
			HasTarget = false;
			return;
		}

		TargetPrefabTransform = tPointer;
	}

	protected virtual bool TrySpawnAtTarget( out EditorObject e, in bool withParent = true )
	{
		if ( !HasTarget )
		{
			e = null;
			return false;
		}

		var parent = Editor.FindIsland( TargetObject );

		if ( parent.IsValid() )
			return TrySpawnObject( Prefab, TargetPrefabTransform, parent, out e );

		return TrySpawnObject( Prefab, TargetPrefabTransform, out e );
	}
}
