namespace GameFish;

partial class BasePawn
{
	/// <summary>
	/// The central view manager for the pawn.
	/// </summary>
	[Property]
	[Feature( FEATURE_PAWN ), Group( PawnView.VIEW )]
	public PawnView View
	{
		get => _view.IsValid() ? _view
			: _view = Modules.GetModule<PawnView>();

		set { _view = value; }
	}

	protected PawnView _view;

	[Property]
	[Feature( FEATURE_PAWN ), Group( PawnView.VIEW )]
	public ViewModel ViewModel => View?.ViewModel;

	public virtual void UpdateView( in float deltaTime )
	{
		if ( !View.IsValid() )
			return;

		View.FrameOperate( deltaTime );
	}

	/// <summary>
	/// Lets this pawn manipulate a camera.
	/// </summary>
	public virtual void ApplyView( CameraComponent cam, ref Transform tView )
	{
		tView = View?.GetViewTransform() ?? tView;
	}
}
