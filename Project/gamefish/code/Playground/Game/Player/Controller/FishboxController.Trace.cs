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

	/// <summary>
	/// Should (un)stuck events be debug logged?
	/// </summary>
	[Property]
	[Title( "Log Unstuck" )]
	[Feature( PLAYER ), Group( PHYSICS )]
	public bool DebugLogUnstuck { get; set; } = false;

	public virtual Vector3 TraceOffset => GetLocalCenter();

	/// <summary> What's up? </summary>
	public Vector3 Up => WorldRotation.Up;
	public Vector3 Down => WorldRotation.Down;
	public Vector3 Right => WorldRotation.Right;

	protected TraceResult GroundTrace { get; set; }

	void IScenePhysicsEvents.PrePhysicsStep()
	{
		if ( !Scene.IsValid() )
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
		TryUnstuck();
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

	/// <summary>
	/// Traces our colliders sized up to our skin at the current position.
	/// </summary>
	public TraceResult TraceSkin()
		=> TraceSkin( WorldTransform );

	/// <summary>
	/// Traces our colliders sized up to our skin at the given transform.
	/// </summary>
	public TraceResult TraceSkin( Transform tWorld )
		=> TraceColliders( tWorld, Vector3.Zero, new( fGrow: SkinWidth, fSkin: 0f ) );

	public TraceResult TraceColliders( in Vector3 startPos, in Vector3 vDelta, in TraceSettings? s = null )
		=> TraceColliders( WorldTransform.WithPosition( startPos ), in vDelta, s ?? new( 0f, SkinWidth ) );

	public virtual TraceResult TraceColliders( Transform tWorld, in Vector3 vDelta, in TraceSettings s )
	{
		var grow = s.Grow;
		var skin = (s.Skin - grow).Positive();

		var radius = (Radius * tWorld.Scale.x.Abs()) + grow;
		var totalHeight = GetTotalHeight() + (grow * 2f);
		var bodyHeight = GetBodyHeight( totalHeight );

		tWorld.Position -= Up * grow;
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

		// DebugOverlay.Trace( trBody );
		// DebugOverlay.Trace( trHead );

		return new( skin, in tWorld, in vDelta, in trBody, in trHead );
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
		=> SetPhysicsTransform( WorldTransform.WithPosition( pos ) );

	public virtual void SetPhysicsTransform( Transform tWorld )
	{
		if ( !Rigidbody.IsValid() || !Rigidbody.PhysicsBody.IsValid() )
			return;

		var vel = Velocity;
		Rigidbody.PhysicsBody.Transform = tWorld;
		Velocity = vel;
	}

	public virtual bool TryUnstuck()
	{
		var trSkin = TraceSkin( WorldTransform );

		if ( !trSkin.StartedSolid )
			return true;

		if ( DebugLogUnstuck )
			this.Log( "Stuck in something!" );

		if ( !TryUnstuck( trSkin ) )
			return false;

		if ( DebugLogUnstuck )
			this.Log( "Got unstuck." );

		return true;
	}

	protected virtual bool TryUnstuck( in TraceResult trStuck, in int attemptsRemaining = 10, in int depth = 0 )
	{
		// Something's definitely gone wrong by now!
		// If you really need to then just run this again.
		if ( depth > 99 )
			return false;

		// Try to get some kind of direction away from what we're stuck in.
		Vector3 fudgeDir;

		if ( trStuck.Hit && !trStuck.Delta.AlmostEqual( 0f ) )
			fudgeDir = trStuck.Delta.Normal;
		else
			fudgeDir = Vector3.Random.Normal;

		var freePos = WorldPosition - (fudgeDir * Random.Float( depth ) + 1);
		var trFree = TraceColliders( freePos, fudgeDir, new( 0f, 0f ) );

		if ( !trFree.StartedSolid )
		{
			var skinDir = trFree.Hit ? trFree.Normal : fudgeDir;
			var trSkin = TraceColliders( trFree.EndPosition, skinDir, new( fGrow: SkinWidth, fSkin: 0f ) );

			if ( !trSkin.StartedSolid )
			{
				if ( trSkin.Hit )
					TryStickToSurface( trSkin, fudgeDir );

				return true;
			}
		}

		// TODO: Trace with escalating desparation depending on depth.
		if ( attemptsRemaining <= 1 )
			return false;

		return TryUnstuck( trFree, attemptsRemaining - 1, depth.Positive() + 1 );
	}

	public virtual bool TryStickToSurface( in TraceResult tr, in Vector3 dir )
	{
		if ( !tr.Hit || tr.Normal.AlmostEqual( 0f ) )
			return false;

		var skin = tr.Skin.Max( SkinWidth );
		var destPos = tr.EndPosition;

		if ( IsValidGround( in tr ) )
			destPos += Up * skin;
		else if ( !tr.StartedSolid )
			destPos += (tr.Normal - dir).Normal * skin;

		SetPhysicsPosition( destPos );
		return true;
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
