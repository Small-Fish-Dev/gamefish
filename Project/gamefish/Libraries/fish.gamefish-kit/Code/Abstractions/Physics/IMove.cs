namespace GameFish;

/// <summary>
/// Allows directly performing movement/physics logic.
/// </summary>
public interface IMove
{
	/// <summary>
	/// Directly performs movement/physics logic.
	/// </summary>
	void Move( in float deltaTime, in bool isFixedUpdate );
}
