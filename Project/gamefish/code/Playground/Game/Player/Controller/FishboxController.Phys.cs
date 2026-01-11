using GameFish;
using ShrimpleCharacterController;

namespace Fishbox;

partial class FishboxController : IScenePhysicsEvents, Component.ICollisionListener
{
	public virtual void SetUpDirection( Vector3 up )
	{
		if ( IsProxy || !Pawn.IsValid() || Pawn.Seat.IsValid() )
			return;

		up = up.Normal;

		if ( up.AlmostEqual( 0f ) )
			return;

		var localCenter = GetLocalCenter();
		var oldCenter = WorldTransform.PointToWorld( localCenter );

		var tEye = Pawn.EyeTransform;
		var tWorld = WorldTransform;

		// Perform the rotation.
		var flatDir = Vector3.VectorPlaneProject( tEye.Forward, up );
		tWorld.Rotation = Rotation.LookAt( flatDir, up );

		// Recenter us on our previous position.
		var newCenter = tWorld.PointToWorld( localCenter );
		tWorld.Position += oldCenter - newCenter;

		// Update transform afterwards.
		SetPhysicsTransform( tWorld );

		// Set and correct our eye aim/origin.
		Pawn.EyePosition = tEye.Position;
		Pawn.EyeRotation = tEye.Rotation;
	}

	void ICollisionListener.OnCollisionStart( Collision c )
	{
		if ( !Scene.IsValid() || IsProxy )
			return;

		var tr = TraceDelta( WorldPosition, Velocity * Scene.FixedDelta );

		OnHitSurface( in tr );
	}

	void IScenePhysicsEvents.PrePhysicsStep()
	{
		if ( !Scene.IsValid() || IsProxy )
			return;

		var vMove = WishVelocity * Scene.FixedDelta;
		var tr = TraceDelta( WorldPosition, vMove );

		OnHitSurface( in tr );
	}

	void IScenePhysicsEvents.PostPhysicsStep()
	{
		if ( !Scene.IsValid() || IsProxy )
			return;

		// TryUnstuck();
	}

	public virtual void OnHitSurface( in TraceResult tr )
	{
		if ( !tr.Hit || tr.StartedSolid )
			return;

		// Negate downward velocity along the ground.
		if ( tr.Hit && IsValidGround( in tr ) )
		{
			var vDown = GravityDirection;
			var downSpeed = Velocity.Forward( vDown ).Dot( vDown );

			if ( downSpeed > 0f )
				Velocity = Velocity.Horizontal( vDown );
		}

		// Project velocty that is pushing this wall along its surface.
		var wallDir = -tr.Normal;
		var wallPush = Velocity.Forward( wallDir ).Dot( wallDir );

		if ( wallPush > 0f )
			Velocity = Vector3.VectorPlaneProject( Velocity, tr.Normal );
	}


	public void SetPhysicsPosition( Vector3 pos, in bool updateParent = true )
		=> SetPhysicsTransform( WorldTransform.WithPosition( pos ) );

	public virtual void SetPhysicsTransform( Transform tWorld, in bool updateParent = true )
	{
		// Don't set transform while seated.
		if ( Pawn.IsValid() && Pawn.Seat.IsValid() )
			return;

		// var vel = Velocity;

		WorldTransform = tWorld;

		// var phys = Rigidbody?.PhysicsBody;

		// if ( phys.IsValid() )
		// phys.Position = tWorld.Position;

		// Velocity = vel;
	}


	protected virtual void UpdateCollision()
	{
		if ( Rigidbody.IsValid() )
			Rigidbody.EnhancedCcd = true;

		var totalHeight = GetTotalHeight();

		if ( BodyCylinder.IsValid() )
		{
			BodyCylinder.Radius = Radius;
			BodyCylinder.Radius2 = Radius;
			BodyCylinder.Height = GetBodyHeight( in totalHeight );
			BodyCylinder.LocalPosition = GetLocalBodyCenter( in totalHeight );
		}

		if ( HeadSphere.IsValid() )
		{
			HeadSphere.Radius = Radius;
			HeadSphere.LocalPosition = Vector3.Up * (totalHeight - Radius);
		}
	}


	protected virtual Vector3 GetLocalCenter()
		=> Vector3.Up * LocalEyePosition.z * 0.5f;

	protected virtual float GetTotalHeight()
		=> ((GetLocalCenter().z * 2f) + Radius.Positive().Min( 8f )).Max( Radius );


	protected virtual float GetBodyHeight( in float totalHeight )
		=> totalHeight - Radius;


	protected Vector3 GetWorldBodyCenter( in Transform tWorld, in float totalHeight )
		=> tWorld.PointToWorld( GetLocalBodyCenter( in totalHeight ) );

	protected Vector3 GetLocalBodyCenter( in float totalHeight )
		=> Vector3.Up * (GetBodyHeight( in totalHeight ) / 2f);

	protected Vector3 GetBodyWorldOffset( in Transform tWorld, in float totalHeight )
	{
		var offset = GetLocalBodyCenter( totalHeight );
		return tWorld.Rotation * offset * tWorld.Scale.z;
	}


	protected Vector3 GetWorldHeadCenter( in Transform tWorld, in float totalHeight )
		=> tWorld.PointToWorld( GetLocalHeadCenter( in totalHeight ) );

	protected Vector3 GetLocalHeadCenter( in float totalHeight )
		=> Vector3.Up * (totalHeight - Radius);

	protected Vector3 GetHeadWorldOffset( in Transform tWorld, in float totalHeight )
	{
		var offset = GetLocalHeadCenter( totalHeight );
		return tWorld.Rotation * offset * tWorld.Scale.z;
	}
}
