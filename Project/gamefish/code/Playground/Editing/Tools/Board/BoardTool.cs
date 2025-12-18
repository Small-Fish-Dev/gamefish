namespace Playground;

public partial class BoardTool : EditorTool
{
	[Property]
	[Title( "Board" )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public PrefabFile BoardPrefab { get; set; }

	[Property]
	[Range( 1f, 1024f, clamped: false )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public float BoxSize { get; set; } = 50f;


	[Property]
	[Range( 0f, 4096f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public float Distance { get; set; } = 256f;

	[Property]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public FloatRange DistanceRange { get; set; } = new( 16f, 1024f );

	[Property]
	[Range( 0f, 100f )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public virtual float ScrollSensitivity { get; set; } = 20f;

	[Property]
	[Title( "Board Width" )]
	[Range( 1f, 32f, clamped: false )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public float BoardWidth { get; set; } = 20f;

	[Property]
	[Title( "Board Height" )]
	[Range( 1f, 16f, clamped: false )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public float BoardHeight { get; set; } = 5f;


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

	protected virtual void UpdateScroll( in float deltaTime )
	{
		var yScroll = Input.MouseWheel.y;

		if ( yScroll == 0f )
			return;

		if ( !HoldingShift )
			yScroll *= ScrollSensitivity;

		Distance = (Distance + yScroll).Clamp( DistanceRange );
	}

	protected virtual void UpdatePlace( in float deltaTime )
	{
		if ( !IsClientAllowed( Client.Local ) )
			return;

		if ( !TryTrace( out var tr ) )
			return;

		var hasHit = tr.Distance <= Distance;
		var pointDist = tr.Distance.Min( Distance );
		var point = tr.StartPosition + (tr.Direction * pointDist);

		var validColor = Color.White.Desaturate( 0.4f );
		var c1 = validColor.WithAlpha( 0.4f );
		var c2 = validColor.WithAlpha( 0.1f );

		if ( hasHit )
		{
			point += tr.Normal * ((BoardHeight / 2f) + 0.1f);
		}
		else
		{
			c1 = c1.WithAlphaMultiplied( 0.3f );
			c2 = c2.WithAlphaMultiplied( 0.3f );
		}

		this.DrawSphere( 2f, point, Color.Transparent, c1, global::Transform.Zero );

		if ( StartPoint is Vector3 start )
		{
			var dist = start.Distance( point );
			var dir = start.Direction( point );
			var center = start.LerpTo( point, 0.5f );

			var length = dist;
			var scale = new Vector3( length, BoardWidth, BoardHeight );
			var rDir = Rotation.LookAt( dir, tr.Normal );

			var bounds = BBox.FromPositionAndSize( Vector3.Zero, Vector3.One );
			var tBox = new Transform( center, rDir, scale );

			var isError = dist < BoardWidth;

			if ( isError && !hasHit )
			{
				var errorColor = Color.Red.Desaturate( 0.4f );
				var c1Error = errorColor.WithAlpha( 0.4f );
				var c2Error = errorColor.WithAlpha( 0.2f );

				c1 = c1Error;
				c2 = c2Error;
			}

			this.DrawBox( bounds, c1, c2, tBox );

			if ( PressedPrimary )
			{
				var tBoard = tBox.WithScale( tBox.Scale / BoxSize );

				if ( TryCreateBoard( tBoard, out _ ) )
					StartPoint = null;
			}
		}
		else
		{
			if ( hasHit && PressedPrimary )
				StartPoint = point;
		}
	}

	protected virtual bool TryCreateBoard( in Transform t, out GameObject objBoard )
	{
		if ( !IsClientAllowed( Client.Local ) )
		{
			objBoard = null;
			return false;
		}

		if ( !BoardPrefab.TrySpawn( t, out objBoard ) )
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
