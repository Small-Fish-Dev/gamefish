using System;
using System.Text.Json.Serialization;
using System.Transactions;

namespace GameFish;

/// <summary>
/// Defines an area with a shape and its position/rotation/scale.
/// </summary>
public partial struct Area : IValid
{
	public enum ShapeType
	{
		None,

		[Icon( "◽" )]
		Point,

		[Icon( "📦" )]
		Box,

		[Icon( "🌐" )]
		Sphere,

		[Icon( "💊" )]
		Capsule,

		[Icon( "🍼" )]
		Cylinder,
	}

	/// <summary>
	/// Is the shape defined and transform valid?
	/// </summary>
	[Hide, JsonIgnore]
	public readonly bool IsValid => Shape is not ShapeType.None
		&& Transform != default && ITransform.IsValid( Transform );

	[Hide, JsonIgnore] public readonly bool HasBounds => Shape is ShapeType.Box;
	[Hide, JsonIgnore] public readonly bool HasHeight => Shape is ShapeType.Capsule or ShapeType.Cylinder;
	[Hide, JsonIgnore] public readonly bool HasRadius => Shape is ShapeType.Capsule or ShapeType.Cylinder or ShapeType.Sphere;

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

	private readonly Capsule GetCapsule( in Transform t )
	{
		var v = Vector3.Up * (Height * 0.5f);
		return new Capsule( -v, v, Radius );
	}

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
		Shape = ShapeType.Box;

