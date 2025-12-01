namespace GameFish;

/// <summary>
/// Provides the <see cref="Actor"/> ways to detect targets.
/// </summary>
[Icon( "visibility" )]
public partial class ActorDetection : ActorFeature
{
	/// <summary>
	/// React to enemy footsteps within this range.
	/// </summary>
	[Property]
	[Feature( DETECTION ), Group( HEARING )]
	protected float HearingDistance { get; set; } = 256f;

	/// <summary> Always detect enemies within this range. </summary>
	[Property]
	[Feature( DETECTION ), Group( SMELL )]
	protected float SmellingDistance { get; set; } = 96f;

	/// <summary>
	/// The radius of the cone that this NPC can see things within.
	/// </summary>
	[Property]
	[Title( "Default Angle" )]
	[Range( 0f, 180f, clamped: false )]
	[Feature( DETECTION ), Group( VISION )]
	protected virtual float DefaultVisionAngle { get; set; } = 65f;

	/// <summary>
	/// How far targets can be detected.
	/// </summary>
	[Property]
	[Title( "Default Distance" )]
	[Range( 0f, 5000f, clamped: false )]
	[Feature( DETECTION ), Group( VISION )]
	protected virtual float DefaultVisionDistance { get; set; } = 3000f;

	/// <summary>
	/// How often to look for targets.
	/// </summary>
	[Property]
	[Title( "Default Frequency" )]
	[Range( 0f, 2f, clamped: false )]
	[Feature( DETECTION ), Group( VISION )]
	protected virtual float DefaultVisionFrequency { get; set; } = 0.25f;

	[Property]
	[Title( "Vision Lines" )]
	[Feature( DETECTION ), Group( DEBUG )]
	public bool DrawDebugVisionLines { get; set; }

	public virtual float VisionAngle => Mind?.GetVisionAngle( DefaultVisionAngle ) ?? DefaultVisionAngle;
	public virtual float VisionDistance => Mind?.GetVisionDistance( DefaultVisionDistance ) ?? DefaultVisionDistance;
	public virtual float VisionFrequency => Mind?.GetVisionFrequency( DefaultVisionFrequency ) ?? DefaultVisionFrequency;

	/// <summary> When we last looked for a target. </summary>
	public TimeSince? LastVisionCheck { get; set; }

	/// <summary>
	/// Is our target visible(since the previous tick)?
	/// </summary>
	[Sync] public bool TargetVisible { get; set; }

	/// <summary>
	/// Look out for enemies.
	/// </summary>
	public virtual void UpdateVision()
	{
		UpdateTargetVisibility();

		if ( LastVisionCheck.HasValue && LastVisionCheck < VisionFrequency )
			return;

		LastVisionCheck = 0f;

		if ( DrawDebugVisionLines )
			DebugOverlay.Line( EyePosition, EyePosition + EyeRotation.Forward * VisionDistance, duration: VisionFrequency );

		// We'll just look for the only possible enemy for NPCs currently.. for now.
		var enemy = Target;

		if ( enemy.IsValid() && IsPawnVisible( enemy, out var aimPos ) )
		{
			OnPawnVisible( enemy, aimPos ?? Target.Center );
			return;
		}

		// Trace in a sphere to find enemies.
		var eyePos = EyePosition;

		var enemyWithDist = Actor.GetEyeTrace()
			.Sphere( VisionDistance, eyePos, eyePos ).RunAll()
			.Select( tr => Pawn.TryGet<Pawn>( tr.GameObject, out var pawn ) ? pawn : null )
			.Where( pawn => pawn.IsValid() && IsEnemy( pawn ) )
			.Select( pawn => IsPawnVisible( pawn, out var visiblePos ) ? (pawn, visiblePos) : (null, null) )
			.Where( seen => seen.pawn.IsValid() && seen.visiblePos.HasValue )
			.OrderBy( seen => eyePos.Distance( seen.visiblePos ?? seen.pawn.Center ) )
			.FirstOrDefault();

		if ( enemyWithDist.pawn is Pawn pawn && pawn.IsValid() )
			OnTargetVisible( pawn, enemyWithDist.visiblePos ?? pawn.Center );
	}

	/// <summary>
	/// Called every tick to determine if/when our target was visible.
	/// </summary>
	public virtual void UpdateTargetVisibility()
	{
		if ( !Target.IsValid() )
			return;

		if ( IsPawnVisible( Target, out var visiblePos ) )
			OnPawnVisible( Target, visiblePos ?? Target.Center );
		else
			OnTargetVisibilityLost();
	}

	/// <summary>
	/// Called each update when actively looking at a <see cref="Pawn"/>.
	/// </summary>
	public virtual void OnPawnVisible( Pawn pawn, in Vector3 visiblePos )
	{
		if ( !pawn.IsValid() || !pawn.IsAlive )
			return;

		if ( Actor?.IsEnemy( pawn ) is true )
			OnTargetVisible( pawn, visiblePos );
	}

