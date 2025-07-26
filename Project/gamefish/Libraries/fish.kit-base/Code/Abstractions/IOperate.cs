namespace GameFish;

/// <summary>
/// Indicates something can be manually simulated if a check passes. <br />
/// For example: an <see cref="Agent"/> telling their pawns to do stuff.
/// </summary>
public interface ISimulate
{
	/// <summary>
	/// Is this allowed to call frame and/or fixed update operations? <br />
	/// This is a good place to put an ownership or input focus check.
	/// </summary>
	public bool CanSimulate();

	/// <summary> Called during OnUpdate(if allowed). </summary>
	public void FrameSimulate( in float deltaTime ) { }

	/// <summary> Called during OnFixedUpdate(if allowed). </summary>
	public void FixedSimulate( in float deltaTime ) { }
}
