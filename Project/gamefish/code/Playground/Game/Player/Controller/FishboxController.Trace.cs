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
	[Range( 1f, 64f, clamped: false )]
	[Feature( PLAYER ), Group( PHYSICS )]
	public float GroundStickDistance { get; set; } = 32f;

	[Property]
	[Range( 0.1f, 5f, clamped: false )]
	[Feature( PLAYER ), Group( PHYSICS )]
	public float GroundSkinWidth { get; set; } = 1f;

	/// <summary>
	/// Should (un)stuck events be debug logged?
	/// </summary>
	[Property]
	[Title( "Log Unstuck" )]
	[Feature( PLAYER ), Group( PHYSICS )]
	public bool DebugLogUnstuck { get; set; } = false;

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
		// TODO: Test stuck by expanding the body/head traces smartly.

		// Prevent getting stuck in the ground.
		if ( GroundTrace.StartedSolid )
		{
			if ( DebugLogUnstuck )
				this.Log( "Stuck in ground!" );

			if ( TryUnstuck( GroundTrace, attemptsRemaining: 5 ) )
			{
				if ( DebugLogUnstuck )
					this.Log( "Pulled out of ground." );

				return;
			}
		}

		// Prevent getting stuck in walls and such.
		var velDir = Velocity.Normal;
		var vDeltaStart = velDir * SkinWidth;
		var trVel = TraceColliders( WorldPosition, vDeltaStart, SkinWidth );

		if ( trVel.StartedSolid )
		{
			if ( DebugLogUnstuck )
				this.Log( "Stuck in something!" );

			if ( TryUnstuck( in trVel ) )
			{
				if ( DebugLogUnstuck )
					this.Log( "Got unstuck." );

				return;
			}
		}
	}

	public override SceneTrace BuildTrace()
	{
		if ( !Scene.IsValid() )
			return default;

		var tr = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject )
			.WithCollisionRules( Tags )
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

		var endPos = tWorld.Position + vDelta + (vDelta.Normal * skin);

		var bodyStart = tWorld.Position + bodyOffset;
		var bodyEnd = endPos + bodyOffset;

		var headStart = tWorld.Position + headOffset;
		var headEnd = endPos + headOffset;

		var trBase = BuildTrace();

		var trBody = trBase.Cylinder( bodyHeight, radius, bodyStart, bodyEnd ).Run();
		var trHead = trBase.Sphere( radius, headStart, headEnd ).Run();

		return new( in skin, in tWorld, in vDelta, in bodyOffset, in headOffset, in trBody, in trHead );
	}

	protected virtual bool IsValidGround( in TraceResult tr )
	{
		if ( !tr.Hit || tr.StartedSolid )
			return false;

		if ( tr.Normal.AlmostEqual( 0f ) )
			return false;

		return Up.Angle( tr.Normal ) <= GroundAngle;
	}

	public void SetPhysicsPosition( Vector3 pos )
		=> SetPhysicsTransform( new( pos ) );

	public virtual void SetPhysicsTransform( Transform tWorld )
	{
		if ( !Rigidbody.IsValid() || !Rigidbody.PhysicsBody.IsValid() )
			return;

		var vel = Velocity;
		Rigidbody.PhysicsBody.Transform = tWorld;
		Velocity = vel;
	}

	public virtual bool TryUnstuck( in TraceResult trStuck, in int attemptsRemaining = 10, in int depth = 0 )
	{
		// Something's definitely gone wrong by now!
		// If you really need to then just run this again.
		if ( depth > 99 )
			return false;

		if ( trStuck.Hit && !trStuck.StartedSolid )
			return true;

		// Try to get some kind of direction away from what we're stuck in.
		Vector3 fudgeDir;

		if ( trStuck.Hit )
			fudgeDir = trStuck.Delta.Normal;
		else
			fudgeDir = Vector3.Random.Normal;

		var endPos = WorldPosition - (fudgeDir * Random.Float( depth ) + 1);
		var trAttempt = TraceColliders( endPos, fudgeDir, SkinWidth );

		if ( !trAttempt.StartedSolid )
		{
			var trSkin = TraceColliders( endPos, fudgeDir * SkinWidth, SkinWidth );

			if ( !trSkin.StartedSolid )
			{
				SetPhysicsPosition( endPos );

				if ( trSkin.Hit )
					StickToSurface( trSkin, fudgeDir );

				return true;
			}
		}

		// TODO: Trace with escalating desparation depending on depth.
		if ( attemptsRemaining <= 1 )
			return false;

		return TryUnstuck( trAttempt, attemptsRemaining - 1, depth.Positive() + 1 );
	}

	protected virtual void StickToSurface( in TraceResult tr, in Vector3 dir, in float? withSkin = null )
	{
		if ( !Rigidbody.IsValid() || !Rigidbody.PhysicsBody.IsValid() )
			return;

		if ( !tr.Hit || tr.Normal.AlmostEqual( 0f ) )
			return;

		var skin = withSkin ?? tr.Skin.Max( SkinWidth );
		var destPos = tr.EndPosition;

		if ( IsValidGround( in tr ) )
			destPos += Up * GroundSkinWidth;
		else if ( !tr.StartedSolid )
			destPos += (tr.Normal - dir).Normal * skin;

		SetPhysicsPosition( destPos );
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
