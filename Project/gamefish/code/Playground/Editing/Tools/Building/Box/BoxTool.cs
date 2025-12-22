namespace Playground;

public partial class BoxTool : EditorTool
{
	[Property]
	[Title( "Box" )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public PrefabFile BoxPrefab { get; set; }

	[Property]
	[Title( "Box Size" )]
	[Range( 1f, 1024f, clamped: false )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public float BoxSize { get; set; } = 50f;


	[Property]
	[Range( 0f, 100f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual float ScrollSensitivity { get; set; } = 10f;


	[Property]
	[Range( 0f, 2048f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public float Height { get; set; } = 32f;

	[Property]
	[Range( 0f, 2048f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public float HeightLimit { get; set; } = 1024f;

	[Property]
	[Range( 0f, 4096f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public float Distance { get; set; } = 256f;

	[Property]
	[Range( 0f, 4096f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public FloatRange DistanceRange { get; set; } = new( 16f, 1024f );

	[Property]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public bool UseHeight { get; set; } = true;



	/// <summary>
	/// If defined: the starting point for the shape.
	/// </summary>
	public Vector3? StartPoint { get; set; }


	public override void OnExit()
	{
		base.OnExit();

		StopShaping();
	}

	public override void FrameSimulate( in float deltaTime )
	{
		if ( !Mouse.Active )
			return;

		UpdateScroll( in deltaTime );
		UpdateUse( in deltaTime );

		UpdatePlace( in deltaTime );
		UpdateCancel( in deltaTime );
	}

	protected virtual void StopShaping()
	{
		StartPoint = null;
	}

	protected virtual void UpdateCancel( in float deltaTime )
	{
		if ( PressedSecondary )
			StopShaping();
	}

	protected virtual void UpdateUse( in float deltaTime )
	{
		if ( PressedUse )
			UseHeight = !UseHeight;
	}

	protected virtual void UpdateScroll( in float deltaTime )
	{
		var yScroll = Input.MouseWheel.y;

		if ( yScroll == 0f )
			return;

		var isPrecise = HoldingShift;

		if ( !isPrecise )
			yScroll *= ScrollSensitivity;

		if ( UseHeight )
		{
			Height = (Height + yScroll).Clamp( -HeightLimit, HeightLimit );

			if ( !isPrecise )
				Height = Height.Round();
		}
		else
		{
			Distance = (Distance + yScroll).Clamp( DistanceRange );
		}
	}

	protected virtual void UpdatePlace( in float deltaTime )
	{
		if ( !TryTrace( out var tr ) )
			return;

		// Point Height
		Vector3 point = tr.EndPosition;

		if ( UseHeight )
		{
			point = tr.EndPosition;

			if ( StartPoint is Vector3 firstPoint )
				point.z = firstPoint.z + Height;
		}
		else
		{
			var dist = tr.Distance.Min( Distance );
			point = tr.StartPosition + (tr.Direction * dist);
		}

		// Point Placement
		if ( PressedPrimary )
			TryPlacePoint( point );

		// Shape Helper Rendering
		var c1 = Color.White.WithAlpha( 0.4f );
		var c2 = Color.White.WithAlpha( 0.1f );

		this.DrawSphere( 2f, point, Color.Transparent, c1, global::Transform.Zero );

		if ( StartPoint is Vector3 start )
		{
			var bounds = BBox.FromPoints( [start, point] )
				.Grow( -0.1f ); // lessen z-fighting

			if ( bounds.Volume >= 2f )
			{
				this.DrawArrow(
					from: start, to: point,
					c: c1, len: 3f, w: 1f,
					tWorld: global::Transform.Zero
				);

				this.DrawBox( bounds, c1, c2, global::Transform.Zero );
			}
		}
	}

	protected virtual bool TryPlacePoint( in Vector3 point )
	{
		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( StartPoint is Vector3 start )
		{
			if ( !TryCreateBox( start, in point, out _ ) )
				return false;

			StopShaping();

			return true;
		}

		StartPoint = point;

		return true;
	}

	protected virtual bool TryCreateBox( in Vector3 a, in Vector3 b, out GameObject objBox )
	{
		var bounds = BBox.FromPoints( [a, b] );

		if ( bounds.Volume < 2f )
		{
			objBox = null;
			return false;
		}

		var center = bounds.Center;
		var scale = bounds.Size / BoxSize.Max( 1f );

		var tBox = new Transform( center, Rotation.Identity, scale );

		if ( !BoxPrefab.TrySpawn( tBox, out objBox ) )
			return false;

		objBox.NetworkSetup(
			cn: Connection.Local,
			orphanMode: NetworkOrphaned.ClearOwner,
			ownerTransfer: OwnerTransfer.Takeover,
			netMode: NetworkMode.Object,
			ignoreProxy: true
		);

		return true;
	}
}
