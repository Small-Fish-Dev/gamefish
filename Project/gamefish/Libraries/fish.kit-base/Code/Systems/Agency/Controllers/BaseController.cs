namespace GameFish;

public abstract partial class BaseController : PhysicsEntity
{
	[Property, Group( PAWN )]
	public BasePawn Pawn
	{
		get => _pawn.IsValid() ? _pawn
			: _pawn = Components?.Get<BasePawn>( FindMode.EverythingInSelf );

		set { _pawn = value; }
	}

	protected BasePawn _pawn;

	[Property, Feature( VIEW )]
	public PawnView View => Pawn?.View;

	[Property]
	[Title( "Standing" )]
	[Feature( VIEW ), Group( "Eye Height" )]
	public virtual float EyeHeightStand { get; set; } = 64f;

	[Property]
	[Title( "Ducked" )]
	[Feature( VIEW ), Group( "Eye Height" )]
	public virtual float EyeHeightDuck { get; set; } = 32f;

	public virtual bool Standing => true;

	public virtual float EyeHeight
		=> Standing ? EyeHeightStand : EyeHeightDuck;
}
