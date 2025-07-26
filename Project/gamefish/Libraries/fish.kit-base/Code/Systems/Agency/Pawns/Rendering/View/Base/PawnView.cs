namespace GameFish;

/// <summary>
/// The central view manager for the pawn. <br />
/// This should be on a child object of the pawn(otherwise expect problems).
/// </summary>
[Icon( "videocam" )]
public partial class PawnView : Module<BasePawn>, ISimulate
{
	public const string VIEW = "🎥 View";
	public const string SPECTATING = BasePawn.SPECTATING;

	public const string CYCLING = "Cycling";
	public const string TRANSITIONING = "Transitioning";

	/// <summary>
	/// The pawn we're looking at/through.
	/// </summary>
	public virtual BasePawn Pawn => ModuleParent;

	/// <summary>
	/// How to scale the opacity from distance.
	/// This is for the pawn itself to consider using.
	/// </summary>
	public float PawnOpacity { get; set; } = 1f;

	public virtual bool CanSimulate()
		=> ModuleParent?.CanSimulate() ?? false;

	protected override void OnStart()
	{
		base.OnStart();

		EnsureValidHierarchy();
	}

	public virtual void FrameSimulate( in float deltaTime )
	{
		HandleInput();

		OnPerspectiveUpdate( Time.Delta );

		UpdateTransition();

		PawnOpacity = DistanceFromEye.Remap( FirstPersonRange.Max, FirstPersonRange.Min );
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
}
