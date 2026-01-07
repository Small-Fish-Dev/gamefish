using GameFish;
using ShrimpleCharacterController;

namespace Fishbox;

partial class FishboxController : IScenePhysicsEvents
{
	[Property]
	[Feature( PLAYER ), Group( PHYSICS )]
	public SphereCollider HeadSphere { get; set; }

	[Property]
	[Feature( PLAYER ), Group( PHYSICS )]
	public HullCollider BodyCylinder { get; set; }

	[Property]
	[Range( 1f, 32f, clamped: false )]
	[Feature( PLAYER ), Group( PHYSICS )]
	public float Radius { get; set; } = 12f;

	[Property]
	[Range( 0f, 90f, clamped: false )]
	[Feature( PLAYER ), Group( PHYSICS )]
	public float GroundAngle { get; set; } = 50f;

	[Property]
	[Range( 1f, 10f, clamped: false )]
	[Feature( PLAYER ), Group( PHYSICS )]
	public float GroundCheckDistance { get; set; } = 4f;

	[Property]
	[Range( 1f, 32f, clamped: false )]
	[Feature( PLAYER ), Group( PHYSICS )]
	public float GroundStickDistance { get; set; } = 24f;

	[Property]
	[Range( 0.1f, 5f, clamped: false )]
	[Feature( PLAYER ), Group( PHYSICS )]
	public float GroundSkinWidth { get; set; } = 1f;

	[Property]
	[Feature( PLAYER ), Group( PHYSICS )]
	public TagSet IgnoreTags { get; set; } = [];

	public virtual Vector3 TraceOffset => GetLocalCenter();

	/// <summary> What's up? </summary>
	public virtual Vector3 Up => WorldRotation.Up;

	protected TraceResult GroundTrace { get; set; }

	void IScenePhysicsEvents.PrePhysicsStep()
	{
		if ( !Scene.IsValid() )
			return;

		var vMove = Velocity * Scene.FixedDelta;
		var tr = TraceColliders( WorldPosition, vMove, SkinWidth );

		if ( !tr.Hit || tr.StartedSolid )
			return;

		// Move towards the surface we'll hit with some skin between.
		StickToSurface( tr, vMove.Normal, SkinWidth );

		// Negative velocity towards this surface.
		Velocity.Separate( tr.Normal, out var upVel, out var hVel );
		upVel = upVel.Dot( tr.Normal ).Positive();

		Velocity = upVel + hVel;
	}

	void IScenePhysicsEvents.PostPhysicsStep()
	{

	}

	public override SceneTrace BuildTrace()
	{
		if ( !Scene.IsValid() )
			return default;

		var tr = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( IgnoreTags )
			.Rotated( WorldRotation );

		return tr;
	}

	public virtual TraceResult TraceColliders( in Vector3 startPos, in Vector3 vDelta, in float skin )
		=> TraceColliders( WorldTransform.WithPosition( startPos ), in vDelta, in skin );

	public virtual TraceResult TraceColliders( in Transform tWorld, in Vector3 vDelta, in float skin )
	{
		var radius = Radius * tWorld.Scale.x.Abs();

		var totalHeight = GetTotalHeight();
		var bodyHeight = GetBodyHeight( totalHeight );

		var bodyOffset = GetBodyWorldOffset( tWorld, in totalHeight );
		var headOffset = GetHeadWorldOffset( tWorld, in totalHeight );

		var dir = vDelta.Normal;
		var endPos = tWorld.Position + vDelta + (dir * skin);

		var bodyStart = tWorld.Position + bodyOffset;
		var bodyEnd = endPos + bodyOffset;

		var headStart = tWorld.Position + headOffset;
		var headEnd = endPos + headOffset;

		var trBase = BuildTrace();

		var trBody = trBase.Cylinder( bodyHeight, radius, bodyStart, bodyEnd ).Run();
		var trHead = trBase.Sphere( radius, headStart, headEnd ).Run();

		return new( in skin, in tWorld, in dir, in bodyOffset, in headOffset, in trBody, in trHead );
	}

	protected virtual bool IsValidGround( in TraceResult tr )
	{
		if ( !tr.Hit || tr.StartedSolid )
			return false;

		if ( tr.Normal.AlmostEqual( 0f ) )
			return false;

		return Up.Angle( tr.Normal ) <= GroundAngle;
	}

	protected virtual void StickToSurface( in TraceResult tr, in Vector3 dir, in float? withSkin = null )
	{
		if ( !Rigidbody.IsValid() || !Rigidbody.PhysicsBody.IsValid() )
			return;

		if ( !tr.TryGetEndPosition( out var destPos ) )
			return;

		var skin = withSkin ?? tr.Skin;

		if ( IsValidGround( in tr ) )
			destPos += Up * GroundSkinWidth;
		else
			destPos += tr.Normal * skin;

		if ( tr.StartedSolid )
			destPos = tr.StartPosition - (dir * skin);
		else
			destPos -= dir * skin;

		var tDest = WorldTransform.WithPosition( destPos );

		var vel = Velocity;
		Rigidbody.PhysicsBody.Transform = tDest;
		Velocity = vel;
	}

	protected virtual Vector3 GetLocalCenter()
		=> Vector3.Up * LocalEyePosition.z * 0.5f;

	protected virtual float GetTotalHeight()
		=> ((GetLocalCenter().z * 2f) + Radius.Positive().Min( 8f )).Max( Radius );


	protected Vector3 GetBodyWorldOffset( in Transform tWorld, in float totalHeight )
	{
		var offset = GetLocalBodyCenter( totalHeight );
		return tWorld.Rotation * offset * tWorld.Scale.z;
	}

	protected Vector3 GetWorldBodyCenter( in Transform tWorld, in float totalHeight )
		=> tWorld.PointToWorld( GetLocalBodyCenter( in totalHeight ) );

	protected Vector3 GetLocalBodyCenter( in float totalHeight )
		=> Vector3.Up * (GetBodyHeight( in totalHeight ) / 2f);


	protected Vector3 GetHeadWorldOffset( in Transform tWorld, in float totalHeight )
	{
		var offset = GetLocalHeadCenter( totalHeight );
		return tWorld.Rotation * offset * tWorld.Scale.z;
	}

	protected Vector3 GetWorldHeadCenter( in Transform tWorld, in float totalHeight )
		=> tWorld.PointToWorld( GetLocalHeadCenter( in totalHeight ) );

	protected Vector3 GetLocalHeadCenter( in float totalHeight )
		=> Vector3.Up * (totalHeight - Radius);

	protected virtual float GetBodyHeight( in float totalHeight )
		=> totalHeight - Radius;


	protected virtual void UpdateCollision()
	{
		var totalHeight = GetTotalHeight();

		if ( BodyCylinder.IsValid() )
		{
			BodyCylinder.Radius = Radius;
			BodyCylinder.Height = GetBodyHeight( in totalHeight );
			BodyCylinder.LocalPosition = GetLocalBodyCenter( in totalHeight );
		}

		if ( HeadSphere.IsValid() )
		{
			HeadSphere.Radius = Radius;
			HeadSphere.LocalPosition = Vector3.Up * (totalHeight - Radius);
		}
	}
}
