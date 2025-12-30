using System.Text.Json.Serialization;

namespace Playground;

public partial class BrickTool : ShapeTool
{
	public const int BRICK_SIZE_MIN = 16;
	public const int BRICK_SIZE_MAX = 32;
	public const float BRICK_SIZE_STEP = 8;
	public const float BRICK_MODEL_SIZE = 16;
	public const float BRICK_HUE_DELTA = 12f;

	public override bool HasScrollFocus => base.HasScrollFocus || HasPoints || HoldingShift;

	[Property]
	[Title( "Prefab Size" )]
	[Range( 1f, 32f, clamped: false )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER - 1 )]
	public float BrickPrefabSize
	{
		get => _brickPrefabSize.Max( 1f );
		protected set => _brickPrefabSize = value.Max( 1f );
	}

	protected float _brickPrefabSize = 16f;

	/// <summary>
	/// Scales the size of the brick by this power.
	/// <b> TODO: </b> Your mother.
	/// </summary>
	[Property]
	[ToolSetting]
	[Range( BRICK_SIZE_MIN, BRICK_SIZE_MAX )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public int BrickSize
	{
		get => _brickSize.Clamp( BRICK_SIZE_MIN, BRICK_SIZE_MAX );
		set
		{
			var step = (value / BRICK_SIZE_STEP).Round();
			var size = (step * BRICK_SIZE_STEP).CeilToInt();
			_brickSize = size.Clamp( BRICK_SIZE_MIN, BRICK_SIZE_MAX );
		}
	}

	protected int _brickSize = 16;

	[ToolSetting]
	[Property, JsonIgnore]
	[ColorUsage( HasAlpha = false, IsHDR = false )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public Color BrickColor { get; set; } = Color.White.Darken( 0.1f );

	/// <summary>
	/// The vertical layer count when placing bricks.
	/// </summary>
	public int BrickHeight
	{
		get => _brickHeight.Max( 1 );
		protected set => _brickHeight = value.Max( 1 );
	}

	protected int _brickHeight;

	public override int PointLimit => 2;

	protected override void OnStart()
	{
		base.OnStart();

		BrickColor = GetRandomBrickColor();
	}

	protected override void Clear()
	{
		base.Clear();

		BrickHeight = 1;
	}

	public static Color GetRandomBrickColor()
	{
		var step = (Random.Float( 360 ) / BRICK_HUE_DELTA).Floor();
		var hue = BRICK_HUE_DELTA * step;
		return new ColorHsv( hue, 0.7f, 0.6f );
	}

	public static Color GetNextColor( ColorHsv color )
	{
		color.Hue = (color.Hue + BRICK_HUE_DELTA).NormalizeDegrees();
		color.Saturation = 0.7f;
		color.Value = 0.6f;

		return color;
	}

	[Rpc.Host]
	protected void RpcDestroyBrick( BrickBlock brick )
	{
		if ( !brick.IsValid() )
			return;

		if ( !TryUse( Rpc.Caller, out _ ) )
			return;

		brick.DestroyGameObject();
	}

	[Rpc.Host]
	protected void RpcSetBrickColor( BrickBlock brick, Color color )
	{
		if ( !brick.IsValid() )
			return;

		if ( !TryUse( Rpc.Caller, out _ ) )
			return;

		brick.BrickColor = color;
	}

	public float SnapToBrickGrid( in float n )
		=> (n / BrickSize).Round() * BrickSize;

	public Vector3 SnapToBrickGrid( Vector3 localPos )
	{
		localPos.x = (localPos.x / BrickSize).Round() * BrickSize;
		localPos.y = (localPos.y / BrickSize).Round() * BrickSize;
		localPos.z = (localPos.z / BrickSize).Round() * BrickSize;

		return localPos;
	}

	public Vector3 SnapToBrickGrid( Transform tBrick, in Vector3 worldPos )
	{
		tBrick.Scale = 1f;

		var localPos = tBrick.PointToLocal( worldPos );
		localPos = SnapToBrickGrid( localPos );

		return tBrick.PointToWorld( localPos );
	}

	protected override void OnScroll( in float scroll )
	{
		if ( HasPoints )
		{
			BrickHeight += scroll.Round().CeilToInt().Sign();
			return;
		}

		// if ( HoldingShift )
		// {
		// BrickSize += scroll.Round().CeilToInt();
		// return;
		// }

		base.OnScroll( scroll );
	}

	protected override void OnReload( in SceneTraceResult tr )
	{
		base.OnReload( tr );

		if ( !tr.Hit || !tr.Collider.IsValid() )
			return;

		var brick = tr.Collider.GetComponent<BrickBlock>();

		if ( !brick.IsValid() )
			return;

		if ( !Editor.TryFindIsland( brick.GameObject, out var island ) )
			return;

		if ( !TrySetOrigin( island.GameObject, island, brick.LocalTransform ) )
			return;

		AddPoint( Vector3.Zero, brick.LocalRotation );

		RpcDestroyBrick( brick );
	}

	protected override void OnMiddleMouse( in SceneTraceResult tr )
	{
		base.OnMiddleMouse( tr );

		if ( !tr.Hit || !tr.Collider.IsValid() )
			return;

		var brick = tr.Collider.GetComponent<BrickBlock>();

		if ( !brick.IsValid() )
			return;

		if ( HoldingShift )
		{
			BrickColor = brick.BrickColor;
			return;
		}

		var nextColor = GetNextColor( brick.BrickColor );

		BrickColor = nextColor;

		RpcSetBrickColor( brick, BrickColor );
	}

	public override bool TryTrace( out SceneTraceResult tr )
		=> Editor.TryTrace( Scene, out tr, dist: Distance );

	protected override bool TryGetOffsetFromTrace( in SceneTraceResult tr, out Offset offset )
	{
		offset = default;
		return false;
	}

	protected override void OnPrimary( in SceneTraceResult tr )
	{
		if ( !TryGetCursorPosition( out var cursorPos ) )
			return;

		if ( !HasPoints && TargetObject.IsValid() )
		{
			var tWorld = TargetObject.WorldTransform;
			var rNormal = Rotation.LookAt( tr.Normal, tWorld.Forward );
			var tLocal = tWorld.ToLocal( new( tr.EndPosition, rNormal ) );

			SetOrigin( tLocal, TargetObject, TargetComponent );
		}

		TryAddWorldPoint( new( cursorPos, Rotation.Identity ) );
	}

	protected override void SetOrigin( Offset offset, GameObject obj = null, Component c = null )
	{
		offset.Position = offset.Position.SnapToGrid( BRICK_SIZE_MIN );

		var vForward = offset.Rotation.Forward;
		var vSnapped = Rotation.Identity.ClosestAxis( vForward );
		offset.Rotation = Rotation.LookAt( vSnapped );

		base.SetOrigin( offset, obj, c );
	}

	protected override void AddPoint( Vector3 pos, Rotation r )
	{
		pos = SnapToBrickGrid( pos );

		base.AddPoint( pos, r );
	}

	protected override bool TryGetCursorPosition( out Vector3 cursorPos )
	{
		if ( !TryTrace( out var tr ) )
		{
			cursorPos = default;
			return false;
		}

		// Manual distance for air placement.
		var userDistance = tr.Distance.Min( Distance );
		cursorPos = tr.StartPosition + (tr.Direction * userDistance);

		if ( HasOrigin )
		{
			var tOrigin = GetShapeOrigin();
			var vUp = tOrigin.Forward;

			// this.DrawArrow( tOrigin.Position, tOrigin.Position + tOrigin.Up * 64f, Color.Cyan, tWorld: global::Transform.Zero );
			// this.DrawArrow( tOrigin.Position, tOrigin.Position + tOrigin.Forward * 64f, Color.Red, tWorld: global::Transform.Zero );
			// this.DrawArrow( tOrigin.Position, tOrigin.Position + tOrigin.Right * 64f, Color.Green, tWorld: global::Transform.Zero );

			var plane = new Plane( tOrigin.Position, vUp );
			var ray = new Ray( tr.StartPosition, tr.Direction );

			if ( !plane.TryTrace( in ray, out var hitPoint, twosided: true ) )
				return false;

			// Horizontal Drag
			cursorPos = SnapToBrickGrid( OriginWorldTransform, hitPoint );

			// Vertical Layers
			cursorPos += vUp * BrickSize * BrickHeight;

			return true;
		}

		// Target Snapping
		if ( TargetObject.IsValid() )
		{
			var tBrick = GetShapeOrigin( TargetObject.WorldTransform );
			cursorPos = SnapToBrickGrid( tBrick, cursorPos );
			return true;
		}

		// Global Snapping
		cursorPos = SnapToBrickGrid( global::Transform.Zero, cursorPos );

		return true;
	}

	protected override bool TryCreateShape( out GameObject obj )
	{
		obj = null;

		if ( !HasPoints || !ValidShape )
			return false;

		var points = Points?.Select( pr => pr.Position ).ToList();
		var box = BBox.FromPoints( points );

		var tOrigin = GetShapeOrigin();
		var tShape = tOrigin.ToWorld( new( box.Mins, Rotation.Identity, box.Size ) );

		tShape.Scale = tShape.Scale.ComponentMax( BRICK_SIZE_MIN );
		tShape.Scale /= BrickPrefabSize;

		if ( Editor.TryFindIsland( OriginObject, out var island ) )
			if ( TrySpawnObject( ShapePrefab, tShape, island, out _ ) )
				return true;

		if ( TrySpawnObject( ShapePrefab, tShape, out _, withIsland: true ) )
			return true;

		return false;
	}

	protected override void OnObjectSpawned( EditorObject e, EditorIsland parent )
	{
		if ( e.IsValid() && e is BrickBlock brick )
			brick.BrickColor = BrickColor;

		base.OnObjectSpawned( e, parent );
	}
}