	/// <summary>
	/// Actively looking at someone we just don't like.
	/// </summary>
	protected virtual void OnTargetVisible( Pawn target, in Vector3 at )
	{
		if ( !target.IsValid() || !target.IsAlive )
			return;

		// this.Log( $"OnEnemyVisible: target:[{target}] at:[{at}]" );

		TargetVisible = true;

		// TODO: Take a second to notice the player? Act surprised?
		Mind?.OnTargetVisible( target, at );
	}

	/// <summary>
	/// We lost sight of our current target.
	/// </summary>
	protected virtual void OnTargetVisibilityLost()
	{
		if ( !TargetVisible )
			return;

		// if ( Target.IsValid() )
		// Log.Info( $"{this} lost sight of: {Target}" );

		TargetVisible = false;
	}

	/// <summary>
	/// The target has been out of sight for too long.
	/// </summary>
	protected virtual void OnLostTarget()
	{
		// No point if we had no recognition of a target.
		if ( !Target.IsValid() && !TargetVisible )
			return;

		// if ( Target.IsValid() )
		// Log.Info( $"{this} forgot about {Target}" );

		Target = null;
		TargetVisible = false;
	}

	public virtual bool IsPawnVisible( Pawn pawn, out Vector3? aimPos )
	{
		aimPos = null;

		if ( !pawn.IsValid() )
			return false;

		var targetPos = pawn.Center;

		if ( !IsWithinVisionCone( targetPos ) )
			return false;

		return HasLineOfSight( pawn, out aimPos );

		/*
		var centerDist = targetPos.Distance( EyePosition );

		if ( pawn.IsAlive )
		{
			// Sniff.
			if ( centerDist <= SmellingDistance )
			{
				Log.Info( $"{this} got a whiff of {pawn}!" );

				lookingAt = pawn.Center;
				return true;
			}

			// Did you hear something?
			var feetDist = EyePosition.Distance( pawn.WorldPosition );

			var scale = WorldScale.z;

			if ( feetDist <= HearingDistance * scale )
			{
				// Can't hear from behind if sneaking.
				if ( pawn.Velocity.Length > 150 )
				{
					Log.Info( $"{this} heard {pawn}!" );

					lookingAt = pawn.Center;
					return true;
				}
			}

			// Log.Info( "vision not within cone. angle: " + EyePosition.Direction( targetPos ).Angle( EyeForward ) );
			return false;
		}

		return true;
		*/
	}

	/// <returns> If the position is within our vision cone. </returns>
	public virtual bool IsWithinVisionCone( in Vector3 targetPos )
	{
		var eyePos = EyePosition;

		if ( eyePos.AlmostEqual( targetPos ) )
			return true;

		if ( eyePos.Direction( targetPos ).Angle( EyeForward ) > VisionAngle )
			return false;

		return eyePos.Distance( targetPos ) <= VisionDistance;
	}

	/// <summary>
	/// Traces to their center then their head and tells you if and where they could see.
	/// </summary>
	/// <param name="target"> The other guy. </param>
	/// <param name="hitPos"> The place we can look at. </param>
	/// <returns> If there was line of sight. </returns>
	public virtual bool HasLineOfSight( Pawn target, out Vector3? hitPos )
	{
		hitPos = null;

		if ( !target.IsValid() )
			return false;

		// We can see ourself.. probably.
		if ( target == Actor )
		{
			hitPos = EyePosition;
			return true;
		}

		var ourEyePos = EyePosition;
		var otherEyePos = target.EyePosition;
		var otherCenterPos = otherEyePos.LerpTo( target.WorldPosition, 0.5f );

		var visionTrace = GetEyeTrace();

		// Look at their center first.
		if ( !IsVisionTraceBlocked( visionTrace, ourEyePos, otherCenterPos ) )
		{
			hitPos = otherCenterPos;
			return true;
		}

		// Then try looking at their head.
		if ( !IsVisionTraceBlocked( visionTrace, ourEyePos, otherEyePos ) )
		{
			hitPos = otherEyePos;
			return true;
		}

		return false;
	}

	/// <summary>
	/// Typically called by <see cref="HasLineOfSight"/> to actually perform and filter the vision trace.
	/// </summary>
	public bool IsVisionTraceBlocked( in SceneTrace trace, in Vector3 from, in Vector3 to )
	{
		if ( DrawDebugVisionLines )
			DebugOverlay.Line( from, to, Color.Cyan.WithAlpha( 0.3f ), VisionFrequency, overlay: true );

		var trAll = trace
			.FromTo( from, to )
			.RunAll()
			.OrderBy( tr => tr.Distance );

		foreach ( var tr in trAll )
			if ( TraceBlocksVision( tr ) )
				return true;

		return false;
	}

	/// <summary>
	/// Determines if one of the hits within <see cref="IsVisionTraceBlocked"/> obscure detection. <br />
	/// This assumes you've already have vision filters, like from <see cref="Pawn.GetEyeTrace(Vector3)"/>.
	/// </summary>
	protected virtual bool TraceBlocksVision( in SceneTraceResult tr )
	{
		// Can see through pawns and projectiles by default.
		return tr.GameObject.IsValid()
			&& !(tr.GameObject.Tags?.HasAny( TAG_PAWN, TAG_PROJECTILE ) ?? true);
	}
}
