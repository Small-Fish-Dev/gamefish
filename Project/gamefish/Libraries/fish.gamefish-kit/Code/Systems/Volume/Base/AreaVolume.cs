namespace GameFish;

[Group( Library.NAME )]
[Icon( "highlight_alt" )]
[EditorHandle( Icon = "📐" )]
public abstract class AreaVolume : ModuleEntity
{
	protected const int AREA_ORDER = DEFAULT_ORDER - 1000;
	protected const int AREA_DEBUG_ORDER = AREA_ORDER - 100;

	/// <summary>
	/// The primary area of this volume.
	/// </summary>
	[Property]
	[InlineEditor, WideMode]
	[Order( AREA_ORDER )]
	[Feature( AREA ), Group( TRANSFORM )]
	public virtual Area Area { get; protected set; } = new( global::Transform.Zero, BBox.FromPositionAndSize( Vector3.Up * 64f, 128f ) );

	/// <summary>
	/// If true: the area will be rendered ingame.
	/// </summary>
	[Property]
	[Title( "Render (ingame)" )]
	[Order( AREA_DEBUG_ORDER )]
	[Feature( AREA ), Group( DEBUG )]
	public bool DebugRenderInGame { get; set; } = false;

	/// <summary>
	/// The color of area shape lines.
	/// </summary>
	[Property]
	[Title( "Line Color" )]
	[Order( AREA_DEBUG_ORDER )]
	[Feature( AREA ), Group( DEBUG )]
	protected virtual Color AreaLineColor { get; set; } = Color.Black;

	/// <summary>
	/// The color of area shape sides/faces.
	/// </summary>
	[Property]
	[Title( "Solid Color" )]
	[Order( AREA_DEBUG_ORDER )]
	[Feature( AREA ), Group( DEBUG )]
	protected virtual Color AreaSolidColor { get; set; } = Color.White.WithAlpha( 0.07f );

	public virtual Color GetLineColor( in Area a ) => AreaLineColor;
	public virtual Color GetSolidColor( in Area a ) => AreaSolidColor;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( DebugRenderInGame )
			RenderArea( allowResize: false );
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		RenderArea( allowResize: Gizmo.IsSelected );
	}

	/// <summary>
	/// Draws the primary area's shape.
	/// </summary>
	/// <param name="allowResize"> If true: allow area gizmo drag/resize handles. </param>
	protected virtual void RenderArea( in bool allowResize )
	{
		if ( allowResize && TryResizeArea( Area, out var newArea ) )
			Area = newArea;

		RenderArea( Area );
	}

	/// <summary>
	/// Draws an area's shape with optional resize gizmos.
	/// </summary>
	/// <param name="a"> The area defining the shape to render. </param>
	public virtual void RenderArea( Area a )
	{
		var tArea = WorldTransform.ToWorld( a.Transform );

		var lineColor = GetLineColor( a );
		var solidColor = GetSolidColor( a );

		a.DrawGizmos( in lineColor, in solidColor, tArea );
	}

	protected virtual bool TryResizeArea( in Area a, out Area newArea )
		=> a.DrawHandles( WorldTransform, out newArea );

	public virtual Vector3? GetRandomPointInside()
		=> Area.GetRandomPointInside( GameObject );

	public virtual Vector3? GetRandomPointOnEdge()
		=> Area.GetRandomPointOnEdge( GameObject );
}
