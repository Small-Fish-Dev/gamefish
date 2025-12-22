namespace Playground;

public partial class PrefabTool : EditorTool
{
	[Property]
	[ToolOption]
	[Range( 0f, 4096f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public float Distance { get; set; } = 512;

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
				PrefabLocalBounds = SceneUtility.GetPrefabScene( _prefab )?.GetLocalBounds();
		}
	}

	protected PrefabFile _prefab;


	public bool HasTargetPosition { get; set; }

	public BBox? PrefabLocalBounds { get; protected set; }
	public Transform PrefabTransform { get; protected set; }


	public override void OnExit()
	{
		base.OnExit();

		HasTargetPosition = false;
	}

	public override void FrameSimulate( in float deltaTime )
	{
		if ( !Mouse.Active )
			return;

		UpdateScroll( in deltaTime );

		UpdatePlace( in deltaTime );
	}

	public override bool TryLeftClick()
	{
		if ( HasTargetPosition )
			TryPlacePrefab( PrefabTransform, out _ );

		return true;
	}

	public override bool TryMouseWheel( in Vector2 dir )
	{
		var scroll = dir.y != 0f ? -dir.y : dir.x;
		scroll *= ScrollSensitivity;

		Distance = (Distance + scroll).Clamp( DistanceRange );

		return true;
	}

	protected virtual void UpdateScroll( in float deltaTime )
	{
		var yScroll = Input.MouseWheel.y;

		if ( yScroll == 0f )
			return;
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
		if ( !PrefabLocalBounds.HasValue )
			return base.TryTrace( out tr );

		if ( !Editor.TryGetAimRay( Scene, out var ray ) )
			return base.TryTrace( out tr );

		var bounds = PrefabLocalBounds.Value;

		tr = Scene.Trace.Box( bounds.Extents, ray, Editor.TRACE_DISTANCE_DEFAULT )
			.IgnoreGameObjectHierarchy( Client.Local?.Pawn?.GameObject )
			.Rotated( GetPrefabRotation() )
			.Run();

		return true;
	}

	protected virtual void UpdatePlace( in float deltaTime )
	{
		HasTargetPosition = false;

		if ( !Prefab.IsValid() || !PrefabLocalBounds.HasValue )
			return;

		if ( !IsClientAllowed( Client.Local ) )
			return;

		if ( !TryTrace( out var tr ) )
			return;

		var traceHit = tr.Distance <= Distance;
		var pointDist = tr.Distance.Min( Distance );
		var point = tr.StartPosition + (tr.Direction * pointDist);

		var c = Color.White.Desaturate( 0.4f );
		var c1 = c.WithAlpha( 0.4f );
		var c2 = c.WithAlpha( 0.1f );

		if ( !traceHit )
		{
			c1 = c1.WithAlphaMultiplied( 0.3f );
			c2 = c2.WithAlphaMultiplied( 0.3f );
		}

		var bounds = PrefabLocalBounds.Value;

		HasTargetPosition = true;
		PrefabTransform = new( point, GetPrefabRotation() );

		var tBox = PrefabTransform;
		tBox.Scale *= 0.5f;

		this.DrawBox( bounds, c1, c2, tWorld: tBox );
	}

	protected virtual bool TryPlacePrefab( in Transform t, out GameObject objBoard )
	{
		if ( !IsClientAllowed( Client.Local ) )
		{
			objBoard = null;
			return false;
		}

		if ( !Prefab.TrySpawn( t, out objBoard ) )
			return false;

		objBoard.NetworkSetup(
			cn: Connection.Local,
			orphanMode: NetworkOrphaned.ClearOwner,
			ownerTransfer: OwnerTransfer.Takeover,
			netMode: NetworkMode.Object,
			ignoreProxy: true
		);

		return true;
	}
}
