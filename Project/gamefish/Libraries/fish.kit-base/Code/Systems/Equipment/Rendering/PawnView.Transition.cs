namespace GameFish;

partial class PawnView
{
	/// <summary>
	/// How quickly to transition between modes.
	/// </summary>
	[Property]
	[Title( "Speed" )]
	[Feature( VIEW ), Group( TRANSITIONING )]
	public float TransitionSpeed { get; set; } = 3f;

	/// <summary>
	/// The smoothness of mode transition speed. Slows it down effectively.
	/// </summary>
	[Property]
	[Title( "Smoothing" )]
	[Feature( VIEW ), Group( TRANSITIONING )]
	public float TransitionSmoothing { get; set; } = 0.35f;

	/// <summary>
	/// The local position and rotation relative to the pawn's origin.
	/// </summary>
	[Title( "Relative Offset" )]
	[Property, ReadOnly, InlineEditor]
	[Feature( VIEW ), Group( TRANSITIONING )]
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
	public Offset? Previous { get; set; }

	public float TransitionFraction { get; set; }
	public float TransitionVelocity { get => _transVel; set => _transVel = value; }
	protected float _transVel;

	/// <summary>
	/// Begins a transition given the current position of this view.
	/// </summary>
	protected virtual void StartTransition()
	{
		var pawn = Pawn;

		if ( !pawn.IsValid() )
			return;

		Previous = new( pawn.EyeTransform.ToLocal( WorldTransform ) );

		TransitionFraction = 0f;
		_transVel = 0f;
	}

	/// <summary>
	/// Instantly ends the transition. Might cause visible snapping if done midway!
	/// </summary>
	protected virtual void StopTransition()
	{
		Previous = null;
		TransitionFraction = 1f;
	}

	protected virtual void UpdateTransition()
	{
		if ( !Previous.HasValue )
			return;

		TransitionFraction = MathX.SmoothDamp( TransitionFraction, 1f, ref _transVel, TransitionSmoothing, Time.Delta )
			.Clamp( 0f, 1f );

		if ( TransitionFraction.AlmostEqual( 1f ) )
			Previous = null;
	}
}
