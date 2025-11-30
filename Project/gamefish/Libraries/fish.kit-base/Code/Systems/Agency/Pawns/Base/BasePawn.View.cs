namespace GameFish;

partial class BasePawn
{
	/// <summary>
	/// The central view manager for the pawn.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( VIEW )]
	public virtual PawnView View
	{
		get => _view.IsValid() ? _view
			: _view = Components?.Get<PawnView>( FindMode.EverythingInSelfAndDescendants );

		set { _view = value; }
	}

	protected PawnView _view;

	[Property]
	[Feature( PAWN ), Group( VIEW )]
	public virtual ViewRenderer ViewRenderer => View?.ViewRenderer;

	/// <summary> The base vision trace will ignore objects with these tags. </summary>
	[Property]
	[Feature( PAWN ), Group( VIEW )]
	public virtual TagSet EyeTraceIgnore { get; set; } = ["water"];

	/// <summary>
	/// The world-space eye position.
	/// </summary>
	public virtual Vector3 EyePosition
	{
		get
		{
			if ( Controller.IsValid() )
				return WorldTransform.PointToWorld( Controller.GetLocalEyePosition() );

			return WorldPosition;
		}
		set
		{
			if ( Controller.IsValid() )
				Controller.SetLocalEyePosition( WorldTransform.PointToLocal( value ) );
		}
	}

	/// <summary>
	/// The world-space eye rotation.
	/// </summary>
	public virtual Rotation EyeRotation
	{
		get
		{
			if ( Controller.IsValid() )
				return WorldTransform.RotationToWorld( Controller.GetLocalEyeRotation() );

			return WorldRotation;
		}
		set
		{
			if ( Controller.IsValid() )
				Controller.SetLocalEyeRotation( WorldTransform.RotationToLocal( value ) );
		}
	}

	public Transform EyeTransform => new( EyePosition, EyeRotation, WorldScale );
	public Vector3 EyeForward => EyeRotation.Forward;

	/// <summary>
	/// Tells the view manager to process its transitions and offsets.
	/// </summary>
	public virtual void UpdateView( in float deltaTime )
		=> View?.FrameSimulate( deltaTime );

	/// <summary>
	/// Lets this pawn affect/override a view transform(probably from the main camera).
	/// </summary>
	public virtual bool TryApplyView( CameraComponent cam, ref Transform tView )
	{
		// Allow view transform transitioning and effects.
		if ( View is PawnView view && view.IsValid() )
		{
			tView = view.GetViewTransform();
			return true;
		}

		// Default to the pawn's viewing origin.
		tView = EyeTransform;
		return true;
	}

	/// <summary>
	/// Called when the view targeting this pawn has finished updating. <br />
	/// It may be a view spectating this pawn, thus not the child view. <br />
	/// You should process clientside effects like model fading here.
	/// </summary>
	public virtual void OnViewUpdate( PawnView view )
	{
		if ( !view.IsValid() )
			return;

		if ( BodyComponent is PawnBody body && body.IsValid() )
			body.OnViewUpdate( view );
	}

	/// <returns> Prepares a default trace with vision filters. </returns>
	public virtual SceneTrace GetEyeTrace()
		=> Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( EyeTraceIgnore );

	/// <returns> Prepares a vision trace from the eye position to a point. </returns>
	public SceneTrace GetEyeTrace( Vector3 to )
		=> GetEyeTrace().FromTo( EyePosition, to );

	/// <returns> Prepares a vision trace from one point to another. </returns>
	public SceneTrace GetEyeTrace( Vector3 from, Vector3 to )
		=> GetEyeTrace().FromTo( from, to );

	/// <returns> Prepares a vision trace from the eye position. </returns>
	public SceneTrace GetEyeTrace( float distance, Vector3? dir = null )
	{
		var startPos = EyePosition;
		var endPos = startPos + ((dir ?? EyeForward) * distance);

		var tr = GetEyeTrace()
			.FromTo( startPos, endPos );

		return tr;
	}
}
