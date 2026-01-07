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

	public virtual Vector3 TraceOffset => GetLocalCenter();

	/// <summary> What's up? </summary>
	public virtual Vector3 Up => WorldRotation.Up;

	protected SceneTraceResult GroundTrace { get; set; }

	void IScenePhysicsEvents.PrePhysicsStep()
	{
		if ( !Scene.IsValid() )
			return;

		var vMove = Velocity * Scene.FixedDelta;
		var tr = TraceBody( WorldPosition, vMove, SkinWidth ).Run();

		if ( !tr.Hit || tr.StartedSolid )
			return;

		// Move towards the surface we'll hit with some skin between.
		StickToSurface( tr, vMove.Normal, skin: SkinWidth );

		// Negative velocity towards this surface.
		Velocity.Separate( tr.Normal, out var upVel, out var hVel );
		upVel = upVel.Dot( tr.Normal ).Positive();

		Velocity = upVel + hVel;
	}

	void IScenePhysicsEvents.PostPhysicsStep()
	{

	}

	public override SceneTrace Trace()
	{
		if ( !Scene.IsValid() )
			return default;

		var tr = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject );

		return tr;
	}

	public virtual SceneTrace TraceBody( in Vector3 startPos, in Vector3 vDelta, in float skin )
	{
		var scale = WorldScale;
		var radius = Radius * scale.x.Abs();

		var tWorld = WorldTransform;
		var totalHeight = GetTotalHeight();

		var offset = GetBodyWorldOffset( tWorld, in totalHeight );

		var vSkin = vDelta.Normal * skin;
		var endPos = startPos + vDelta + vSkin;

		var tr = Trace( startPos, endPos, offset )
			.Cylinder( GetBodyHeight( in totalHeight ), radius ); // squadala

		// .WithoutTags( IgnoreTags )

		tr = tr.Rotated( WorldRotation );

		return tr;
	}

	protected virtual bool IsValidGround( in SceneTraceResult tr )
	{
		if ( !tr.Hit || tr.StartedSolid )
			return false;

		return Up.Angle( tr.Normal ) <= GroundAngle;
	}

	protected virtual void StickToSurface( in SceneTraceResult trBody, in Vector3 dir, in float skin = 1f )
	{
		if ( !Rigidbody.IsValid() || !Rigidbody.PhysicsBody.IsValid() )
			return;

		Vector3 destPos = trBody.EndPosition;

		if ( IsValidGround( in trBody ) )
			destPos += Up * GroundSkinWidth;
		else
			destPos += trBody.Normal * skin;

		if ( trBody.StartedSolid )
			destPos = trBody.StartPosition - (dir * skin);
		else
			destPos -= dir * skin;

		destPos -= BodyWorldOffset;
		var tDest = WorldTransform.WithPosition( destPos );

		var vel = Velocity;
		Rigidbody.PhysicsBody.Transform = tDest;
		Velocity = vel;
	}

	protected virtual Vector3 GetLocalCenter()
		=> Vector3.Up * LocalEyePosition.z * 0.5f;

	protected virtual float GetTotalHeight()
		=> ((GetLocalCenter().z * 2f) + Radius.Positive().Min( 8f )).Max( Radius );

	protected virtual float GetBodyHeight( in float totalHeight )
		=> totalHeight - Radius;

	public Vector3 BodyWorldOffset => GetBodyWorldOffset( WorldTransform, GetTotalHeight() );

	protected Vector3 GetBodyWorldOffset( in Transform tWorld, in float totalHeight )
	{
		var offset = GetLocalBodyCenter( totalHeight );
		return tWorld.Rotation * offset * tWorld.Scale.z;
	}

	protected Vector3 GetWorldBodyCenter( in Transform tWorld, in float totalHeight )
		=> tWorld.PointToWorld( GetLocalBodyCenter( in totalHeight ) );

	protected Vector3 GetWorldHeadCenter( in Transform tWorld, in float totalHeight )
		=> tWorld.PointToWorld( GetLocalHeadCenter( in totalHeight ) );

	protected Vector3 GetLocalBodyCenter( in float totalHeight )
		=> Vector3.Up * (GetBodyHeight( in totalHeight ) / 2f);

	protected Vector3 GetLocalHeadCenter( in float totalHeight )
		=> Vector3.Up * (totalHeight - Radius);

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
