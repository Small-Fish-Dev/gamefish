namespace GameFish;

/// <summary>
/// The central view manager for the pawn. <br />
/// This should be on a child object of the pawn(otherwise expect problems).
/// </summary>
[Icon( "videocam" )]
public partial class PawnView : Module<BasePawn>, IOperate
{
	public const string VIEW = "🎥 View";
	public const string INPUT = "🕹 Input";

	public const string CYCLING = "Cycling";
	public const string TRANSITIONING = "Transitioning";

	/// <summary>
	/// If true: you can press buttons to cycle through perspectives.
	/// </summary>
	[Property]
	[Title( "Allow" )]
	[Feature( INPUT ), Group( CYCLING )]
	public virtual bool AllowCyclingMode { get; set; } = false;

	/// <summary>
	/// The button to press to select the next mode.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Forward" )]
	[Feature( INPUT ), Group( CYCLING )]
	public string CycleModeForwardAction { get; set; } = "View";

	/// <summary>
	/// The button to press to select the next mode.
	/// </summary>
	[Property]
	[InputAction]
	[Title( "Backward" )]
	[Feature( INPUT ), Group( CYCLING )]
	public string CycleModeBackwardAction { get; set; }

	/// <summary>
	/// The pawn we're looking at/through.
	/// </summary>
	public virtual BasePawn Pawn => ModuleParent;

	public Vector3 EyePosition => Pawn?.EyePosition ?? WorldPosition;
	public Rotation EyeRotation => Pawn?.EyeRotation ?? WorldRotation;
	public Vector3 EyeForward => EyeRotation.Forward;

	/// <summary>
	/// Distance from this view to the pawn's first-person origin.
	/// </summary>
	public float DistanceFromEye => WorldPosition.Distance( EyePosition );

	public virtual bool CanOperate()
		=> ModuleParent?.CanOperate() ?? false;

	protected override void OnStart()
	{
		base.OnStart();

		EnsureValidHierarchy();
	}

	public virtual void FrameOperate( in float deltaTime )
	{
		HandleInput();

		OnPerspectiveUpdate( Time.Delta );

		UpdateTransition();
	}

	/// <summary>
	/// Checks if something would fuck up and if so: warns about it then disables this view.
	/// </summary>
	protected void EnsureValidHierarchy()
	{
		var pawn = Pawn;

		if ( !pawn.IsValid() )
			return;

		if ( pawn.GameObject == GameObject )
		{
			this.Warn( this + " was directly on the pawn! It needs to be a child!" );
			Enabled = false;
		}
	}

	/// <summary>
	/// Allows cycling of perspective modes.
	/// </summary>
	protected virtual void HandleInput()
	{
		if ( !AllowCyclingMode )
			return;

		if ( !string.IsNullOrEmpty( CycleModeForwardAction ) )
			if ( Input.Pressed( CycleModeForwardAction ) )
				CycleMode( 1 );

		if ( !string.IsNullOrEmpty( CycleModeBackwardAction ) )
			if ( Input.Pressed( CycleModeBackwardAction ) )
				CycleMode( -1 );
	}
}
