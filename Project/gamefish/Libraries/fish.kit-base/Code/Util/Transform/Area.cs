using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Defines an area with a shape and its position/rotation/scale.
/// </summary>
public partial struct Area : IValid
{
	public enum ShapeType
	{
		None,
		[Icon( "◽" )] Point,
		[Icon( "📦" )] Box,
		[Icon( "🌐" )] Sphere,
		[Icon( "🍼" )] Cylinder,
	}

	/// <summary>
	/// Is the shape defined and transform valid?
	/// </summary>
	[Hide, JsonIgnore]
	public readonly bool IsValid => Shape is not ShapeType.None
		&& Transform != default && ITransform.IsValid( Transform );

	[Hide, JsonIgnore] public readonly bool HasBounds => Shape is ShapeType.Box;
	[Hide, JsonIgnore] public readonly bool HasHeight => Shape is ShapeType.Cylinder;
	[Hide, JsonIgnore] public readonly bool HasRadius => Shape is ShapeType.Sphere or ShapeType.Cylinder;

	/// <summary>
	/// The shape of the area.
	/// </summary>
	public ShapeType Shape { get; set; } = ShapeType.Box;

	/// <summary>
	/// The bounds of the box defining this location.
	/// </summary>
	[ShowIf( nameof( HasBounds ), true )]
	public BBox Bounds { get; set; } = BBox.FromPositionAndSize( Vector3.Zero, 256f );

	[ShowIf( nameof( HasRadius ), true )]
	public float Radius { get; set; } = 256f;

	[ShowIf( nameof( HasHeight ), true )]
	public float Height { get; set; } = 256f;

	/// <summary>
	/// The origin, rotation and scale of the shape. <br />
	/// May be in local space if used by a component.
	/// </summary>
	public Transform Transform { get; set; } = Transform.Zero;

	public Area() { }

	/// <summary>
	/// Makes a box area with the given transform.
	/// </summary>
	/// <param name="t"></param>
	/// <param name="bounds"></param>
	public Area( in Transform t, in BBox bounds )
	{
		Transform = t;
		Bounds = bounds;
	}

	/// <summary>
	/// Makes a sphere area with the given transform.
	/// </summary>
	public Area( in Transform t, in float radius )
	{
		Transform = t;
		Radius = radius;
	}

	/// <summary>
	/// Makes a cylinder area with the given transform.
	/// </summary>
	public Area( in Transform t, in float radius, in float height )
	{
		Transform = t;
		Radius = radius;
		Height = height;
	}

	/// <summary>
	/// Draws the shape with depth gizmos according to its configuration.
	/// </summary>
	/// <param name="cLines"></param>
	/// <param name="cSolid"></param>
	/// <param name="tWorld"> Overrides the world-space orientation of the area. </param>
	public readonly bool DrawGizmos( in Color cLines, in Color cSolid, in Transform? tWorld = null )
	{
		var t = tWorld ?? Transform;

		return Shape switch
		{
			ShapeType.Box => Gizmo.Draw.DepthBox( Bounds, cLines, cSolid, tWorld: t ),
			ShapeType.Sphere => Gizmo.Draw.DepthSphere( Radius, Vector3.Zero, cLines, cSolid, tWorld: t ),
			ShapeType.Cylinder => Gizmo.Draw.DepthCylinder( Radius, Height, cLines, cSolid, tWorld: t ),
			_ => false,
		};
	}

	/// <returns> The bottom center position of the shape. </returns>
	public readonly Vector3 GetBottom( bool worldSpace = true, Transform? tWorld = null )
	{
		var localPos = Shape switch
		{
			ShapeType.Point => Vector3.Zero,
			ShapeType.Box => Bounds.Center.WithZ( Bounds.Mins.z ),
			ShapeType.Sphere => Vector3.Down * Radius,
			ShapeType.Cylinder => Vector3.Down * Height,
			_ => Vector3.Zero,
		};

		return worldSpace
			? (tWorld ?? Transform).PointToWorld( localPos )
			: localPos;
	}

	/// <param name="worldSpace"> Transform the point into world-space? </param>
	/// <param name="tWorld"> The world-space transform to override with. </param>
	/// <returns> A random point inside of the defined shape. </returns>
	public readonly Vector3 GetRandomPoint( bool worldSpace = true, Transform? tWorld = null )
	{
		Vector3 localPos;

		switch ( Shape )
		{
			case ShapeType.Point:
				localPos = Vector3.Zero;
				break;

			case ShapeType.Box:
				localPos = Bounds.RandomPointInside;
				break;

			case ShapeType.Sphere:
				localPos = Vector3.Random.Normal * Random.Float( 0f, Radius );
				break;

			case ShapeType.Cylinder:
				localPos = Vector2.Random.Normal * Random.Float( 0f, Radius );
				localPos.z = Random.Float( 0f, Height ) - (Height * 0.5f);
				break;

			default:
				localPos = Vector3.Zero;
				break;
		}

		return worldSpace
			? (tWorld ?? Transform).PointToWorld( localPos )
			: localPos;
	}
}
