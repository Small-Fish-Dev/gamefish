using GameFish;
using ShrimpleCharacterController;

namespace Fishbox;

partial class FishboxController : IScenePhysicsEvents
{
	void IScenePhysicsEvents.PrePhysicsStep()
	{
		if ( !Scene.IsValid() || IsProxy )
			return;

		var vMove = Velocity * Scene.FixedDelta;
		var tr = TraceColliders( WorldPosition, vMove );

		if ( !tr.Hit || tr.StartedSolid )
			return;

		// Move towards the surface we'll hit with some skin between.
		TryStickToSurface( tr, vMove.Normal );

		// Negative velocity towards this surface.
		var awaySpeed = Velocity.Forward( tr.Normal ).Dot( tr.Normal );

		if ( awaySpeed < 0f )
			Velocity = Velocity.Horizontal( tr.Normal );

		// Velocity.Separate( tr.Normal, out var upVel, out var hVel );
		// Velocity = upVel + hVel;
	}

	void IScenePhysicsEvents.PostPhysicsStep()
	{
		if ( !Scene.IsValid() || IsProxy )
			return;

		TryUnstuck();
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
