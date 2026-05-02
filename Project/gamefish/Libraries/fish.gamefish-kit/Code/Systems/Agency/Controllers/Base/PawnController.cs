namespace GameFish;

/// <summary>
/// Something that takes input to move around.
/// <br /> <br />
/// <b> NOTE: </b> Meant to be controlled by a <see cref="Pawn"/>.
/// </summary>
[Icon( "directions_run" )]
public abstract partial class PawnController : PawnModule
{
	protected const int AIMING_ORDER = 1000;
	protected const int EYEPOS_ORDER = 2000;

	protected const int SPRINT_ORDER = 4000;
	protected const int DUCKING_ORDER = 5000;
	protected const int JUMPING_ORDER = 6000;

	public PawnView View => Pawn?.View;

	protected override void OnStart()
	{
		base.OnStart();

		SetupView();
	}

	/// <summary>
	/// Ran by the paret pawn just before movement is performed.
	/// </summary>
	public virtual void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
		UpdateInput( in deltaTime );
		SimulateView( in deltaTime );
	}
}
