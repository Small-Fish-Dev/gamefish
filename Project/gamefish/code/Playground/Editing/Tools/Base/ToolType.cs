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
	/// For adding/building new stuff.
	/// </summary>
	[Order( 10 )]
	[Icon( "👷" )]
	[Group( "Building" )]
	Construction,

	/// <summary>
	/// Physics interaction.
	/// </summary>
	[Order( 20 )]
	[Icon( "🍎" )]
	[Group( "Physics" )]
	Physics,

	/// <summary>
	/// Contraption helpers.
	/// </summary>
	[Order( 30 )]
	[Icon( "⚙" )]
	[Group( "Joints" )]
	Mechanical,

	/// <summary>
	/// Automobiles.
	/// </summary>
	[Order( 40 )]
	[Icon( "🚘" )]
	[Group( "Vehicles" )]
	Vehicle,

	/// <summary>
	/// Non-player characters.
	/// </summary>
	[Order( 40 )]
	[Icon( "🤖" )]
	[Group( "NPCs" )]
	Actor,

	/// <summary>
	/// Silly/weird stuff.
	/// </summary>
	[Order( 60 )]
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
