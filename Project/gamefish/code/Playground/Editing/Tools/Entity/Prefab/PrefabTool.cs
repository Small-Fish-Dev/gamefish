using System.Text.Json.Serialization;

namespace Playground;

public partial class PrefabTool : EditorTool
{
	[Property]
	[ToolOption]
	[Range( 0f, 4096f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual float Distance { get; set; } = 512;

	[Property]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public FloatRange DistanceRange { get; set; } = new( 16f, 1024f );

	[Property]
	[ToolOption]
	[Range( 0f, 100f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual float ScrollSensitivity { get; set; } = 20f;

	[Property]
	[ToolOption]
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

	public bool HasTarget { get; protected set; }
	public Transform TargetTransform { get; protected set; }

	public override void OnExit()
	{
		base.OnExit();

		HasTarget = false;
	}

	public override void FrameSimulate( in float deltaTime )
	{
		UpdateTarget( in deltaTime );

		if ( PressedPrimary )
			TryPlaceAtTarget( out _ );
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

	protected virtual void UpdateTarget( in float deltaTime )
	{
		HasTarget = false;

		if ( !Prefab.IsValid() || !PrefabBounds.HasValue )
			return;

		if ( !IsClientAllowed( Client.Local ) )
			return;

		if ( !TryTrace( out var tr ) )
			return;

		if ( !TrySetTarget( in tr ) )
			return;

		var c1 = Color.Black.WithAlpha( 0.5f );
		var c2 = Color.White.WithAlpha( 0.04f );

		if ( !HasTarget )
		{
			c1 = c1.WithAlphaMultiplied( 0.3f );
			c2 = c2.WithAlphaMultiplied( 0.3f );
		}

		var bounds = PrefabBounds.Value;
		this.DrawBox( bounds, c1, c2, tWorld: TargetTransform );
	}

	protected virtual bool TrySetTarget( in SceneTraceResult tr )
	{
		var pointDist = tr.Distance.Min( Distance );
		var hitPoint = tr.StartPosition + (tr.Direction * pointDist);

		HasTarget = true;
		TargetTransform = new Transform( hitPoint, GetPrefabRotation() );

		return true;
	}

	protected virtual bool TryPlaceAtTarget( out GameObject obj )
	{
		if ( !HasTarget )
		{
			obj = null;
			return false;
		}

		return TryPlace( TargetTransform, out obj );
	}

	protected virtual bool TryPlace( Transform tPrefab, out GameObject obj )
	{
		if ( !IsClientAllowed( Client.Local ) )
		{
			obj = null;
			return false;
		}

		if ( !Prefab.TrySpawn( in tPrefab, out obj ) )
			return false;

		obj.NetworkSetup(
			cn: Connection.Local,
			orphanMode: NetworkOrphaned.ClearOwner,
			ownerTransfer: OwnerTransfer.Takeover,
			netMode: NetworkMode.Object,
			ignoreProxy: true
		);

		return true;
	}
}
