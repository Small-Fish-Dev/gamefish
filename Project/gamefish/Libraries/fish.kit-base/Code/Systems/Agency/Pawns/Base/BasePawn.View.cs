namespace GameFish;

partial class BasePawn
{
	public const string SPECTATING = "👻 Spectating";

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

	/// <summary>
	/// If true: indicate that spectators can spectate this pawn. <br />
	/// If false: all spectators are blocked.
	/// </summary>
	[Property]
	[Feature( SPECTATING )]
	public virtual bool AllowSpectators { get; set; } = false;

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

	/// <param name="spec"> A spectator. </param>
	/// <returns> If the spectator can target this pawn. </returns>
	public virtual bool CanSpectate( BasePawn spec )
	{
		if ( !this.IsValid() )
			return false;

		if ( !AllowSpectators || !spec.IsValid() )
			return false;

		return true;
	}

	/// <param name="target"> The pawn we're trying to spectate. </param>
	/// <returns> If the spectating was successful. </returns>
	public virtual bool TrySpectate( BasePawn target )
	{
		// Only spectators can spectate.
		return false;
	}

	/// <summary>
	/// Kicks the spectator out of the fuggen thing, man.
	/// </summary>
	[Button]
	[Feature( SPECTATING ), Group( DEBUG )]
	public virtual void StopSpectating()
	{
	}
}
