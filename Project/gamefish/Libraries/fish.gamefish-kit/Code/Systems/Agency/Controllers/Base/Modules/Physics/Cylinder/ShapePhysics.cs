namespace GameFish;

public abstract class ShapePhysics : ControllerPhysics
{
	/// <summary>
	/// Render the shape in the editor?
	/// </summary>
	[Property]
	[Title( "Render Shape" )]
	[Feature( PAWN ), Group( DEBUG )]
	public bool RenderShapeEnabled { get; set; } = true;

	/// <summary>
	/// The color to render the shape as.
	/// </summary>
	[Property]
	[Title( "Shape Color" )]
	[Feature( PAWN ), Group( DEBUG )]
	public virtual Color RenderColor { get; set; } = Color.Cyan;

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		RenderShape();
	}

	/// <summary>
	/// Draw this shape at the current origin.
	/// </summary>
	public abstract void RenderShape();
}
