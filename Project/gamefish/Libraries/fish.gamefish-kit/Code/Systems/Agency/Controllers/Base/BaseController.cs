using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Something that takes input to move around.
/// <br /> <br />
/// <b> NOTE: </b> Meant to be controlled by a pawn.
/// </summary>
[Icon( "directions_run" )]
public abstract partial class BaseController : Module
{
	protected const int PAWN_ORDER = DEFAULT_ORDER - 444;

	protected const int AIMING_ORDER = 1000;
	protected const int EYEPOS_ORDER = 2000;
	protected const int SPRINT_ORDER = 4000;
	protected const int DUCKING_ORDER = 5000;
	protected const int JUMPING_ORDER = 6000;

	public override bool IsParent( ModuleEntity comp )
		=> comp is Pawn;

	/// <summary>
	/// The pawn using this for movement etc.
	/// <br /> <br />
	/// <b> NOTE: </b> It should probably be on the same object.
	/// </summary>
	[Property, ReadOnly, JsonIgnore]
	[Feature( PAWN ), Order( PAWN_ORDER )]
	public Pawn Pawn => Parent as Pawn;

	public PawnView View => Pawn?.View;

	protected override void OnStart()
	{
		base.OnStart();

		SetupView();
	}

	/// <summary>
	/// Typically ran by the associated pawn just before movement is performed.
	/// </summary>
	public virtual void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
		SimulateView( in deltaTime );
	}
}
