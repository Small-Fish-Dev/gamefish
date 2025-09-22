namespace GameFish;

partial class BasePawn
{
	[Property]
	[Feature( PAWN )]
	public virtual BaseController Controller { get; set; }

	[Property]
	[Feature( PAWN ), Group( MODEL )]
	public virtual PawnModel ModelComponent { get; set; }

	protected virtual void UpdateController( in float deltaTime )
		=> Controller?.FrameSimulate( in deltaTime );
}
