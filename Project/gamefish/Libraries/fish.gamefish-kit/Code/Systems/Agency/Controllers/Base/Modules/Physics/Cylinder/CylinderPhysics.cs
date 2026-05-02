namespace GameFish;

public class CylinderPhysics : ControllerPhysics
{
	/// <summary>
	/// The radius of the cylinder and head.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( PHYSICS )]
	public float Radius { get; set; } = 16f;

	protected float Height => Radius * 2f;

	public override SceneTrace Trace()
	{
		var tr = base.Trace()
			.Cylinder( Height, Radius )
			.IgnoreGameObjectHierarchy( GameObject );

		return tr;
	}
}
