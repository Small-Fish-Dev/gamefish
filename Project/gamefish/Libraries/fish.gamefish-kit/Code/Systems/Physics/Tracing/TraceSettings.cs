namespace GameFish;

/// <summary>
/// Configures relevant information about a trace to be performed.
/// </summary>
public partial struct TraceSettings
{
	/// <summary>
	/// The shape of the trace.
	/// </summary>
	[Group( TRANSFORM )]
	public TraceShape Shape { get; set; } = TraceShape.Box;

	/// <summary>
	/// The shape's rotation.
	/// </summary>
	[Group( TRANSFORM )]
	[Title( "Rotation" )]
	[HideIf( nameof( Shape ), TraceShape.Line )]
	public Rotation ShapeRotation { get; set; } = Rotation.Identity;

	[Group( TRANSFORM )]
	[ShowIf( nameof( Shape ), TraceShape.Box )]
	public BBox Box { get; set; } = BBox.FromPositionAndSize( Vector3.Zero, 16f );

	/// <summary>
	/// The radius of the sphere/cylinder.
	/// </summary>
	[Group( TRANSFORM )]
	[ShowIf( nameof( Shape ), TraceShape.Sphere )]
	[ShowIf( nameof( Shape ), TraceShape.Cylinder )]
	public float Radius { get; set; } = 8f;

	/// <summary>
	/// The height of the cylinder.
	/// </summary>
	[Group( TRANSFORM )]
	[ShowIf( nameof( Shape ), TraceShape.Cylinder )]
	public float Height { get; set; } = 32f;

	[InlineEditor]
	[Group( TRANSFORM )]
	[ShowIf( nameof( Shape ), TraceShape.Capsule )]
	public Capsule Capsule { get; set; } = new( Vector3.Forward * 12f, Vector3.Backward * 12f, 8f );

	[Title( "Filter" )]
	[Group( TRACE ), Order( 100 )]
	public TraceFilter Filter { get; set; }

	public TraceSettings() { }

	public TraceSettings( in TraceShape shape, in Rotation? r = null, TraceFilter filter = null )
	{
		Shape = shape;
		ShapeRotation = r ?? Rotation.Identity;

		Filter = filter;
	}

	/// <summary>
	/// Draws the trace's shape according to its configuration.
	/// </summary>
	public readonly bool DrawGizmos( Transform tWorld, in Color cLines, in Color cSolid )
	{
		if ( ShapeRotation != default )
			tWorld.Rotation *= ShapeRotation;

		tWorld.Scale = 1f;

		return Shape switch
		{
			TraceShape.Line => Gizmo.Draw.DepthArrow( Vector3.Zero, Vector3.Forward * 64f, cSolid, len: 8f, w: 3f, tWorld: tWorld ),
			TraceShape.Box => Gizmo.Draw.DepthBox( Box, cLines, cSolid, tWorld: tWorld ),
			TraceShape.Sphere => Gizmo.Draw.DepthSphere( Radius, Vector3.Zero, cLines, cSolid, tWorld: tWorld ),
			TraceShape.Capsule => Gizmo.Draw.DepthCapsule( Capsule, cLines, cSolid, tWorld: tWorld ),
			TraceShape.Cylinder => Gizmo.Draw.DepthCylinder( Radius, Height, cLines, cSolid, tWorld: tWorld ),
			_ => false,
		};
	}

	public readonly SceneTrace Build( Scene sc, in Vector3 from, in Vector3 to, Rotation? rOffset = null )
	{
		if ( !sc.IsValid() )
			return default;

		var tr = Shape switch
		{
			TraceShape.Line => sc.Trace.Ray( from, to ),
			TraceShape.Box => sc.Trace.Box( Box, from, to ),
			TraceShape.Sphere => sc.Trace.Sphere( Radius, from, to ),
			TraceShape.Capsule => sc.Trace.Capsule( Capsule, from, to ),
			TraceShape.Cylinder => sc.Trace.Cylinder( Height, Radius, from, to ),
			_ => default
		};

		// Shape Rotation
		var r = Rotation.Identity;

		if ( ShapeRotation != default )
			r *= ShapeRotation;

		if ( rOffset.HasValue )
			r *= rOffset.Value;

		tr = tr.Rotated( r );

		// Tagging
		if ( Filter.IsValid() )
		{
			if ( Filter.TagsHit.Enabled )
				tr = tr.WithAnyTags( Filter.TagsHit.Tags );

			if ( Filter.TagsIgnore.Enabled && !Filter.UseTagWhitelist )
				tr = tr.WithoutTags( Filter.TagsIgnore.Tags );

			if ( Filter.TagsRequire.Enabled )
				tr = tr.WithAllTags( Filter.TagsRequire.Tags );

			// Hitboxes
			tr = tr.UseHitboxes( Filter.UseHitboxes );

			// Trigger Hitting
			if ( Filter.TriggerType is TraceTriggerType.Include )
				tr = tr.HitTriggers();
			else if ( Filter.TriggerType is TraceTriggerType.Exclusive )
				tr = tr.HitTriggersOnly();
		}

		return tr;
	}

	public readonly SceneTrace Build( GameObject obj, in Vector3 from, in Vector3 to, bool ignoreObject = true, bool useRotation = true )
	{
		if ( !obj.IsValid() )
			return default;

		Rotation? rOffset = useRotation ? obj.WorldRotation : null;

		var tr = Build( obj.Scene, in from, in to, rOffset );

		if ( ignoreObject )
			tr = tr.IgnoreGameObjectHierarchy( obj );

		return tr;
	}

	public readonly bool PassesWhitelist( SceneTraceResult tr )
	{
		if ( !Filter.IsValid() )
			return true;

		if ( !Filter.UseTagWhitelist || tr.GameObject?.Tags is null )
			return true;

		if ( Filter.TagsIgnore.HasAny( tr.GameObject.Tags ) )
			return Filter.TagsHit.HasAny( tr.GameObject.Tags );

		return true;
	}

	public readonly SceneTraceResult Run( in SceneTrace tr )
	{
		var result = tr.Run();

		if ( !PassesWhitelist( result ) )
			return default;

		return result;
	}

	public readonly IEnumerable<SceneTraceResult> RunAll( in SceneTrace tr )
	{
		var results = tr.RunAll();

		if ( Filter.UseTagWhitelist )
			return results.Where( PassesWhitelist );

		return results;
	}

	public readonly SceneTraceResult Run( Scene sc, in Vector3 from, in Vector3 to, Rotation? rOffset = null )
		=> Run( Build( sc, from, to, rOffset ) );

	public readonly IEnumerable<SceneTraceResult> RunAll( Scene sc, in Vector3 from, in Vector3 to, Rotation? rOffset = null )
		=> RunAll( Build( sc, from, to, rOffset ) );

	public readonly SceneTraceResult Run( GameObject obj, in Vector3 from, in Vector3 to, bool ignoreObject = true, bool useRotation = true )
		=> Run( Build( obj, from, to, ignoreObject, useRotation ) );

	public readonly IEnumerable<SceneTraceResult> RunAll( GameObject obj, in Vector3 from, in Vector3 to, bool ignoreObject = true, bool useRotation = true )
		=> RunAll( Build( obj, from, to, ignoreObject, useRotation ) );
}
