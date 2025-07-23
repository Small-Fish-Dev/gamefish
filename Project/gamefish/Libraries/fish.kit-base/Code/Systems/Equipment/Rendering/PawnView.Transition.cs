namespace GameFish;

partial class PawnView
{
	/// <summary>
	/// How quickly to transition between modes.
	/// </summary>
	[Property]
	[Feature( VIEW ), Group( TRANSITIONING )]
	public float TransitionSpeed { get; set; } = 2f;

	/// <summary>
	/// The smoothness of mode transition speed. Slows it down effectively.
	/// </summary>
	[Property]
	[Feature( VIEW ), Group( TRANSITIONING )]
	public float TransitionSmoothing { get; set; } = 0.5f;

	/// <summary>
	/// The local position and rotation relative to the pawn's origin.
	/// </summary>
	[Title( "Relative Offset" )]
	[Property, ReadOnly, InlineEditor]
	[Feature( VIEW ), Group( TRANSITIONING )]
	public Offset Relative { get; set; }

	/// <summary>
	/// The previous world position and rotation to transition from.
	/// </summary>
	public Offset? Previous { get; set; }

	public float TransitionFraction { get; set; }
	public float TransitionVelocity { get => _transVel; set => _transVel = value; }
	protected float _transVel;

	protected virtual void StartTransition()
	{
		var pawn = Pawn;

		if ( !pawn.IsValid() )
			return;

		Previous = new( pawn.EyeTransform.ToLocal( WorldTransform ) );

		TransitionFraction = 0f;
		_transVel = 0f;
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
