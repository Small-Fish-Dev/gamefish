namespace GameFish;

partial class BasePawn
{
	/// <summary>
	/// The central view manager for the pawn.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( VIEW )]
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

	/// <summary> The base vision trace will ignore objects with these tags. </summary>
	[Property]
	[Feature( PAWN ), Group( PawnView.VIEW )]
	public virtual TagSet EyeTraceIgnore { get; set; } = ["water"];

	/// <summary>
	/// Where traces and the view should originate from.
	/// </summary>
	public virtual Vector3 LocalEyePosition
	{
		get
		{
			if ( Controller is var c && c.IsValid() )
				return c.LocalEyePosition;
			else if ( View is var view && view.IsValid() )
				return WorldTransform.PointToLocal( view.ViewPosition );

			return Center;
		}
		set
		{
			if ( Controller is var c && c.IsValid() )
				c.LocalEyePosition = value;
			else if ( View is var view && view.IsValid() )
				view.ViewPosition = WorldTransform.PointToWorld( value );
		}
	}

	/// <summary>
	/// Where bullets and first person views should be pointed.
	/// </summary>
	public virtual Rotation LocalEyeRotation
	{
		get
		{
			if ( Controller is var c && c.IsValid() )
				return c.LocalEyeRotation;

			return WorldTransform.RotationToLocal( View?.ViewRotation ?? WorldRotation );
		}
		set
		{
			if ( Controller is var c && c.IsValid() )
				c.LocalEyeRotation = value;
			else if ( View is var view && view.IsValid() )
				view.ViewRotation = WorldTransform.RotationToWorld( value );
		}
	}

	public virtual Vector3 EyePosition
	{
		get => WorldTransform.PointToWorld( LocalEyePosition );
		set => LocalEyePosition = WorldTransform.PointToLocal( value );
	}

	public virtual Rotation EyeRotation
	{
		get => WorldTransform.RotationToWorld( LocalEyeRotation );
		set => LocalEyeRotation = WorldTransform.RotationToLocal( value );
	}

	public Transform EyeTransform => new( EyePosition, EyeRotation, WorldScale );
	public Vector3 EyeForward => EyeRotation.Forward;

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
	public virtual bool TryApplyView( CameraComponent cam, ref Transform tView )
	{
		if ( View is not PawnView view || !view.IsValid() )
			return false;

		tView = view.GetViewTransform();

		return true;
	}

	/// <summary>
	/// Tells this pawn where they should be looking. <br />
	/// Now's a good time to update related components. <br />
	/// Typically called by the view manager.
	/// </summary>
	public virtual void OnSetLookRotation( in Rotation rLook )
	{
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

		if ( ModelComponent is PawnModel model && model.IsValid() )
			model.OnViewUpdate( view );
	}

	/// <returns> A default trace with vision filters. </returns>
	public virtual SceneTrace GetVisionTrace()
		=> Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject )
			.WithoutTags( EyeTraceIgnore );

	public SceneTrace GetEyeTrace( Vector3 from, Vector3 to )
		=> GetVisionTrace()
			.FromTo( from, to );

	public SceneTrace GetEyeTrace( Vector3 to )
		=> GetVisionTrace()
			.FromTo( EyePosition, to );

	public SceneTrace GetEyeTrace( float distance, Vector3? dir = null )
	{
		var startPos = EyePosition;
		var endPos = startPos + ((dir ?? EyeForward) * distance);

		var tr = GetVisionTrace()
			.FromTo( startPos, endPos );

		return tr;
	}
}
