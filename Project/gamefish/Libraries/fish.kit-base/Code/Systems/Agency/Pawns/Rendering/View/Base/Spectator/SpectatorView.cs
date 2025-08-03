namespace GameFish;

/// <summary>
/// A <see cref="PawnView"/> that can look through a <see cref="global::GameFish.SpectatorPawn"/> target's eyes.
/// </summary>
public partial class SpectatorView : PawnView
{
	public SpectatorPawn SpectatorPawn => ParentPawn as SpectatorPawn;
	public BasePawn SpectatorTarget => SpectatorPawn?.Spectating;

	/// <summary>
	/// Is our pawn a spectator that is spectating someone?
	/// </summary>
	public bool IsSpectating => SpectatorTarget.IsValid();

	public override BasePawn TargetPawn => SpectatorPawn is SpectatorPawn spec && spec.IsValid()
		? spec.Spectating.IsValid() ? spec.Spectating : spec
		: null;

	public override bool TrySetPosition( in Vector3 newPos )
		=> SpectatorPawn?.TrySetPosition( newPos ) ?? false;

	public override bool TrySetRotation( in Rotation rNew )
		=> SpectatorPawn?.TrySetRotation( rNew ) ?? false;

	protected override void SetTransformFromRelative()
	{
		if ( !IsSpectating )
			return;

		base.SetTransformFromRelative();
	}

	public override void StartTransition( in bool useWorldPosition = false )
	{
		// Don't transition while flying around.
		if ( !IsSpectating )
			return;

		OrbitingReset = null;

		base.StartTransition( useWorldPosition );

		ParentPawn?.SetVelocity( default );
	}

	public override void StopTransition()
	{
		base.StopTransition();

		ParentPawn?.SetVelocity( default );
	}

	protected override void UpdateTransition( in float deltaTime )
	{
		if ( !PreviousOffset.HasValue )
			return;

		// Stop any transitioning while flying around.
		if ( !IsSpectating )
		{
			StopTransition();
			return;
		}

		base.UpdateTransition( in deltaTime );
	}

	public override void CycleMode( in int dir )
	{
		if ( !EnsureFlyingMode() )
			base.CycleMode( dir );
	}

	/// <summary>
	/// Makes sure we're in first person while not spectating someone.
	/// </summary>
	/// <returns> If the mode was forced to first person. </returns>
	protected bool EnsureFlyingMode()
	{
		// Ensure first person while not spectating someone.
		if ( !IsSpectating )
		{
			if ( Mode != Perspective.FirstPerson )
			{
				Mode = Perspective.FirstPerson;
				StopTransition();
			}

			return true;
		}

		return false;
	}
}
