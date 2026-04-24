namespace GameFish;

partial class Actor
{
	/// <summary>
	/// If enabled: aim at targets.
	/// Otherwise.. they ain't aiming at shit, guy.
	/// </summary>
	[Property]
	[Feature( ACTOR )]
	[ToggleGroup( nameof( IsAimingAllowed ), Label = AIMING )]
	public virtual bool IsAimingAllowed { get; set; } = true;

	/// <summary>
	/// How quickly to aim towards the target.
	/// A higher smoothness value dampens this.
	/// </summary>
	[Property]
	[Title( "Speed" )]
	[Feature( ACTOR )]
	[Range( 0.1f, 20f, clamped: false )]
	[ToggleGroup( nameof( IsAimingAllowed ) )]
	public virtual float AimingSpeed { get; set; } = 2f;

	/// <summary>
	/// The sluggishness of aiming towards the target.
	/// Aiming speed is divided by this.
	/// </summary>
	[Property]
	[Feature( ACTOR )]
	[Title( "Smoothness" )]
	[Range( 0.1f, 10f, clamped: false )]
	[ToggleGroup( nameof( IsAimingAllowed ) )]
	public virtual float AimingSmoothness { get; set; } = 1f;

	/// <summary>
	/// The point in world-space this is trying to aim towards.
	/// </summary>
	[Sync]
	public Vector3? AimPoint { get; set; }

	protected Vector3 _lookSpeed = Vector3.Zero;

	protected virtual void UpdateAiming( in float deltaTime )
	{
		if ( !IsAimingAllowed )
			return;

		if ( IsTargetVisible() )
			AimPoint = GetTargetAimPoint( Target );

		if ( AimPoint is Vector3 aimAt )
			LookAt( aimAt, in deltaTime );
		else if ( Velocity.Length > 20 )
			LookTowards( Rotation.LookAt( Velocity ), in deltaTime );
	}

	/// <summary>
	/// Perform our attacking logic if possible.
	/// </summary>
	protected virtual void UpdateAttacking( in float deltaTime )
	{
		if ( !ActiveEquip.IsValid() || !ActiveEquip.IsUsable( this, forCombat: true ) )
			return;

		ActiveEquip.TryPrimary( this );
	}

	/// <summary>
	/// Rotates this actor's aim towards a target position.
	/// </summary>
	/// <param name="targetPos"> The target position. </param>
	/// <param name="deltaTime"> The rate of rotation per second. </param>
	protected virtual void LookAt( in Vector3 targetPos, in float deltaTime )
	{
		var aimPos = EyePosition;
		var aimDir = aimPos.Direction( targetPos );

		LookTowards( Rotation.LookAt( aimDir ), in deltaTime );
	}

	/// <summary>
	/// Rotates this actor's aim towards a target rotation.
	/// </summary>
	/// <param name="rTarget"> The target rotation. </param>
	/// <param name="deltaTime"> The rate of rotation per second. </param>
	protected virtual void LookTowards( in Rotation rTarget, in float deltaTime )
	{
		EyeRotation = Rotation.SmoothDamp(
			current: EyeRotation,
			target: rTarget,
			velocity: ref _lookSpeed,
			smoothTime: AimingSmoothness,
			deltaTime: AimingSpeed * deltaTime
		);
	}

	/// <returns> Where we should aim at to hit this target(such as ahead of them). </returns>
	public virtual Vector3? GetTargetAimPoint( Pawn target = null, Vector3? at = null )
	{
		if ( !IsAimingAllowed )
			return null;

		target ??= Target;

		if ( !target.IsValid() )
			return null;

		// Default to the approximate center of the target.
		at ??= target.Center;

		// Allow equipment to affect our aim(such as shooting a projectile ahead).
		if ( ActiveEquip is var equip && equip.IsValid() )
			if ( equip.GetTargetAimPoint( target, at ) is Vector3 equipAim )
				return equipAim;

		return at;
	}

	/// <returns> The distance from the target(or null). </returns>
	public virtual float? GetDistanceFromTarget( Pawn target = null )
	{
		target ??= Target;

		if ( !target.IsValid() )
			return null;

		if ( AimPoint is Vector3 aimPos )
			return EyePosition.Distance( aimPos );

		if ( GetTargetOrigin( target ) is Vector3 targetPos )
			return EyePosition.Distance( targetPos );

		return null;
	}
}
