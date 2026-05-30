using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Tells gizmo helper methods how it should render the shape.
/// </summary>
public sealed class GizmoSettings : IValid
{
	public bool IsValid => EditorPasses.HasFlag( GizmoPass.Both )
						|| InGamePasses.HasFlag( GizmoPass.Both );

	/// <summary>
	/// If it should render within and/or over the world(while editing the scene).
	/// </summary>
	[Title( "Editor" )]
	[Group( RENDERING )]
	public GizmoPass EditorPasses = GizmoPass.Depth | GizmoPass.Overlay;

	/// <summary>
	/// If it should render within and/or over the world(while playing the game).
	/// </summary>
	[Title( "Game" )]
	[Group( RENDERING )]
	public GizmoPass InGamePasses = GizmoPass.None;

	/// <summary>
	/// The color of the lines surrounding the shape.
	/// </summary>
	public Color LineColor = Color.White;

	/// <summary>
	/// The shape's inner solid color.
	/// </summary>
	public Color SolidColor = Color.White.WithAlpha( 0.05f );

	/// <summary>
	/// The opacity of the gizmo when it's rendered in-world(not seen through walls).
	/// </summary>
	[Title( "Alpha (depthless)" )]
	[Range( 0f, 1f, clamped: false )]
	public float AlphaDepth = 0.8f;

	/// <summary>
	/// Multiplies the alpha of the pass that's visible through walls.
	/// </summary>
	[Title( "Alpha (overlay)" )]
	[Range( 0f, 1f, clamped: false )]
	public float AlphaOverlay = 0.2f;

	public GizmoSettings() { }

	public GizmoSettings( in Color cLine, in Color cSolid )
	{
		LineColor = cLine;
		SolidColor = cSolid;
	}

	public bool HasDepth( in bool inGame ) => inGame
		? InGamePasses.HasFlag( GizmoPass.Depth )
		: EditorPasses.HasFlag( GizmoPass.Depth );

	public bool HasOverlay( in bool inGame ) => inGame
		? InGamePasses.HasFlag( GizmoPass.Overlay )
		: EditorPasses.HasFlag( GizmoPass.Overlay );
}
