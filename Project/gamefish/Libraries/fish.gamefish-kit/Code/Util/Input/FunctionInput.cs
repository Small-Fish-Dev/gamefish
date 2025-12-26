namespace GameFish;

/// <summary>
/// Defines the input configuration to check before executing a function.
/// </summary>
public partial class FunctionInput
{
	/// <summary>
	/// The key/button.
	/// </summary>
	[InputAction]
	[KeyProperty]
	public virtual string Action { get; set; } = "Attack1";

	/// <summary>
	/// The different states of a key/button to detect.
	/// </summary>
	[KeyProperty]
	public virtual InputMode Mode { get; set; } = InputMode.Pressed;

	/// <summary>
	/// The default delay before this can be activated again.
	/// </summary>
	[KeyProperty]
	[Range( 0f, 5f, clamped: false )]
	public virtual float Cooldown { get; set; } = 0.5f;

	public FunctionInput() { }

	public FunctionInput( in string action, InputMode mode, in float delay )
	{
		Action = action;
		Mode = mode;
		Cooldown = delay;
	}

	/// <returns> If the input is active this frame. </returns>
	public virtual bool IsInputting()
		=> Mode.Active( Action ) && !IControls.BlockActions;
}
