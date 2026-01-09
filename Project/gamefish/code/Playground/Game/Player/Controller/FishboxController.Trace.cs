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
	/// Traces our colliders sized up to our skin if we were at that position.
	/// </summary>
	public TraceResult TraceSkin( in Vector3 worldPos )
		=> TraceSkin( WorldTransform.WithPosition( worldPos ) );

	/// <summary>
	/// Traces our colliders sized up to our skin at the given transform.
	/// </summary>
	public TraceResult TraceSkin( in Transform tWorld )
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

		if ( trStuck.Hit )
			fudgeDir = trStuck.HitPosition.Direction( trStuck.HitTrace.StartPosition );
		else
			fudgeDir = Rotation.Identity.ClosestAxis( Vector3.Random.Normal );

		var startPos = trStuck.StartPosition;
		var freePos = startPos - (fudgeDir * Random.Float( depth ) + 1);
		var trFree = TraceSkin( freePos );

		if ( !trFree.StartedSolid )
		{
			var skinDir = freePos.Direction( startPos );
			var radius = skinDir * Radius * WorldScale.x;
			var trSkin = TraceColliders( trFree.EndPosition, radius, new( fGrow: SkinWidth, fSkin: 0f ) );

			if ( !trSkin.StartedSolid && trSkin.Hit )
				if ( TryStickToSurface( trSkin, skinDir ) )
					return true;
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
}
