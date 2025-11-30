namespace GameFish;

/// <summary>
/// Something that takes input to move around. Meant to be controlled by a pawn.
/// </summary>
[Icon( "directions_run" )]
public abstract partial class BaseController : PhysicsEntity
{
	protected const int PAWN_ORDER = DEFAULT_ORDER - 444;

	protected const string AIMING = "Aiming";
	protected const int AIMING_ORDER = 1000;

	protected const string EYEPOS = "Eye Position";
	protected const int EYEPOS_ORDER = 2000;

	protected const string SPRINTING = "Sprinting";
	protected const int SPRINTING_ORDER = 4000;

	protected const string DUCKING = "Ducking";
	protected const int DUCKING_ORDER = 5000;

	protected const string JUMPING = "Jumping";
	protected const int JUMPING_ORDER = 6000;

	/// <summary>
	/// The pawn using this for movement etc.
	/// It should be on the same object.
	/// </summary>
	[Property]
	[Feature( PAWN ), Order( PAWN_ORDER )]
	public BasePawn Pawn
	{
		get => _pawn.IsValid() && _pawn.GameObject == GameObject ? _pawn
			: _pawn = Components?.Get<BasePawn>( FindMode.EverythingInSelf );

		set { _pawn = value; }
	}

	protected BasePawn _pawn;

	public PawnView View => Pawn?.View;
}
