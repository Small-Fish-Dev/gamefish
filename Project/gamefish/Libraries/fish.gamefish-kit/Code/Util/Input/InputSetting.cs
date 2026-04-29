using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Defines a key/button and how it is activated.
/// </summary>
public struct InputSetting
{
	/// <summary>
	/// The key/button.
	/// </summary>
	[InputAction]
	[KeyProperty]
	public string Action { get; set; }

	/// <summary>
	/// The different states of a key/button to detect.
	/// </summary>
	[KeyProperty]
	public InputMode Mode { get; set; } = InputMode.Pressed;

	/// <summary>
	/// Is the input active according to the configured state?
	/// </summary>
	[Hide, JsonIgnore, ReadOnly]
	public readonly bool IsActive => Mode.IsActive( Action );

	/// <summary>
	/// Is the input being actively held?
	/// </summary>
	[Hide, JsonIgnore, ReadOnly]
	public readonly bool IsHeld => InputMode.Held.IsActive( Action );

	/// <summary>
	/// Was the input just pressed?
	/// </summary>
	[Hide, JsonIgnore, ReadOnly]
	public readonly bool IsPressed => InputMode.Pressed.IsActive( Action );

	/// <summary>
	/// Was the input just released?
	/// </summary>
	[Hide, JsonIgnore, ReadOnly]
	public readonly bool IsReleased => InputMode.Released.IsActive( Action );

	public InputSetting() { }

	public InputSetting( in string action, in InputMode mode )
	{
		Action = action;
		Mode = mode;
	}
}
