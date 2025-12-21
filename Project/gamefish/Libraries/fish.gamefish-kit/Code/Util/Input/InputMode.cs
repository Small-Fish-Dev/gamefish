namespace GameFish;

/// <summary>
/// The different states of a key/button to detect.
/// </summary>
public enum InputMode
{
	/// <summary>
	/// No activation.
	/// </summary>
	None = 0,

	/// <summary>
	/// Continuously while held.
	/// </summary>
	Held = 1,

	/// <summary>
	/// Once when the key is first pressed.
	/// </summary>
	Pressed = 2,

	/// <summary>
	/// Once when the key is let go of.
	/// </summary>
	Released = 3,
}

partial class Library
{
	/// <returns> If the mode matches the action's input state. </returns>
	public static bool Active( this InputMode mode, string action, bool complainOnMissing = true )
	{
		if ( action.IsBlank() )
			return false;

		return mode switch
		{
			InputMode.Held => Input.Down( action, complainOnMissing ),
			InputMode.Pressed => Input.Pressed( action ),
			InputMode.Released => Input.Released( action ),
			_ => false,
		};
	}
}
