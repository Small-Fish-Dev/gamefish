namespace GameFish;

/// <summary>
/// A dynamic(health/physics) entity with custom movement.
/// </summary>
public abstract partial class MovingEntity : DynamicEntity
{
	/// <summary>
	/// Directly performs movement/physics logic.
	/// </summary>
	protected abstract void Move( in float deltaTime, in bool isFixedUpdate );
}
