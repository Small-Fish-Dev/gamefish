namespace GameFish;

partial class BaseController : ISimulate
{
	[Sync]
	public float WishVelocity { get; set; }

	public bool CanSimulate() => !IsProxy;

	public void FrameSimulate( in float deltaTime )
	{
		Move( deltaTime );
	}

	public abstract void Move( in float deltaTime );

	protected virtual void PreMove( in float deltaTime ) { }

	protected virtual void PostMove( in float deltaTime )
	{

	}

	public virtual void UpdateView( in float deltaTime ) { }
}
