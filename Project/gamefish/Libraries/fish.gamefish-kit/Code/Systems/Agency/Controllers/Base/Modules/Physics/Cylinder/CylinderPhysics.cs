using Sandbox.VR;

namespace GameFish;

public class CylinderPhysics : ShapePhysics
{
	/// <summary>
	/// The distance from the center the side of the cylinder.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( COLLISION )]
	public virtual float Radius { get; set; } = 16f;

	/// <summary>
	/// The total height of the cylinder.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( COLLISION )]
	protected virtual float Height { get; set; } = 64f;

	[Property]
	[Feature( PAWN ), Group( COLLISION )]
	protected HullCollider CylinderCollider { get; set; }

	public override Collider ShapeCollider => CylinderCollider;

	public override SceneTrace Trace( in float skin = 0f )
	{
		var tr = base.Trace( skin: skin )
			.Cylinder( Height - (skin * 2), Radius - skin )
			.IgnoreGameObjectHierarchy( GameObject );

		return tr;
	}

	public override void RenderShape()
	{
		var tOrigin = Origin.WithOffset( TraceOffset );

		this.DrawCylinder( Radius, Height, RenderColor, Color.Transparent, tWorld: tOrigin );
	}

	protected override void CreateCollider( out GameObject obj )
	{
		base.CreateCollider( out obj );

		if ( !obj.IsValid() )
			return;

		CylinderCollider = obj.Components?.Create<HullCollider>();

		UpdateCollider();
	}

	protected override void UpdateCollider()
	{
		base.UpdateCollider();

		if ( !CylinderCollider.IsValid() )
			return;

		CylinderCollider.Height = Height;
		CylinderCollider.Radius = Radius;
		CylinderCollider.Radius2 = Radius;

		CylinderCollider.Type = HullCollider.PrimitiveType.Cylinder;
	}
}
