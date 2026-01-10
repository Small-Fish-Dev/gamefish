using GameFish;
using ShrimpleCharacterController;

namespace Fishbox;

partial class FishboxController : IScenePhysicsEvents, Component.ICollisionListener
{
	void ICollisionListener.OnCollisionStart( Collision c )
	{
		if ( !Scene.IsValid() || IsProxy )
			return;

		var tr = TraceDelta( WorldPosition, Velocity );

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
		if ( IsValidGround( in tr ) )
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


	public void SetPhysicsPosition( Vector3 pos )
		=> SetPhysicsTransform( WorldTransform.WithPosition( pos ) );

	public virtual void SetPhysicsTransform( Transform tWorld )
	{
		if ( !Rigidbody.IsValid() || !Rigidbody.PhysicsBody.IsValid() )
			return;

		var vel = Velocity;
		Rigidbody.PhysicsBody.Transform = tWorld;
		Velocity = vel;
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
