using System;

namespace GameFish;

/// <summary>
/// How should something that's only ever on/off or open/closed be affected?
/// </summary>
[Icon( "toggle_on" )]
[Group( Library.NAME )]
[DefaultValue( Toggle )]
public enum ToggleCommand
{
	/// <summary>
	/// Stop/close.
	/// </summary>
	[Icon( "📫" )]
	Disable = 0,

	/// <summary>
	/// Activate/open.
	/// </summary>
	[Icon( "📭" )]
	Enable = 1,

	/// <summary>
	/// Switch between on/off, open/closed.
	/// </summary>
	[Icon( "♻" )]
	Toggle = 2
}

partial class Library
{
	/// <summary>
	/// Tells you what would be the result of this command given the state you provided.
	/// </summary>
	/// <returns> The resulting boolean. </returns>
	public static bool Apply( this ToggleCommand cmd, in bool isOn )
	{
		return cmd switch
		{
			ToggleCommand.Disable => false,
			ToggleCommand.Enable => true,
			ToggleCommand.Toggle => !isOn,

			// idk
			_ => false,
		};
	}

	/// <summary>
	/// Tells you what would be the result of this command given the instance you provided.
	/// </summary>
	/// <returns> The resulting boolean. </returns>
	public static bool Apply( this ToggleCommand cmd, IToggle toggle )
	{
		return cmd switch
		{
			ToggleCommand.Disable => false,
			ToggleCommand.Enable => true,
			ToggleCommand.Toggle => toggle?.IsOn is not true,

			// idk
			_ => false,
		};
	}
}
