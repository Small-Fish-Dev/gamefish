namespace GameFish.Nodes;

/// <summary>
/// Determines the procedures to use when linked with a specific node.
/// </summary>
public enum LinkProtocol
{
	/// <summary>
	/// Howdy neighbor.
	/// </summary>
	[Icon( "🧔" )]
	Peer = 0,

	/// <summary>
	/// Something this node should respect.
	/// <br /> <br />
	/// <b> TODO: </b> Fix this shitpost of a description.
	/// </summary>
	[Icon( "👑" )]
	Parent = 1,

	/// <summary>
	/// A state we can select.
	/// <br /> <br />
	/// <b> NOTE: </b> Selecting a state disables others.
	/// </summary>
	[Icon( "🚦" )]
	State = 2,

	/// <summary>
	/// Ignoring signals from this link.
	/// </summary>
	[Icon( "🔥" )]
	Blocked = 8,
}
