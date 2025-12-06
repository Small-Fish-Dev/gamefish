using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Configures relevant information about a trace to be performed.
/// </summary>
public partial struct TraceSettings
{
	/// <summary>
	/// The shape of the trace.
	/// </summary>
	public TraceShape Shape { get; set; } = TraceShape.Box;

	/// <summary>
	/// The shape's rotation.
	/// </summary>
	[Title( "Rotation" )]
	public Rotation ShapeRotation { get; set; } = Rotation.Identity;

	[ShowIf( nameof( Shape ), TraceShape.Box )]
	public BBox Box { get; set; } = BBox.FromPositionAndSize( Vector3.Zero, 16f );

	/// <summary>
	/// The radius of the sphere/cylinder.
	/// </summary>
	[ShowIf( nameof( Shape ), TraceShape.Sphere )]
	[ShowIf( nameof( Shape ), TraceShape.Cylinder )]
	public float Radius { get; set; } = 8f;

	/// <summary>
	/// The height of the cylinder.
	/// </summary>
	[ShowIf( nameof( Shape ), TraceShape.Cylinder )]
	public float Height { get; set; } = 32f;

	[InlineEditor]
	[ShowIf( nameof( Shape ), TraceShape.Capsule )]
	public Capsule Capsule { get; set; } = new( Vector3.Forward * 12f, Vector3.Backward * 12f, 8f );

	/// <summary>
	/// If enabled: only consider objects with ANY of these tags.
	/// </summary>
	[Title( "Tags (hit)" )]
	public TagFilter TagsHit { get; set; } = new( false, ["solid", TAG_PAWN] );

	/// <summary>
	/// If enabled: ignore objects with ANY of these tags.
	/// </summary>
	[Title( "Tags (ignore)" )]
	public TagFilter TagsIgnore { get; set; } = new( false, [TAG_TRIGGER] );

	/// <summary>
	/// If enabled: only trace against objects that have ALL of these tags.
	/// </summary>
	[Title( "Tags (require)" )]
	public TagFilter TagsRequire { get; set; } = new( false, [] );

	/// <summary>
	/// How should this trace treat triggers/solids?
	/// </summary>
	[Title( "Triggers" )]
	public TraceTriggerHitType TriggerType { get; set; }

	public TraceSettings() { }

	public TraceSettings( in TraceShape shape, in Rotation r, TagSet hit = null, TagSet ignore = null )
	{
		Shape = shape;
		ShapeRotation = r;

		TagsHit = new( hit );
		TagsIgnore = new( ignore );
	}

	/// <summary>
	/// Draws the trace's shape according to its configuration.
	/// </summary>
	public readonly bool DrawGizmos( Transform tWorld, in Color cLines, in Color cSolid )
	{
		if ( ShapeRotation != default )
			tWorld.Rotation *= ShapeRotation;

		return Shape switch
		{
			TraceShape.Line => Gizmo.Draw.DepthArrow( Vector3.Zero, Vector3.Forward, cSolid, tWorld: tWorld ),
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
		if ( TagsHit.Enabled )
			tr = tr.WithAnyTags( TagsHit.Tags );

		if ( TagsRequire.Enabled )
			tr = tr.WithAllTags( TagsRequire.Tags );

		if ( TagsIgnore.Enabled )
			tr = tr.WithoutTags( TagsIgnore.Tags );

		// Trigger Hitting
		if ( TriggerType is TraceTriggerHitType.Include )
			tr = tr.HitTriggers();
		else if ( TriggerType is TraceTriggerHitType.Exclusive )
			tr = tr.HitTriggersOnly();

		return tr;
	}

	public readonly SceneTrace Build( GameObject obj, in Vector3 from, in Vector3 to, bool ignoreObject = true, bool useRotation = true )
	{
		if ( !obj.IsValid() )
			return default;

		var tr = Build(
			sc: obj.Scene, in from, in to,
			rOffset: useRotation ? obj.WorldRotation : null
		);

		if ( ignoreObject )
			tr = tr.IgnoreGameObjectHierarchy( obj );

		return tr;
	}

	public readonly SceneTraceResult Run( Scene sc, in Vector3 from, in Vector3 to, Rotation? rOffset = null )
		=> Build( sc, from, to, rOffset ).Run();

	public readonly IEnumerable<SceneTraceResult> RunAll( Scene sc, in Vector3 from, in Vector3 to, Rotation? rOffset = null )
		=> Build( sc, from, to, rOffset ).RunAll();

	public readonly SceneTraceResult Run( GameObject obj, in Vector3 from, in Vector3 to, bool ignoreObject = true, bool useRotation = true )
		=> Build( obj, from, to, ignoreObject, useRotation ).Run();

	public readonly IEnumerable<SceneTraceResult> RunAll( GameObject obj, in Vector3 from, in Vector3 to, bool ignoreObject = true, bool useRotation = true )
		=> Build( obj, from, to, ignoreObject, useRotation ).RunAll();
}
