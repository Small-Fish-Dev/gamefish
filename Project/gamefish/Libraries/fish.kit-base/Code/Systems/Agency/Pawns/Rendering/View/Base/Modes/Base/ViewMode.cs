namespace GameFish;

/// <summary>
/// A perspective to use with a <see cref="PawnView"/>. <br />
/// You can put this as a child or on the view component's object directly.
/// </summary>
[Icon( "remove_red_eye" )]
public abstract class ViewMode : Module
{
	protected const int VIEW_ORDER = DEFAULT_ORDER - 999;

	/// <summary>
	/// Should this bother calculating if we're in first person?
	/// </summary>
	[Property]
	[Feature( VIEW ), Order( VIEW_ORDER )]
	public virtual bool AllowFirstPerson { get; set; } = false;

	/// <summary>
	/// Display the view renderer in first person?
	/// </summary>
	[Property]
	[Feature( VIEW ), Order( VIEW_ORDER )]
	public virtual bool ShowViewRenderer { get; set; } = true;

	public override bool IsParent( ModuleEntity comp )
		=> comp is PawnView;

	public PawnView View => Parent as PawnView;

	protected BasePawn ParentPawn => View?.ParentPawn;
	protected BasePawn TargetPawn => View?.TargetPawn;
	protected ViewRenderer ViewRenderer => View?.ViewRenderer;

	protected Offset Relative
	{
		get => View?.Relative ?? default;
		set
		{
			if ( View.IsValid() )
				View.Relative = value;
		}
	}

	/// <summary>
	/// This view mode has just been set as the active one.
	/// </summary>
	public virtual void OnModeEnter( ViewMode previousMode = null )
	{
		View?.ToggleViewRenderer( InFirstPerson() );
	}

	/// <summary>
	/// Another view mode has been set as the active one.
	/// </summary>
	public virtual void OnModeExit( ViewMode nextMode = null )
	{
		if ( !View.IsValid() )
			return;

		if ( nextMode.IsValid() )
		{
			View.StartTransition();
			View.ToggleViewRenderer( nextMode.InFirstPerson() );
		}
	}

	/// <summary>
	/// A good place to set <see cref="Relative"/>, check for collisions, do movement etc.
	/// </summary>
	public virtual void OnModeUpdate( in float deltaTime )
	{
		UpdateViewRenderer( in deltaTime );
	}

	/// <summary>
	/// Sets the view's transform according to this mode.
	/// </summary>
	public virtual void SetViewTransform()
	{
		View?.SetTransformFromRelative();
	}

	/// <summary>
	/// Handles toggling and repositioning the view renderer.
	/// </summary>
	/// <param name="deltaTime"></param>
	public virtual void UpdateViewRenderer( in float deltaTime )
	{
		if ( View.IsValid() )
			return;

		var inFirstPerson = InFirstPerson();

		View.ToggleViewRenderer( inFirstPerson );

		if ( inFirstPerson && ViewRenderer.IsValid() )
			OnViewRender( in deltaTime );
	}

	/// <summary>
	/// The <see cref="ViewRenderer"/> is visbile.
	/// </summary>
	protected virtual void OnViewRender( in float deltaTime )
	{
		ViewRenderer?.UpdateOffset( deltaTime );
	}

	/// <summary>
	/// Determines if we're actually in first person based on
	/// the distance from this view to the eye position.
	/// </summary>
	public virtual bool InFirstPerson()
	{
		if ( !AllowFirstPerson || !View.IsValid() )
			return false;

		return View.DistanceFromEye <= 5f;
	}
}
