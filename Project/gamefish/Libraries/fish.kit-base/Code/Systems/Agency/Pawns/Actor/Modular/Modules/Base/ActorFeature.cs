namespace GameFish;

/// <summary>
/// Provides a <see cref="ModularActor"/> some additional capacity.
/// </summary>
public abstract partial class ActorFeature : ActorModule
{
	/// <summary>
	/// Runs this feature's logic for its network owner.
	/// </summary>
	public virtual void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
	}
}
