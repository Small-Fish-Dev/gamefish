
namespace Fishbox;

public abstract class PlayerModule : FishboxModule
{
	protected const int PLAYER_ORDER = GAME_ORDER - 500;

	public override bool IsParent( ModuleEntity comp )
		=> comp is FishboxPlayer;

	public FishboxPlayer Player => Parent as FishboxPlayer;

	public virtual void UpdateInput( in float deltaTime ) { }

	public virtual void PreMove( in float deltaTime, in bool isFixedUpdate ) { }
	public virtual void PostMove( in float deltaTime, in bool isFixedUpdate ) { }
}
