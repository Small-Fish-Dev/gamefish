namespace GameFish;

partial class SimpleActor
{
	/// <summary>
	/// If enabled: aim at targets.
	/// Otherwise.. they ain't aiming at shit, guy.
	/// </summary>
	[Property]
	[Feature( ACTOR )]
	[ToggleGroup( nameof( AllowAiming ), Label = AIMING )]
	public virtual bool AllowAiming { get; set; } = true;

	/// <summary>
	/// How quickly to aim towards the target.
	/// A higher smoothness value dampens this.
	/// </summary>
	[Property]
	[Title( "Speed" )]
	[Feature( ACTOR )]
	[Range( 0.1f, 20f, clamped: false )]
	[ToggleGroup( nameof( AllowAiming ) )]
	public virtual float AimingSpeed { get; set; } = 2f;

	/// <summary>
	/// The sluggishness of aiming towards the target.
	/// Aiming speed is divided by this.
	/// </summary>
	[Property]
	[Feature( ACTOR )]
	[Title( "Smoothness" )]
	[Range( 0.1f, 10f, clamped: false )]
	[ToggleGroup( nameof( AllowAiming ) )]
	public virtual float AimingSmoothness { get; set; } = 1f;

	protected Vector3 _lookSpeed;

	/// <summary>
	/// Perform our attacking logic if possible.
	/// </summary>
	protected virtual void UpdateAttacking( in float deltaTime )
	{
		if ( MindState is not MentalState.Fighting )
			return;

		if ( TargetVisible )
			TargetAimPosition = GetTargetAimPosition( Target );

		if ( TargetAimPosition is not Vector3 aimAt )
			return;

		if ( AllowAiming )
			LookAt( aimAt, in deltaTime );

		ActiveEquip?.TryPrimary( EyePosition.Distance( aimAt ) );
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
}
