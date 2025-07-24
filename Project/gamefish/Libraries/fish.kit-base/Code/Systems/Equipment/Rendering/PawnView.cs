namespace GameFish;

/// <summary>
/// The central view manager for the pawn. <br />
/// This should be on a child object of the pawn(otherwise expect problems).
/// </summary>
[Icon( "videocam" )]
public partial class PawnView : Module<BasePawn>
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
	public bool AllowCyclingMode { get; set; } = false;

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

	protected override void OnUpdate()
	{
		base.OnUpdate();

		HandleInput();

		OnPerspectiveUpdate( Time.Delta );

		UpdateTransition();
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();

		UpdateTransform();
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

	protected virtual void UpdateTransform()
	{
		var pawn = Pawn;

		if ( !pawn.IsValid() )
			return;

		if ( !pawn.IsValid() )
			return;

		if ( pawn.GameObject == GameObject )
		{
			this.Warn( this + " was directly on the pawn! It needs to be a child!" );
			Enabled = false;
			return;
		}

		var mode = GetMode();

		if ( mode is not Perspective.Fixed or Perspective.FreeCam )
		{
			if ( Previous is Offset prevOffset )
			{
				// Smoothed transitioning.
				var offsLerped = prevOffset.LerpTo( Relative, TransitionFraction );
				var tLerped = offsLerped.ToWorld( pawn.EyeTransform );

				TrySetPosition( tLerped.Position );
				TrySetRotation( tLerped.Rotation );
			}
			else
			{
				// No transitioning.
				var tRelative = Relative.ToWorld( pawn.EyeTransform );

				TrySetPosition( tRelative.Position );
				TrySetRotation( tRelative.Rotation );
			}
		}

		var cam = Scene?.Camera;

		if ( cam.IsValid() )
			cam.WorldTransform = WorldTransform;
	}
}
