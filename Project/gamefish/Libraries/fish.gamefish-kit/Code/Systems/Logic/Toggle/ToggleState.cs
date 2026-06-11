namespace GameFish;

/// <summary>
/// A boolean that looks prettier in the inspector.
/// </summary>
[Icon( "multiple_stop" )]
[DefaultValue( Disabled )]
public enum ToggleState
{
	/// <summary>
	/// Flipped off. Not doing shit.
	/// </summary>
	[Icon( "❌" )]
	Disabled = 0,

	/// <summary>
	/// Flipped on. Doing stuff, things even.
	/// </summary>
	[Icon( "✅" )]
	Enabled = 1,
}

partial class Library
{
	/// <returns> The only other state it could be. </returns>
	public static ToggleState Flip( this ToggleState state )
		=> state is ToggleState.Enabled ? ToggleState.Disabled : ToggleState.Enabled;

	/// <returns> If the state is defined as enabled. </returns>
	public static bool IsEnabled( this ToggleState state )
		=> state is ToggleState.Enabled;

	/// <returns> If the state is not defined as enabled. </returns>
	public static bool IsDisabled( this ToggleState state )
		=> state is not ToggleState.Enabled;

	/// <returns> Either <c>true</c> or <c>false</c>. </returns>
	public static bool ToBool( this ToggleState state )
		=> state.IsEnabled();

	/// <returns> Either <c>0</c> if <c>false</c> or <c>1</c> if <c>true</c>. </returns>
	public static int ToInt( this ToggleState state )
	{
		return state switch
		{
			ToggleState.Enabled => 1,
			ToggleState.Disabled => 0,
			_ => 0,
		};
	}

	/// <returns> The command meant to assign this state. </returns>
	public static ToggleCommand ToCommand( this ToggleState state )
	{
		return state switch
		{
			ToggleState.Enabled => ToggleCommand.Enable,
			ToggleState.Disabled => ToggleCommand.Disable,
			_ => ToggleCommand.Enable,
		};
	}
}
