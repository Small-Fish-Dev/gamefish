using System.Text.Json.Serialization;

namespace Playground;

/// <summary>
/// Categories for sandbox/editor tools.
/// </summary>
[DefaultValue( Default )]
public enum ToolType
{
	/// <summary>
	/// Uncategorized tools.
	/// </summary>
	[Order( 500 )]
	[Group( "General" )]
	Default,

	/// <summary>
	/// Some pretty useful things.
	/// </summary>
	[Order( -1 )]
	[Icon( "🛠" )]
	[Group( "Utility" )]
	Utility,

	/// <summary>
	/// For adding new stuff.
	/// </summary>
	[Order( 10 )]
	[Icon( "👶" )]
	[Group( "Creation" )]
	Creation,

	/// <summary>
	/// Physics interaction.
	/// </summary>
	[Order( 20 )]
	[Icon( "🍎" )]
	[Group( "Physics" )]
	Physics,

	/// <summary>
	/// Physical behaviors.
	/// </summary>
	[Order( 25 )]
	[Icon( "💪" )]
	[Group( "Joints" )]
	Constraint,

	/// <summary>
	/// Silly/weird stuff
	/// </summary>
	[Order( 40 )]
	[Icon( "👽" )]
	[Group( "Fun" )]
	Fun,

	/// <summary>
	/// You are the law.
	/// </summary>
	[Order( 999 )]
	[Icon( "👮" )]
	[Group( "Admin" )]
	Administration,
}
