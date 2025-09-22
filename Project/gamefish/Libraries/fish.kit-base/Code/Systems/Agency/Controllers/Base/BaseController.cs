namespace GameFish;

/// <summary>
/// Something that takes input to move around. Meant to be controlled by a pawn.
/// </summary>
public abstract partial class BaseController : PhysicsEntity
{
	[Property, Feature( PAWN )]
	public BasePawn Pawn
	{
		get => _pawn.IsValid() && _pawn.GameObject == GameObject ? _pawn
			: _pawn = Components?.Get<BasePawn>( FindMode.EverythingInSelf );

		set { _pawn = value; }
	}

	protected BasePawn _pawn;

	public PawnView View => Pawn?.View;
}
