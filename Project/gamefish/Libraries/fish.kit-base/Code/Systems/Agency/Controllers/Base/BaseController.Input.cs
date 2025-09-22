namespace GameFish;

partial class BaseController
{
	public const string MOVEMENT = "Movement";
	public const int MOVEMENT_ORDER = 200;

	public const string DUCKING = "Ducking";
	public const int DUCKING_ORDER = 220;

	public const string JUMPING = "Jumping";
	public const int JUMPING_ORDER = 240;

	/// <summary>
	/// The default walking/flying speed.
	/// </summary>
	[Property]
	[Range( 0, 1000, clamped: false )]
	[Feature( INPUT ), Group( MOVEMENT ), Order( MOVEMENT_ORDER )]
	public virtual float MoveSpeed { get; set; } = 100f;

	/// <summary>
	/// The button to run. <br />
	/// Set this to blank/null to disable it.
	/// </summary>
	[Property]
	[InputAction]
	[Feature( INPUT ), Group( MOVEMENT ), Order( MOVEMENT_ORDER )]
	public string SprintButton { get; set; } = "run";
	public bool HasSprintButton => !string.IsNullOrWhiteSpace( SprintButton );

	[Property]
	[Range( 0, 3, clamped: false )]
	[ShowIf( nameof( HasSprintButton ), true )]
	[Feature( INPUT ), Group( MOVEMENT ), Order( MOVEMENT_ORDER )]
	public virtual float SprintMultiplier { get; set; } = 1.5f;

	/// <summary>
	/// The button to crouch. <br />
	/// Set this to blank/null to disable it.
	/// </summary>
	[Property]
	[InputAction]
	[Feature( INPUT ), Group( DUCKING ), Order( DUCKING_ORDER )]
	public string DuckButton { get; set; } = "duck";
	public bool HasDuckButton => !string.IsNullOrWhiteSpace( DuckButton );

	[Property]
	[Range( 0, 1000, clamped: false )]
	[ShowIf( nameof( HasDuckButton ), true )]
	[Feature( INPUT ), Group( DUCKING ), Order( DUCKING_ORDER )]
	public virtual float DuckMoveSpeed { get; set; } = 50f;

	[Property]
	[Range( 0, 100, clamped: false )]
	[ShowIf( nameof( HasDuckButton ), true )]
	[Feature( INPUT ), Group( DUCKING ), Order( DUCKING_ORDER )]
	public virtual float DuckLowerSpeed { get; set; } = 10f;

	[Property]
	[Range( 0, 100, clamped: false )]
	[ShowIf( nameof( HasDuckButton ), true )]
	[Feature( INPUT ), Group( DUCKING ), Order( DUCKING_ORDER )]
	public virtual float DuckRaiseSpeed { get; set; } = 15f;

	/// <summary>
	/// The button to let you jump. <br />
	/// Set this to blank/null to disable it.
	/// </summary>
	[Property]
	[InputAction]
	[Feature( INPUT ), Group( JUMPING ), Order( JUMPING_ORDER )]
	public string JumpButton { get; set; } = "jump";
	public bool HasJumpButton => !string.IsNullOrWhiteSpace( JumpButton );

	[Property]
	[Feature( INPUT ), Group( JUMPING ), Order( JUMPING_ORDER )]
	[Range( 0, 1000, clamped: false )]
	[ShowIf( nameof( HasJumpButton ), true )]
	public virtual float JumpSpeed { get; set; } = 250f;

	[Property]
	[Title( "Standing" )]
	[Feature( VIEW ), Group( "Eye Height" )]
	public virtual float EyeHeightStand { get; set; } = 64f;

	[Property]
	[Title( "Ducked" )]
	[Feature( VIEW ), Group( "Eye Height" )]
	public virtual float EyeHeightDuck { get; set; } = 32f;

	public virtual bool Standing => true;

	public virtual float EyeHeightTarget
		=> Standing ? EyeHeightStand : EyeHeightDuck;

	[Sync( SyncFlags.Interpolate )]
	public Vector3 LocalEyePosition
	{
		get => GetLocalEyePosition();
		set => SetLocalEyePosition( value );
	}

	protected Vector3 _localEyePos;

	[Sync( SyncFlags.Interpolate )]
	public Rotation LocalEyeRotation
	{
		get => GetLocalEyeRotation();
		set => SetLocalEyeRotation( value );
	}

	protected Rotation _localEyeRotation;

	public virtual Vector3 GetLocalEyePosition()
		=> _localEyePos;

	public virtual void SetLocalEyePosition( Vector3 value )
	{
		if ( ITransform.IsValid( in value ) )
			_localEyePos = value;
	}

	public virtual Rotation GetLocalEyeRotation()
		=> _localEyeRotation;

	public virtual void SetLocalEyeRotation( Rotation value )
	{
		if ( ITransform.IsValid( in value ) )
			_localEyeRotation = value;
	}
}
