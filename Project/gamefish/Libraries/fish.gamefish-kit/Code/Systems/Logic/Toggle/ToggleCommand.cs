using System;

namespace GameFish;

/// <summary>
/// How should something that's only ever on/off or open/closed be affected?
/// </summary>
[Group( Library.NAME )]
[DefaultValue( Toggle )]
public enum ToggleCommand
{
	/// <summary>
	/// Stop/close.
	/// </summary>
	[Icon( "📫" )] Disable = 0,

	/// <summary>
	/// Activate/open.
	/// </summary>
	[Icon( "📭" )] Enable = 1,

	/// <summary>
	/// Switch between on/off, open/closed.
	/// </summary>
	[Icon( "♻" )] Toggle = 2
}
