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

	public override SceneTrace Trace( in float skin = 0f )
	{
		var tr = base.Trace()
			.Cylinder( Height + skin, Radius + (skin * 0.5f) )
			.IgnoreGameObjectHierarchy( GameObject );

		return tr;
	}
}
