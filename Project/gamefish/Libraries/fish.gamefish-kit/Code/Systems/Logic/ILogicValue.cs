namespace GameFish;

/// <summary>
/// Allows logical entities to interact consistently with components that hold a value.
/// </summary>
[Icon( "123" )]
public interface ILogicValue
{
	public float Value { get; }

	/// <summary>
	/// Tries to set the value and tells you what the result is.
	/// </summary>
	/// <param name="value"> The value to assign. </param>
	/// <param name="result"> What the value was changed to. </param>
	/// <returns> If the value could be set to that. </returns>
	public bool TrySetValue( in float value, out float result );
}
