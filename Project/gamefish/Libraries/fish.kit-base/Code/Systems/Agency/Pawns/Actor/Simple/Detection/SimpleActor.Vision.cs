namespace GameFish;

partial class SimpleActor
{
	/// <summary>
	/// Enables drawing of vision debug gizmos.
	/// </summary>
	[Property]
	[Title( "Debug Gizmos" )]
	[Feature( ACTOR ), Group( VISION )]
	public virtual bool DrawVisionGizmos { get; set; }

	/// <summary>
	/// The total angle that this NPC can see things within.
	/// </summary>
	[Property]
	[Title( "Angle" )]
	[Range( 0f, 360f, clamped: false )]
	[Feature( ACTOR ), Group( VISION )]
	protected virtual float DefaultVisionAngle { get; set; } = 130f;

	/// <summary>
	/// How far targets can be detected.
	/// </summary>
	[Property]
	[Title( "Distance" )]
	[Range( 0f, 5000f, clamped: false )]
	[Feature( ACTOR ), Group( VISION )]
	protected virtual float DefaultVisionDistance { get; set; } = 3000f;

	/// <summary>
	/// How often to look for targets.
	/// </summary>
	[Property]
	[Title( "Frequency" )]
	[Range( 0f, 2f, clamped: false )]
	[Feature( ACTOR ), Group( VISION )]
	protected virtual float DefaultVisionFrequency { get; set; } = 0.25f;

	public virtual float VisionAngle => DefaultVisionAngle;
	public virtual float VisionDistance => DefaultVisionDistance;
	public virtual float VisionFrequency => DefaultVisionFrequency;

	/// <summary> When we last looked for a target. </summary>
	public TimeSince? LastVisionCheck { get; set; }

	/// <summary>
	/// Look out for things of interest.
	/// </summary>
	public override void UpdatePerception( in float deltaTime )
	{
		/*
		// Debug vision cone.
		var target = Player.Local;

		if ( target.IsValid() )
		{
			using ( Gizmo.Scope( Id + "cone", global::Transform.Zero ) )
			{
				var eyePos = EyePosition;
				var rAim = EyeRotation;
				var aimDir = rAim.Forward;
				var dist = VisionDistance;

				var coneEnd = eyePos + aimDir * dist;
				var spreadDir = (rAim * Rotation.FromYaw( VisionAngle )).Forward;

				float angle = coneEnd.Distance( eyePos + (spreadDir * dist) );

				Gizmo.Draw.Color = Color.White.WithAlpha( 0.5f );
				Gizmo.Draw.SolidCone( eyePos + (aimDir * dist), -aimDir * dist, angle, 32 );
			}
		}

		// this.DrawArrow( EyePosition, EyePosition + EyeForward * 64f, Color.Cyan, tWorld: global::Transform.Zero );
		*/

		UpdateTargetVisibility();

		if ( LastVisionCheck.HasValue && LastVisionCheck < VisionFrequency )
			return;

		LastVisionCheck = 0f;

		// if ( DrawVisionGizmos )
		// DebugOverlay.Line( EyePosition, EyePosition + EyeRotation.Forward * VisionDistance, duration: VisionFrequency );

		var target = Target;

		if ( target.IsValid() && IsPawnVisible( target, out var aimPos ) )
		{
			OnPawnVisible( target, aimPos ?? Target.Center );
			return;
		}

		// Trace in a sphere to find enemies.
		var eyePos = EyePosition;

		var enemyWithDist = GetEyeTrace()
			.Sphere( VisionDistance, eyePos, eyePos ).RunAll()
			.Select( tr => TryGet<Pawn>( tr.GameObject, out var pawn ) ? pawn : null )
			.Where( pawn => pawn.IsValid() && IsEnemy( pawn ) )
			.Select( pawn => IsPawnVisible( pawn, out var visiblePos ) ? (pawn, visiblePos) : (null, null) )
			.Where( seen => seen.pawn.IsValid() && seen.visiblePos.HasValue )
			.OrderBy( seen => eyePos.Distance( seen.visiblePos ?? seen.pawn.Center ) )
			.FirstOrDefault();

		if ( enemyWithDist.pawn is Pawn pawn && pawn.IsValid() )
			OnPawnVisible( pawn, enemyWithDist.visiblePos ?? pawn.Center );
	}

	/// <summary>
	/// Called every tick to determine if/when our target was visible.
	/// </summary>
	protected virtual void UpdateTargetVisibility()
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
	protected virtual void OnPawnVisible( Pawn pawn, in Vector3 visiblePos )
	{
		if ( !pawn.IsValid() )
			return;

		if ( IsEnemy( pawn ) )
			OnEnemyVisible( pawn, visiblePos );
	}

	/// <summary>
	/// Actively looking at someone we just don't like.
	/// </summary>
	protected virtual void OnEnemyVisible( Pawn enemy, in Vector3 at )
	{
		// Double check that new targets are enemies.
		if ( Target != enemy && !TrySetTarget( enemy ) )
			return;

		TargetVisible = true;

		LastSeenTarget = 0f;

		TargetAimPosition = GetTargetAimPosition( enemy, at ) ?? at;
		LastKnownTargetPosition = GetTargetOrigin( enemy );
	}

	/// <summary>
	/// We lost sight of our current target.
	/// </summary>
	protected virtual void OnTargetVisibilityLost()
	{
		if ( !TargetVisible )
			return;

		// if ( Target.IsValid() )
		// this.Log( $"lost sight of: {Target}" );

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
		// this.Log( $"forgot about {Target}" );

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
				this.Log( $"got a whiff of {pawn}!" );

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
					this.Log( $"heard {pawn}!" );

					lookingAt = pawn.Center;
					return true;
				}
			}

			// this.Log( "vision not within cone. angle: " + EyePosition.Direction( targetPos ).Angle( EyeForward ) );
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

		if ( eyePos.Direction( targetPos ).Angle( EyeForward ) > VisionAngle * 0.5f )
			return false;

		return eyePos.Distance( targetPos ) <= VisionDistance;
	}

	/// <param name="pawn"> The other guy. </param>
	/// <param name="hitPos"> The place we can look at. </param>
	/// <returns> If there was line of sight. </returns>
	public virtual bool HasLineOfSight( Pawn pawn, out Vector3? hitPos )
	{
		hitPos = null;

		if ( !pawn.IsValid() )
			return false;

		// We can see ourself.. probably.
		if ( pawn == this )
		{
			hitPos = EyePosition;
			return true;
		}

		var ourEyePos = EyePosition;
		var otherEyePos = pawn.EyePosition;
		var otherCenterPos = otherEyePos.LerpTo( pawn.WorldPosition, 0.5f );

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
		var trAll = trace
			.FromTo( from, to )
			.RunAll()
			.OrderBy( tr => tr.Distance );

		foreach ( var tr in trAll )
		{
			if ( TraceBlocksVision( tr ) )
			{
				if ( DrawVisionGizmos )
					DebugOverlay.Trace( tr, VisionFrequency, overlay: true );

				return true;
			}
		}

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
			&& tr.GameObject.Tags?.HasAny( TAG_PAWN, TAG_PROJECTILE ) is not true;
	}
}
