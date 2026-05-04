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

	public abstract Collider ShapeCollider { get; }

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( RenderShapeEnabled )
			RenderShape();
	}

	/// <summary>
	/// Draw this shape at the current origin.
	/// </summary>
	public abstract void RenderShape();

	[Button( "Create" )]
	[ShowIf( nameof( InEditor ), true )]
	[Feature( PAWN ), Group( COLLISION )]
	protected void ButtonCreateCollider()
		=> CreateCollider();

	[Button( "Update" )]
	[ShowIf( nameof( InEditor ), true )]
	[Feature( PAWN ), Group( COLLISION )]
	protected void ButtonUpdateCollider()
		=> UpdateCollider();

	/// <summary>
	/// Creates the collider for this shape.
	/// </summary>
	protected abstract void CreateCollider();

	/// <summary>
	/// Sets the transform and settings of the collider for this shape.
	/// </summary>
	protected abstract void UpdateCollider();
}
