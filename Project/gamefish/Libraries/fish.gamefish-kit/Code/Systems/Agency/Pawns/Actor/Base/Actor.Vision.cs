using System.Text.Json.Serialization;

namespace GameFish;

partial class Actor
{
	protected const int VISION_ORDER = ACTOR_ORDER + 20;

	/// <summary>
	/// If enabled: allow sensing targets by vision.
	/// </summary>
	[Property]
	[Feature( ACTOR )]
	[ToggleGroup( nameof( IsVisionEnabled ), Label = VISION )]
	public virtual bool IsVisionEnabled { get; set; } = true;

	/// <summary>
	/// If enabled: log vision events in console.
	/// </summary>
	[Property]
	[Title( "Debug (logging)" )]
	[Feature( ACTOR ), Group( VISION )]
	[ToggleGroup( nameof( IsVisionEnabled ) )]
	public virtual bool DebugVisionLogging { get; set; } = false;

	/// <summary>
	/// If enabled: display vision debug helpers.
	/// </summary>
	[Property]
	[Title( "Debug (rendering)" )]
	[Feature( ACTOR ), Group( VISION )]
	[ToggleGroup( nameof( IsVisionEnabled ) )]
	public virtual bool DebugVisionRendering { get; set; } = false;

	/// <summary>
	/// The default angle of the vision cone.
	/// </summary>
	[Property]
	[Title( "Angle (default)" )]
	[Feature( ACTOR ), Group( VISION )]
	[ToggleGroup( nameof( IsVisionEnabled ) )]
	protected virtual float BaseVisionAngle { get; set; } = 45f;

	[Title( "Angle (current)" )]
	[Property, JsonIgnore, ReadOnly]
	[ShowIf( nameof( InGame ), true )]
	[Feature( ACTOR ), Group( VISION )]
	[ToggleGroup( nameof( IsVisionEnabled ) )]
	protected float InspectorVisionAngle => VisionAngle;

	/// <summary>
	/// The default distance of the vision cone.
	/// </summary>
	[Property]
	[Title( "Distance (default)" )]
	[Feature( ACTOR ), Group( VISION )]
	[ToggleGroup( nameof( IsVisionEnabled ) )]
	protected virtual float BaseVisionDistance { get; set; } = 2048f;

	[Title( "Distance (current)" )]
	[Property, JsonIgnore, ReadOnly]
	[ShowIf( nameof( InGame ), true )]
	[Feature( ACTOR ), Group( VISION )]
	[ToggleGroup( nameof( IsVisionEnabled ) )]
	protected float InspectorVisionDistance => VisionDistance;

	/// <summary>
	/// The default delay in real time between looking for target(s).
	/// </summary>
	[Property]
	[Title( "Frequency (default)" )]
	[Feature( ACTOR ), Group( VISION )]
	[ToggleGroup( nameof( IsVisionEnabled ) )]
	protected virtual float BaseVisionFrequency { get; set; } = 0.1f;

	[Title( "Frequency (current)" )]
	[Property, JsonIgnore, ReadOnly]
	[ShowIf( nameof( InGame ), true )]
	[Feature( ACTOR ), Group( VISION )]
	[ToggleGroup( nameof( IsVisionEnabled ) )]
	protected float InspectorVisionFrequency => VisionFrequency;

	public virtual float VisionAngle => BaseVisionAngle;
	public virtual float VisionDistance => BaseVisionDistance;
	public virtual float VisionFrequency => BaseVisionFrequency;

	/// <summary>
	/// The targetable pawns we are actively looking at.
	/// Meant to be updated per tick/frame.
	/// </summary>
	[Sync]
	public NetList<Pawn> VisibleTargets { get; set; } = [];

	/// <summary> When we last looked for a target(if ever). </summary>
	public RealTimeSince? SinceLastVisionCheck { get; protected set; }

	/// <summary>
	/// A quick check to see if a target is known to be in our sight.
	/// <br /> <br />
	/// <b> NOTE: </b> Keep this lite or it may lag! <br />
	/// It's meant to be used very often by behavioral logic to know if a target is visible.
	/// Use this to retrieve, <b>NOT</b> perform line of sight calculations.
	/// </summary>
	/// <param name="target"> A specific target(or <see cref="Target"/> by default). </param>
	/// <returns> If the target is currently in sight. </returns>
	public virtual bool IsTargetVisible( Pawn target = null )
	{
		if ( !IsVisionEnabled )
			return false;

		if ( VisibleTargets is null )
			return false;

		target ??= Target;

		if ( !target.IsValid() )
			return false;

		return VisibleTargets.Contains( target );
	}

