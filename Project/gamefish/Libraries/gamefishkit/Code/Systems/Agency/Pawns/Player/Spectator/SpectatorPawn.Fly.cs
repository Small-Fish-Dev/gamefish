namespace GameFish;

partial class SpectatorPawn
{
	[Property]
	[InputAction]
	[Title( "Run" )]
	[Feature( SPECTATOR ), Group( INPUT )]
	public virtual string RunAction { get; set; } = "Run";
	public virtual bool AllowRunning => !string.IsNullOrWhiteSpace( RunAction );

	[Property]
	[InputAction]
	[Title( "Ascend" )]
	[Feature( SPECTATOR ), Group( INPUT )]
	public virtual string AscendAction { get; set; } = "Jump";
	public virtual bool AllowAscend => !string.IsNullOrWhiteSpace( AscendAction );

	[Property]
	[InputAction]
	[Title( "Descend" )]
	[Feature( SPECTATOR ), Group( INPUT )]
	public virtual string DescendAction { get; set; } = "Duck";
	public virtual bool AllowDescend => !string.IsNullOrWhiteSpace( DescendAction );

	/// <summary>
	/// Allows flying around while not spectating someone.
	/// </summary>
	[Property]
	[Title( "Enabled" )]
	[Feature( SPECTATOR ), ToggleGroup( nameof( FlyingEnabled ), Label = FLYING )]
	public virtual bool FlyingEnabled { get; set; } = true;

	/// <summary>
	/// The speed to fly while not spectating someone.
	/// </summary>
	[Property]
	[Title( "Speed" )]
	[Range( 0f, 5000f, clamped: false )]
	[Feature( SPECTATOR ), ToggleGroup( nameof( FlyingEnabled ) )]
	public virtual float FlyingSpeed { get; set; } = 1000f;

	/// <summary>
	/// Fly this fast while holding the run key(if set).
	/// </summary>
	[Property]
	[Title( "Run Speed" )]
	[Range( 0f, 5000f, clamped: false )]
	[Feature( SPECTATOR ), ToggleGroup( nameof( FlyingEnabled ) )]
	public virtual float FlyingRunSpeed { get; set; } = 2000f;

	/// <summary>
	/// Affects how long it takes for your momentum to stop.
	/// </summary>
	[Property]
	[Title( "Friction" )]
	[Feature( SPECTATOR ), ToggleGroup( nameof( FlyingEnabled ) )]
	public virtual Friction FlyingFriction { get; set; }

	protected virtual void DoFlying( in float deltaTime )
	{
		if ( Spectating.IsValid() || !FlyingEnabled )
			return;

		var speed = AllowRunning && Input.Down( RunAction )
			? FlyingRunSpeed
			: FlyingSpeed;

		var wishVel = Input.AnalogMove * speed;

		if ( AllowAscend && Input.Down( AscendAction ) )
			wishVel += Vector3.Up * speed;

		if ( AllowDescend && Input.Down( DescendAction ) )
			wishVel += Vector3.Down * speed;

		var move = EyeRotation * (wishVel * deltaTime);

		Velocity += move;

		var dest = EyePosition + (Velocity * deltaTime);

		EyePosition = dest;
		WorldPosition = dest;

		// Apply friction.
		Velocity = Velocity.WithFriction( FlyingFriction, deltaTime );
	}
}
