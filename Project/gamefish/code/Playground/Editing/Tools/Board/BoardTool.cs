using System.Runtime.Serialization;

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


	public bool IsPlacingBoard { get; protected set; }

	public Vector3? TargetPoint { get; protected set; }
	public Vector3 StartPoint { get; protected set; }

	public BBox BoardBounds { get; protected set; }
	public Transform BoardTransform { get; protected set; }


	public override void OnExit()
	{
		base.OnExit();

		TryStopShaping();
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
		if ( IsPlacingBoard )
		{
			if ( TryCreateBoard( BoardTransform, out _ ) )
				TryStopShaping();
		}
		else if ( TargetPoint.HasValue )
		{
			StartPoint = TargetPoint.Value;
			IsPlacingBoard = true;
		}

		return true;
	}

	public override bool TryMouseWheel( in Vector2 dir )
	{
		var scroll = dir.y != 0f ? -dir.y : dir.x;
		scroll *= ScrollSensitivity;

		Distance = (Distance + scroll).Clamp( DistanceRange );

		return true;
	}

	public override bool TryRightClick()
		=> TryStopShaping();

	protected virtual bool TryStopShaping()
	{
		if ( !IsPlacingBoard )
			return false;

		TargetPoint = null;
		IsPlacingBoard = false;

		return true;
	}

	protected virtual void UpdateScroll( in float deltaTime )
	{
		var yScroll = Input.MouseWheel.y;

		if ( yScroll == 0f )
			return;
	}

	protected virtual void UpdatePlace( in float deltaTime )
	{
		TargetPoint = null;

		if ( !IsClientAllowed( Client.Local ) )
			return;

		if ( !TryTrace( out var tr ) )
			return;

		var traceHit = tr.Distance <= Distance;
		var pointDist = tr.Distance.Min( Distance );
		var point = tr.StartPosition + (tr.Direction * pointDist);

		var validColor = Color.White.Desaturate( 0.4f );
		var c1 = validColor.WithAlpha( 0.4f );
		var c2 = validColor.WithAlpha( 0.1f );

		var hitNormal = tr.Normal;

		if ( traceHit )
		{
			point += tr.Normal * ((BoardHeight / 2f) + 0.1f);
		}
		else
		{
			hitNormal = Vector3.Up;

			c1 = c1.WithAlphaMultiplied( 0.3f );
			c2 = c2.WithAlphaMultiplied( 0.3f );
		}

		TargetPoint = point;

		this.DrawSphere( 2f, point, Color.Transparent, c1, global::Transform.Zero );

		if ( IsPlacingBoard )
		{
			var dist = StartPoint.Distance( point );
			var dir = StartPoint.Direction( point );
			var center = StartPoint.LerpTo( point, 0.5f );

			var length = dist;
			var scale = new Vector3( length, BoardWidth, BoardHeight );
			var rDir = Rotation.LookAt( dir, hitNormal );

			var bounds = BBox.FromPositionAndSize( Vector3.Zero, Vector3.One );
			var tBox = new Transform( center, rDir, scale );

			BoardBounds = bounds;
			BoardTransform = tBox.WithScale( tBox.Scale / BoxSize );

			var isError = dist < BoardWidth;

			if ( isError && !traceHit )
			{
				var errorColor = Color.Red.Desaturate( 0.4f );
				var c1Error = errorColor.WithAlpha( 0.4f );
				var c2Error = errorColor.WithAlpha( 0.2f );

				c1 = c1Error;
				c2 = c2Error;
			}

			this.DrawBox( bounds, c1, c2, tBox );
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
