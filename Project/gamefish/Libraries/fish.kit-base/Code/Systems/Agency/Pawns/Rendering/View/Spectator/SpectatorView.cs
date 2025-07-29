namespace GameFish;

/// <summary>
/// A <see cref="PawnView"/> that can look through a <see cref="SpectatorPawn"/> target's eyes.
/// </summary>
public partial class SpectatorView : PawnView
{
	public SpectatorPawn Spectator => ModuleParent as SpectatorPawn;

	public override BasePawn Pawn => Spectator is SpectatorPawn spec && spec.IsValid()
		? spec.Spectating ?? base.Pawn
		: base.Pawn;

	public override Angles EyeAngles
	{
		get => base.EyeAngles;
		set => base.EyeAngles = value;
	}

	public override void StartTransition( bool isRelative = false )
		=> base.StartTransition( isRelative );

	/// <summary>
	/// Makes sure we're in first person while not spectating someone.
	/// </summary>
	/// <returns> If the mode was forced to first person. </returns>
	protected bool EnsureFlyingMode()
	{
		var spec = Spectator;

		// Ensure first person while not spectating someone.
		if ( spec.IsValid() && !spec.Spectating.IsValid() )
		{
			if ( Mode != Perspective.FirstPerson )
			{
				Mode = Perspective.FirstPerson;
				return true;
			}
		}

		return false;
	}

	public override void FrameSimulate( in float deltaTime )
	{
		base.FrameSimulate( deltaTime );

		EnsureFlyingMode();
	}

	public override void CycleMode( in int dir )
	{
		/*var spec = Spectator;

		if ( spec.IsValid() && !spec.Spectating.IsValid() )
		{
			Mode = Perspective.FirstPerson;
			return;
		}*/

		base.CycleMode( dir );
	}
}
