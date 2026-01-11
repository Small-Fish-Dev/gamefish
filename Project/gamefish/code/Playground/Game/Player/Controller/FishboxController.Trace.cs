using GameFish;
using Sandbox.UI;
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

	/// <summary>
	/// Should collision traces be visualized?
	/// </summary>
	[Property]
	[Title( "Draw Traces" )]
	[Feature( PLAYER ), Group( PHYSICS )]
	public bool DebugRenderTraces { get; set; } = false;

	public virtual Vector3 TraceOffset => GetLocalCenter();

	/// <summary> What's up? </summary>
	public Vector3 Up => WorldRotation.Up;
	public Vector3 Down => WorldRotation.Down;
	public Vector3 Right => WorldRotation.Right;

	public float Scale => WorldScale.z.Abs();

	protected TraceResult GroundTrace { get; set; }

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
	/// Traces our colliders at the current position.
	/// </summary>
	public TraceResult TraceAtPosition( in TraceSettings? s = null )
		=> TraceTransform( WorldTransform, s );

	/// <summary>
	/// Traces our colliders if we were at that position.
	/// </summary>
	public TraceResult TraceAtPosition( in Vector3 worldPos, in TraceSettings? s = null )
		=> TraceTransform( WorldTransform.WithPosition( worldPos ), s );

	/// <summary>
	/// Traces our colliders as they are at the given transform.
	/// </summary>
	public TraceResult TraceTransform( in Transform tWorld, in TraceSettings? s = null )
		=> TraceDelta( tWorld, Vector3.Zero, s );

	public TraceResult TraceDelta( in Vector3 startPos, in Vector3 vDelta, in TraceSettings? s = null )
		=> TraceDelta( WorldTransform.WithPosition( startPos ), in vDelta, s );

	public virtual TraceResult TraceDelta( Transform tWorld, in Vector3 vDelta, in TraceSettings? s = null )
	{
		var scale = WorldScale.z;
		var settings = s ?? new( skin: SkinWidth * scale );
		var skin = settings.Skin;

		var radius = (Radius * scale) - skin;
		var totalHeight = (GetTotalHeight() * scale) - skin;
		var bodyHeight = GetBodyHeight( totalHeight );

		var vSkinOffset = Up * skin;
		var bodyOffset = GetBodyWorldOffset( tWorld, in totalHeight ) + vSkinOffset;
		var headOffset = GetHeadWorldOffset( tWorld, in totalHeight ) + vSkinOffset;

		var endPos = tWorld.Position + vDelta + (vDelta.Normal * skin * 2f);

		var bodyStart = tWorld.Position + bodyOffset;
		var bodyEnd = endPos + bodyOffset;

		var headStart = tWorld.Position + headOffset;
		var headEnd = endPos + headOffset;

		var trBase = BuildTrace();

		var trBody = trBase.Cylinder( bodyHeight, radius, bodyStart, bodyEnd ).Run();
		var trHead = trBase.Sphere( radius, headStart, headEnd ).Run();

		if ( DebugRenderTraces )
		{
			DebugOverlay.Trace( trBody );
			DebugOverlay.Trace( trHead );
		}

		return new( in settings, in tWorld, in vDelta, in trBody, in trHead );
	}

	protected virtual bool IsValidGround( in TraceResult tr )
	{
		if ( !tr.Hit || tr.StartedSolid )
			return false;

		if ( tr.Normal.AlmostEqual( 0f ) )
			return false;

		if ( !TryGetGroundNormal( out var vNormal ) )
			return false;

		return vNormal.Angle( tr.Normal ) <= GroundAngle;
	}

	public virtual bool TryStickToSurface( in TraceResult trSurface, float skin = 0.5f, in bool slideVel = true )
	{
		if ( !Scene.IsValid() )
			return false;

		if ( trSurface.StartedSolid || !trSurface.Hit )
			return false;

		Vector3 vDelta;

		if ( IsValidGround( in trSurface ) && TryGetGroundNormal( out var vUp ) )
		{
			vDelta = vUp * skin;
		}
		else
		{
			if ( trSurface.Normal.AlmostEqual( 0f ) ) // idk man
				return false;

			if ( IsWallRunning && IsValidForWallRunning( trSurface ) )
				vDelta = trSurface.Normal * (WallRunStickDistance * Scale).Max( skin );
			else
				vDelta = trSurface.Normal * skin;
		}

		// Is the position we've decided on free?
		var trSkin = TraceDelta( trSurface.EndPosition + vDelta, -vDelta );

		if ( !trSkin.StartedSolid )
		{
			var endPos = trSkin.StartPosition;

			if ( slideVel )
			{
				if ( trSkin.Hit && !trSkin.Normal.AlmostEqual( 0f ) )
					Velocity = Velocity.ProjectAndScale( trSkin.Normal );
			}

			SetPhysicsPosition( endPos );

			return true;
		}

		return false;
	}

	public virtual bool TryStep( in TraceResult tr )
	{
		if ( !tr.Hit )
			return false;

		var vRemaining = tr.Delta - (tr.Delta.Normal * tr.Distance);

		return TryStep( tr.StartPosition, vRemaining, tr.Normal, 32f * Scale, vRemaining.Length );
	}

	public virtual bool TryStep( in Vector3 startPos, in Vector3 vMove, in Vector3 vNormal, float stepHeight, float hDist )
	{
		if ( stepHeight < float.Epsilon || hDist < float.Epsilon )
			return false;

		if ( vMove.AlmostEqual( 0f ) || vNormal.AlmostEqual( 0f ) )
			return false;

		// Align the movement axis along the hit normal.
		var vForward = Vector3.VectorPlaneProject( vMove, Up ).Normal;
		var vUpward = Vector3.VectorPlaneProject( Up, vForward ).Normal;
		hDist = Vector3.VectorPlaneProject( vMove, vUpward ).Length;

		var trUpward = TraceDelta( startPos, vUpward * stepHeight );

		// DebugOverlay.Trace( trUpward.BodyTrace, duration: 3f );

		if ( trUpward.StartedSolid )
			return false;

		var vPos = trUpward.EndPosition;
		var trForward = TraceDelta( vPos, vForward * hDist );
		vPos = trForward.EndPosition;

		// DebugOverlay.Trace( trForward.BodyTrace, duration: 3f );

		if ( trForward.StartedSolid )
			return false;

		var trDown = TraceDelta( vPos, vUpward * (stepHeight * -2f) );

		// DebugOverlay.Trace( trDown.BodyTrace, duration: 3f );

		if ( trDown.StartedSolid || !trDown.Hit )
			return false;

		if ( !TryStickToSurface( trDown ) )
			return false;

		Transform.ClearInterpolation();

		return true;
	}

	public virtual bool TryUnstuck()
	{
		var trGrown = TraceTransform( WorldTransform );

		if ( !trGrown.StartedSolid )
			return true;

		if ( DebugLogUnstuck )
			this.Log( "Stuck in something!" );

		if ( TryUnstuck( attemptsRemaining: 12 ) )
		{
			if ( DebugLogUnstuck )
				this.Log( "Got unstuck." );

			return true;
		}

		return false;
	}

	protected virtual bool TryUnstuck( in int attemptsRemaining, in int depth = 0 )
	{
		// Something's definitely gone wrong by now!
		// If you really need to then just run this again.
		const int depthLimit = 99;

		if ( depth >= depthLimit )
		{
			this.Log( $"Unstuck attempt reached the limit at #{depth}." );
			return false;
		}

		var trStuck = TraceTransform( WorldTransform );

		if ( !trStuck.Hit || !trStuck.StartedSolid )
			return true;

		var fudgeDir = trStuck.HitPosition.Direction( trStuck.HitTrace.StartPosition );

		var radius = Radius * WorldScale.x;
		var skin = radius * 0.25f;

		var startPos = trStuck.StartPosition;
		var freePos = startPos - (fudgeDir * skin * (depth + 1));

		var toOriginDelta = startPos - freePos;
		var trShrunk = TraceDelta( freePos, toOriginDelta, new( skin: skin ) );

		if ( trShrunk.StartedSolid )
			goto NextAttempt;

		SetPhysicsPosition( freePos );

		return true;

		// var trTest = TraceColliders( freePos, toOriginDelta, new( grow: 0f, skin: fat ) );

		// if ( !trTest.StartedSolid )
		// SetPhysicsPosition( freePos );

		// return true;

		NextAttempt:

		// TODO: Trace with escalating desparation depending on depth.
		if ( attemptsRemaining <= 1 )
			return false;

		return TryUnstuck( attemptsRemaining - 1, depth.Positive() + 1 );
	}
}