		Transform = t;
		Bounds = bounds;
	}

	/// <summary>
	/// Makes a sphere area with the given transform.
	/// </summary>
	public Area( in Transform t, in float radius )
	{
		Shape = ShapeType.Sphere;

		Transform = t;
		Radius = radius;
	}

	/// <summary>
	/// Makes an shape with the given transform.
	/// </summary>
	public Area( in Transform t, in float radius, in float height, in ShapeType shape = ShapeType.Cylinder )
	{
		Shape = shape;

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
			ShapeType.Capsule => Gizmo.Draw.DepthCapsule( GetCapsule( in t ), cLines, cSolid, tWorld: t ),
			ShapeType.Cylinder => Gizmo.Draw.DepthCylinder( Radius, Height, cLines, cSolid, tWorld: t ),
			_ => false,
		};
	}

	public bool DrawHandles( in Transform tObj, out Area area )
	{
		using ( Gizmo.Scope( "Area" ) )
		{
			Gizmo.Transform = tObj.ToWorld( Transform );

			switch ( Shape )
			{
				case ShapeType.Box:

					if ( Gizmo.Control.BoundingBox( "Area", Bounds, out var bbox ) )
					{
						Bounds = bbox;

						goto Dragged;
					}

					break;

				case ShapeType.Sphere:

					if ( DrawRadialBoxHandles( in tObj ) )
						goto Dragged;

					break;

				case ShapeType.Capsule:

					if ( DrawRadialBoxHandles( in tObj ) )
						goto Dragged;

					break;

				case ShapeType.Cylinder:

					if ( DrawRadialBoxHandles( in tObj ) )
						goto Dragged;

					break;
			}
		}

		// Fuck you.
		area = default;
		return false;

		// Only apply the shit if it shat.
		Dragged:

		area = this;
		return true;
	}

	private bool DrawRadialBoxHandles( in Transform tObj )
	{
		var h = HasHeight ? Height * 0.5f : Radius;
		var r = Radius;

		if ( Shape is ShapeType.Capsule )
			h += Radius.Abs() * h.Sign();

		var vUp = Vector3.Up * h;
		var vRight = Vector3.Right * r;
		var vForward = Vector3.Forward * r;

		var oldBox = BBox.FromPoints( [-vUp, vUp, -vRight, vRight, -vForward, vForward] );

		if ( Gizmo.Control.BoundingBox( "Area", oldBox, out var newBox ) )
		{
			var tWorld = tObj.ToWorld( Transform );

			var delta = newBox.Extents - oldBox.Extents;

			var fwd = Vector3.Forward / tWorld.Scale.x.NonZero();
			var left = Vector3.Left / tWorld.Scale.y.NonZero();

			var hDot = delta.Dot( fwd + left );

			if ( HasHeight )
			{
				Radius += hDot;
				Height += delta.z * 2f;
			}
			else
			{
				Radius += delta.z;
			}

			var newMins = tWorld.PointToWorld( newBox.Mins );
			var newMaxs = tWorld.PointToWorld( newBox.Maxs );

			var worldCenter = newMins.LerpTo( in newMaxs, 0.5f );
			var localPos = tObj.PointToLocal( in worldCenter );

			// Move the shape.
			var t = Transform;
			t.Position = localPos;

			if ( !ITransform.IsValid( t ) )
				return false;

			Transform = t;
			return true;
		}

		return false;
	}

	/// <returns> The transform to use if linked to an object. </returns>
	public readonly Transform? ToWorld( GameObject obj )
		=> obj?.WorldTransform.ToWorld( Transform );

	/// <param name="tWorld"> The transform the point is relative to. </param>
	/// <returns> The middle center position of the shape. </returns>
	public readonly Vector3 GetCenter( Transform? tWorld = null )
	{
		var t = tWorld ?? Transform;

		Vector3 localPos;

		if ( Shape is ShapeType.Box )
			localPos = Bounds.Center;
		else
			localPos = Vector3.Zero;

		return t.PointToWorld( localPos );
	}

	/// <param name="tWorld"> The transform the point is relative to. </param>
	/// <returns> The top center position of the shape. </returns>
	public readonly Vector3 GetTop( Transform? tWorld = null )
	{
		var t = tWorld ?? Transform;

		var localPos = Shape switch
		{
			ShapeType.Point => Vector3.Zero,
			ShapeType.Box => Bounds.Center.WithZ( Bounds.Maxs.z ),
			ShapeType.Sphere => Vector3.Up * Radius,
			ShapeType.Capsule => Vector3.Up * Height,
			ShapeType.Cylinder => Vector3.Up * Height,
			_ => Vector3.Zero,
		};

		return t.PointToWorld( localPos );
	}

	/// <param name="tWorld"> The transform the point is relative to. </param>
	/// <returns> The bottom center position of the shape. </returns>
	public readonly Vector3 GetBottom( Transform? tWorld = null )
	{
		var t = tWorld ?? Transform;

		var localPos = Shape switch
		{
			ShapeType.Point => Vector3.Zero,
			ShapeType.Box => Bounds.Center.WithZ( Bounds.Mins.z ),
			ShapeType.Sphere => Vector3.Down * Radius,
			ShapeType.Capsule => Vector3.Down * Height,
			ShapeType.Cylinder => Vector3.Down * Height,
			_ => Vector3.Zero,
		};

		return t.PointToWorld( localPos );
	}

	/// <param name="obj"> The object that the point is relative to. </param>
	/// <returns> The bottom center position of the shape. </returns>
	public readonly Vector3? GetCenter( GameObject obj )
		=> obj.IsValid() ? GetCenter( ToWorld( obj ) ) : null;

	/// <param name="obj"> The object that the point is relative to. </param>
	/// <returns> The bottom center position of the shape. </returns>
	public readonly Vector3? GetTop( GameObject obj )
		=> obj.IsValid() ? GetTop( ToWorld( obj ) ) : null;

	/// <param name="obj"> The object that the point is relative to. </param>
	/// <returns> The bottom center position of the shape. </returns>
	public readonly Vector3? GetBottom( GameObject obj )
		=> obj.IsValid() ? GetBottom( ToWorld( obj ) ) : null;

	/// <param name="tWorld"> The transform the point is relative to. </param>
	/// <returns> A random point inside of the defined shape. </returns>
	public readonly Vector3 GetRandomPointInside( Transform? tWorld = null )
	{
		var t = tWorld ?? Transform;

		Vector3 localPos;

		switch ( Shape )
		{
			case ShapeType.Box:
				localPos = Bounds.RandomPointInside;
				break;

			case ShapeType.Sphere:
				localPos = Vector3.Random.Normal * Random.Float( 0f, Radius );
				break;

			case ShapeType.Capsule:
				var v = Vector3.Up * (Height * 0.5f);
				localPos = new Capsule( v, -v, Radius ).RandomPointInside;
				break;

			case ShapeType.Cylinder:
				localPos = Vector2.Random.Normal * Random.Float( 0f, Radius );
				localPos.z = Random.Float( Height * -0.5f, Height * 0.5f );
				break;

			default:
				localPos = Vector3.Zero;
				break;
		}

		return t.PointToWorld( localPos );
	}

	/// <param name="tWorld"> The transform the point is relative to. </param>
	/// <returns> A random point on the edge of the defined shape. </returns>
	public readonly Vector3 GetRandomPointOnEdge( Transform? tWorld = null )
	{
		var t = tWorld ?? Transform;

		Vector3 localPos;

		switch ( Shape )
		{
			case ShapeType.Box:
				localPos = Bounds.RandomPointOnEdge;
				break;

			case ShapeType.Sphere:
				localPos = new Sphere( 0f, Radius ).RandomPointOnEdge;
				break;

			case ShapeType.Capsule:
				var v = Vector3.Up * (Height * 0.5f);
				localPos = new Capsule( v, -v, Radius ).RandomPointOnEdge;
				break;

			case ShapeType.Cylinder:
				var rYaw = Rotation.FromYaw( Random.Float( 0f, 360f ) );
				var flatDir = Vector3.Forward.RotateAround( 0f, rYaw );

				localPos = flatDir * Radius;
				localPos.z = Random.Float( Height * -0.5f, Height * 0.5f );
				break;

			default:
				localPos = Vector3.Zero;
				break;
		}

		return t.PointToWorld( localPos );
	}

	/// <param name="obj"> The object that the point is relative to. </param>
	/// <returns> A random point inside of the defined shape. </returns>
	public readonly Vector3? GetRandomPointInside( GameObject obj )
		=> obj.IsValid() ? GetRandomPointInside( ToWorld( obj ) ) : null;

	/// <param name="obj"> The object that the point is relative to. </param>
	/// <returns> A random point inside of the defined shape. </returns>
	public readonly Vector3? GetRandomPointOnEdge( GameObject obj )
		=> obj.IsValid() ? GetRandomPointOnEdge( ToWorld( obj ) ) : null;
}
