using GameFish;
using ShrimpleCharacterController;

namespace Fishbox;

partial class FishboxController
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

	public virtual Vector3 TraceOffset => GetLocalCenter();

	/// <summary> What's up? </summary>
	public virtual Vector3 Up => WorldRotation.Up;

	protected SceneTraceResult GroundTrace { get; set; }

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

		var totalHeight = GetTotalHeight();
		var offset = GetLocalBodyCenter( totalHeight );
		offset = WorldRotation * offset * scale.z;

		var vSkin = vDelta.Normal * skin;
		var endPos = startPos + vDelta + vSkin;

		var tr = Trace( startPos, endPos, offset )
			.Cylinder( GetBodyHeight( in totalHeight ), radius ); // squadala

		// .WithoutTags( IgnoreTags )

		tr = tr.Rotated( WorldRotation );

		return tr;
	}

	public virtual void DoGroundTrace()
	{
		var origin = WorldPosition;
		var vDown = WorldRotation.Down * WorldScale.z * 5f;

		GroundTrace = TraceBody( origin, vDown, SkinWidth ).Run();

		// DebugOverlay.Trace( GroundTrace );

		if ( GroundTrace.Hit )
		{
			GroundNormal = GroundTrace.Normal;
			GroundCollider = GroundTrace.Collider;
			GroundObject = GroundTrace.GameObject;

			var upSpeed = Velocity.Forward( GroundNormal ).Dot( GroundNormal );

			IsGrounded = upSpeed <= 10f && GroundNormal.Angle( Up ) < 45f; // TEMP
		}
		else
		{
			IsGrounded = false;
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

	protected virtual void StickToSurface( in SceneTraceResult trHit, in float skinWidth = 1f )
	{
		if ( trHit.StartedSolid || !trHit.Hit )
			return;

		if ( trHit.Normal.AlmostEqual( 0f ) || !ITransform.IsValid( trHit.Normal ) )
			return;

		// The actual distance from the wall.
		var hitDist = trHit.Distance;

		// The distance to actually move.
		var moveDist = hitDist - SkinWidth.Max( skinWidth );

		if ( moveDist.AlmostEqual( 0f ) )
			return;

		// Are we moving away from the wall?
		var moveDelta = trHit.Direction * moveDist;

		if ( moveDist < 0f )
		{
			var trSkin = TraceBody( WorldPosition, moveDelta, SkinWidth ).Run();

			// DebugOverlay.Trace( trSkin );

			if ( trSkin.StartedSolid )
				return;

			// What distance should we move backwards?
			var backDist = (trSkin.Distance - SkinWidth).Positive();
			moveDelta = trSkin.Direction * backDist;
		}

		WorldPosition += moveDelta;
	}
}
