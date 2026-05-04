namespace GameFish;

public class CylinderPhysics : ShapePhysics
{
	/// <summary>
	/// The distance from the center the side of the cylinder.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( PHYSICS )]
	public virtual float Radius { get; set; } = 16f;

	/// <summary>
	/// The total height of the cylinder.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( PHYSICS )]
	protected virtual float Height { get; set; } = 64f;

	public override SceneTrace Trace( in float skin = 0f )
	{
		var tr = base.Trace()
			.Cylinder( Height + skin, Radius + (skin * 0.5f) )
			.IgnoreGameObjectHierarchy( GameObject );

		return tr;
	}

	public override void RenderShape()
	{
		var tOrigin = Origin.WithOffset( TraceOffset );

		this.DrawCylinder( Radius, Height, RenderColor, Color.Transparent, tWorld: tOrigin );
	}
}
