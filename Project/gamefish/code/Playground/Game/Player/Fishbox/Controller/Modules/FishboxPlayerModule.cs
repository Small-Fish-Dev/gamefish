
namespace Fishbox;

public abstract class FishboxPlayerModule : PlayerModule
{
	public override bool IsParent( ModuleEntity comp )
		=> comp is FishboxPlayer;

	public FishboxPlayer FishboxPlayer => Parent as FishboxPlayer;

	public virtual void UpdateInput( in float deltaTime ) { }

	public virtual void PreMove( in float deltaTime, in bool isFixedUpdate ) { }
	public virtual void PostMove( in float deltaTime, in bool isFixedUpdate ) { }
}
