namespace GameFish;

/// <summary>
/// The core automation and processing center responsible for perception and behavior.
/// Has mental states(such as for combat) that drive behaviors.
/// You probably need this unless you're doing that yourself.
/// </summary>
[Icon( "psychology" )]
public partial class ActorMind : ActorFeature
{
	protected const int MIND_ORDER = DEFAULT_ORDER - 120;

	/// <summary>
	/// The behavior to initialize with and/or fall back to.
	/// </summary>
	[Property]
	[Feature( MIND )]
	public virtual ActorBehavior DefaultBehavior
	{
		get
		{
			// Auto-cache if invalid.
			return !_defaultBehavior.IsValid() && !IsProxy
				? _defaultBehavior = GetModules<ActorBehavior>().FirstOrDefault( b => b?.IsDefault is true )
				: _defaultBehavior;
		}
		set => _defaultBehavior = value;
	}

	protected ActorBehavior _defaultBehavior;


	/// <summary>
	/// The mental state to initialize with and/or fall back to.
	/// </summary>
	[Property]
	[Feature( MIND )]
	public MentalState DefaultState { get; set; }

	/// <summary> Are we chilling or killing? </summary>
	[Sync]
	public MentalState State
	{
		get => _state.IsValid() ? _state : null;
		protected set
		{
			if ( _state == value )
				return;

			_state = value;

			if ( !IsProxy )
				OnSetState( value );
		}
	}

	protected MentalState _state;


	public override void Simulate( in float deltaTime, in bool isFixedUpdate )
		=> Think( in deltaTime, isFixedUpdate );

	public virtual void Think( in float deltaTime, in bool isFixedUpdate )
	{
		if ( IsProxy )
			return;

		State?.Think( in deltaTime, in isFixedUpdate );

		UpdateAiming( deltaTime );
	}


	protected virtual void OnSetState( MentalState state )
	{
		// state?.OnSelect();
	}

	public virtual bool TrySetDefaultState()
	{
		if ( !DefaultState.IsValid() )
		{
			DefaultState = GetModules<MentalState>()?
				.FirstOrDefault( m => m?.IsDefault is true );
		}

		return TrySetState( DefaultState );
	}

	public virtual bool TrySetState( MentalState state )
	{
		if ( !state.IsValid() || IsProxy )
			return false;

		if ( State == state )
			return true;

		// idgaf
		return (State = state) == state;
	}

	public virtual bool TryEndState()
	{
		if ( State.IsValid() )
			State.TryEnd();

		return TrySetDefaultState();
	}

	/// <summary>
	/// Update where we should be and actively are aiming at.
	/// </summary>
	protected virtual void UpdateAiming( in float deltaTime )
	{
		if ( IsTargetVisible() )
			TargetAimPosition = GetTargetAimPosition( Target );

		if ( TargetAimPosition is not Vector3 targetAimPos )
		{
			var vel = Controller?.Velocity ?? Velocity;

			if ( vel.Length > 20f )
			{
				var dir = Rotation.LookAt( vel.Normal, WorldRotation.Up );
				EyeRotation = EyeRotation.SlerpTo( dir, deltaTime * 10f );
			}

			return;
		}

		// Looking at a target.
		var aimDir = EyePosition.Direction( targetAimPos );
		EyeRotation = Rotation.LookAt( aimDir, WorldRotation.Up );
	}
}
