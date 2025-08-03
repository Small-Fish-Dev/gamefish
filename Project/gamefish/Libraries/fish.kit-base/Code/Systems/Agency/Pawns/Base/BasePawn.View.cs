namespace GameFish;

partial class BasePawn
{
	/// <summary>
	/// The central view manager for the pawn.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( PawnView.VIEW )]
	public PawnView View
	{
		get => _view.IsValid() ? _view
			: _view = GetModule<PawnView>();

		set { _view = value; }
	}

	protected PawnView _view;

	[Property]
	[Feature( PAWN ), Group( PawnView.VIEW )]
	public ViewRenderer ViewRenderer => View?.ViewRenderer;

	/// <summary>
	/// Where traces and the view should originate from.
	/// </summary>
	[Sync]
	public virtual Vector3 EyePosition { get => WorldPosition; set => WorldPosition = value; }

	/// <summary>
	/// Where bullets and first person views should be pointed.
	/// </summary>
	[Sync]
	public virtual Rotation EyeRotation { get => WorldRotation; set => WorldRotation = value; }

	public Vector3 EyeForward => EyeRotation.Forward;

	public Transform EyeTransform => new( EyePosition, EyeRotation, 1f );

	/// <summary>
	/// Tells the view manager to process its transitions and offsets.
	/// </summary>
	public virtual void UpdateView( in float deltaTime )
	{
		if ( View.IsValid() )
			View.FrameSimulate( deltaTime );
	}

	/// <summary>
	/// Lets this pawn manipulate what is probably the main camera.
	/// </summary>
	public virtual void ApplyView( CameraComponent cam, ref Transform tView )
	{
		tView = View?.GetViewTransform() ?? tView;
	}

	/// <summary>
	/// Tells this pawn where they should be looking. <br />
	/// Now's a good time to update related components. <br />
	/// Typically called by the view manager.
	/// </summary>
	public virtual void SetLookRotation( in Rotation rLook )
	{
	}

	/// <summary>
	/// Called when the view targeting this pawn has finished updating. <br />
	/// It may be a view spectating this pawn, not the child view. <br />
	/// You should process clientside effects like model fading here.
	/// </summary>
	public virtual void OnViewUpdate( PawnView view )
	{
		if ( !view.IsValid() )
			return;

		var model = Actor?.Model;

		if ( model.IsValid() )
			model.OnViewUpdate( view );
	}
}