	/// <summary>
	/// Handles targets entering/exiting our sight.
	/// </summary>
	/// <param name="target"></param>
	/// <param name="isVisible"></param>
	public virtual void SetTargetVisibility( Pawn target, in bool isVisible )
	{
		if ( IsProxy )
			return;

		if ( !target.IsValid() )
			return;

		VisibleTargets ??= [];

		if ( VisibleTargets is null )
			return;

		if ( isVisible )
		{
			if ( VisibleTargets.Contains( target ) )
				return;

			VisibleTargets.Add( target );
			OnTargetVisibilityGained( target );
		}
		else
		{
			if ( !VisibleTargets.Contains( target ) )
				return;

			VisibleTargets.Remove( target );
			OnTargetVisibilityLost( target );
		}
	}

	/// <summary>
	/// Tracks which targets can still be seen.
	/// </summary>
	protected virtual void UpdateTargetVisibility()
	{
		if ( IsProxy )
			return;

		UpdateTargetVisibility( Target );

		if ( VisibleTargets is null )
			return;

		// Stop tracking targets that aren't valid any more.
		var bad = VisibleTargets.Where( e => !IsTargetValid( e ) );

		if ( bad.Any() )
		{
			foreach ( var target in bad.ToArray() )
				VisibleTargets.Remove( target );
		}

		// Lose sight of hidden targets.
		foreach ( var visibleTarget in VisibleTargets )
		{
			// Don't double-check the primary target.
			if ( visibleTarget == Target )
				continue;

			UpdateTargetVisibility( visibleTarget );
		}
	}

	protected virtual void UpdateTargetVisibility( Pawn target )
	{
		if ( !IsTargetValid( target ) || !IsVisible( target, out var visiblePos ) )
		{
			SetTargetVisibility( target, false );
			return;
		}

		SetTargetVisibility( target, true );
		OnTargetDetected( target, visiblePos ?? target.Center );
	}

	public virtual void UpdateVision( in float deltaTime )
	{
		UpdateTargetVisibility();

		// Slight delay between expensive checks for optimization.
		if ( SinceLastVisionCheck is RealTimeSince lastCheck )
			if ( lastCheck < VisionFrequency )
				return;

		SinceLastVisionCheck = 0f;

		if ( DebugVisionRendering )
			DebugOverlay.Line( EyePosition, EyePosition + EyeRotation.Forward * VisionDistance, duration: VisionFrequency );

		// Trace in a sphere to find enemies.
		var eyePos = EyePosition;

		var enemyWithDist = GetEyeTrace()
			.Sphere( VisionDistance, eyePos, eyePos ).RunAll()
			.Select( tr => TryGet<Pawn>( tr.GameObject, out var pawn ) ? pawn : null )
			.Where( pawn => IsTargetValid( pawn ) && IsEnemy( pawn ) )
			.Select( pawn => IsVisible( pawn, out var visiblePos ) ? (pawn, visiblePos) : (null, null) )
			.Where( seen => seen.pawn.IsValid() && seen.visiblePos.HasValue )
			.OrderBy( seen => eyePos.Distance( seen.visiblePos ?? seen.pawn.Center ) )
			.FirstOrDefault();

		if ( enemyWithDist.pawn is Pawn pawn && pawn.IsValid() )
			OnTargetDetected( pawn, enemyWithDist.visiblePos ?? pawn.Center );
	}

	/// <summary>
	/// (Re)gained sight of an active target.
	/// </summary>
	protected virtual void OnTargetVisibilityGained( Pawn target )
	{
		if ( DebugVisionLogging )
			this.Log( $"Gained visibility of target:[{target}]." );
	}

	/// <summary>
	/// Lost sight of an active target.
	/// </summary>
	protected virtual void OnTargetVisibilityLost( Pawn target )
	{
		if ( DebugVisionLogging )
			this.Log( $"Lost visibility of target:[{target}]." );
	}

	public virtual bool IsVisible( Pawn pawn, out Vector3? aimPos )
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
			if ( IsVisionTraceBlocked( tr ) )
			{
				if ( DebugVisionRendering )
					DebugOverlay.Trace( tr, VisionFrequency, overlay: true );

				return true;
			}
		}

		return false;
	}

	/// <summary>
	/// Determines if the performed trace is one that should block vision. <br />
	/// This assumes you've already have vision filters, like from <see cref="Pawn.GetEyeTrace(Vector3)"/>.
	/// </summary>
	protected virtual bool IsVisionTraceBlocked( in SceneTraceResult tr )
	{
		// Can see through pawns and projectiles by default.
		return tr.GameObject.IsValid()
			&& tr.GameObject.Tags?.HasAny( TAG_PAWN, TAG_PROJECTILE ) is not true;
	}
}
