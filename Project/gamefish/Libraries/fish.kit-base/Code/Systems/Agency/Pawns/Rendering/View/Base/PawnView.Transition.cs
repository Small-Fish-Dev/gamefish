namespace GameFish;

partial class PawnView
{
	/// <summary>
	/// How quickly to transition between modes.
	/// </summary>
	[Property]
	[Title( "Speed" )]
	[Feature( MODES ), Group( TRANSITIONING )]
	public float TransitionSpeed { get; set; } = 3f;

	/// <summary>
	/// The smoothness of mode transition speed. Slows it down effectively.
	/// </summary>
	[Property]
	[Title( "Smoothing" )]
	[Feature( MODES ), Group( TRANSITIONING )]
	public float TransitionSmoothing { get; set; } = 0.35f;

	/// <summary>
	/// The local position and rotation relative to the pawn's origin.
	/// </summary>
	[Title( "Relative Offset" )]
	[Property, ReadOnly, InlineEditor]
	[Feature( MODES ), Group( TRANSITIONING )]
	public Offset Relative
	{
		get => _relative;
		set
		{
			_relative = value;
			// UpdateTransform();
		}
	}

	protected Offset _relative;

	/// <summary>
	/// The previous world position and rotation to transition from.
	/// </summary>
	public Transform? PreviousTransform { get; set; }

	/// <summary>
	/// The previous relative position and rotation to transition from.
	/// </summary>
	public Offset? PreviousOffset { get; set; }

	public virtual float TransitionFraction
	{
		get => _transFrac;
		set => _transFrac = value.Clamp( 0f, 1f );
	}
	protected float _transFrac;

	public virtual float TransitionVelocity { get => _transVel; set => _transVel = value; }
	protected float _transVel;

	/// <summary>
	/// Begins a transition given the current position of this view.
	/// </summary>
	public virtual void StartTransition( bool isRelative = true )
	{
		var pawn = Pawn;

		if ( !pawn.IsValid() )
			return;

		if ( isRelative )
		{
			PreviousTransform = null;
			PreviousOffset = new( pawn.EyeTransform.ToLocal( WorldTransform ) );
		}
		else
		{
			PreviousOffset = null;
			PreviousTransform = WorldTransform;
		}

		TransitionFraction = 0f;
		_transVel = 0f;
	}

	/// <summary>
	/// Instantly ends the transition. Might cause visible snapping if done midway!
	/// </summary>
	public virtual void StopTransition()
	{
		PreviousTransform = null;
		PreviousOffset = null;

		TransitionFraction = 1f;
	}

	protected virtual void UpdateTransition()
	{
		if ( !PreviousTransform.HasValue && !PreviousOffset.HasValue )
			return;

		TransitionFraction = MathX.SmoothDamp( TransitionFraction, 1f, ref _transVel, TransitionSmoothing, Time.Delta )
			.Clamp( 0f, 1f );

		if ( TransitionFraction.AlmostEqual( 1f ) )
		{
			PreviousTransform = null;
			PreviousOffset = null;
		}
	}
}
