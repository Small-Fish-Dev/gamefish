namespace GameFish.Nodes;

/// <summary>
/// Determines the intent of a node-specific command.
/// </summary>
public enum CommandProtocol
{
	/// <summary>
	/// Hey, what's up?
	/// </summary>
	[Icon( "📞" )]
	Ping = 0,

	/// <summary>
	/// Start/set a one-way link.
	/// </summary>
	[Icon( "🔗" )]
	Link = 1,

	/// <summary>
	/// Cut a link off.
	/// </summary>
	[Icon( "💔" )]
	Disconnect = 2,

	/// <summary>
	/// Switch on/off.
	/// </summary>
	[Icon( "♻" )]
	Toggle = 5,

	/// <summary>
	/// State machine interaction.
	/// </summary>
	[Icon( "🚦" )]
	State = 10,

	/// <summary>
	/// Destroy yourself.
	/// </summary>
	[Icon( "💀" )]
	Terminate = 20,

	/// <summary>
	/// Do something special for me.
	/// </summary>
	[Icon( "⚡" )]
	Method = 30,
}

partial class NodeExtensions
{
	/// <returns> If it's meant to be a ping. </returns>
	public static bool IsPing( this CommandProtocol s )
		=> s is CommandProtocol.Ping;

	/// <returns> If it's meant to establish/alter a link to another node. </returns>
	public static bool IsLink( this CommandProtocol s )
		=> s is CommandProtocol.Link;

	/// <returns> If it's meant to cut a node off. </returns>
	public static bool IsDisconnect( this CommandProtocol s )
		=> s is CommandProtocol.Disconnect;

	/// <returns> If it's meant to toggle a node on/off. </returns>
	public static bool IsToggle( this CommandProtocol s )
		=> s is CommandProtocol.Toggle;

	/// <returns> If it's meant to call a node-specific function. </returns>
	public static bool IsTerminate( this CommandProtocol s )
		=> s is CommandProtocol.Terminate;

	/// <returns> If it's meant to call a function. </returns>
	public static bool IsState( this CommandProtocol s )
		=> s is CommandProtocol.State;

	/// <returns> If it's meant to call a node-specific function. </returns>
	public static bool IsMethod( this CommandProtocol s )
		=> s is CommandProtocol.Method;
}
