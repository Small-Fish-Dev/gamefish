using System;

namespace GameFish.Nodes;

/// <summary>
/// Various toggleable features of nodes.
/// </summary>
[Flags]
public enum NodeFeatures
{
	/// <summary>
	/// Continuously run logic on a fixed
	/// tick rate whenever the node is enabled.
	/// <br /> <br />
	/// <b> NOTE: </b> Setting this to <c>0</c> makes it use the framerate.
	/// </summary>
	[Icon( "⌚" )]
	Tick = 1 << 0,

	/// <summary>
	/// Select and enable one <b>State</b> node at a time.
	/// All other nodes linked as <b>State</b> will be disabled.
	/// <br />
	/// <b> In other words: </b> A node-based state machine.
	/// <br /> <br />
	/// <b> NOTE: </b> To make a node a state you must mark
	/// it as a such when defining its link in the inspector.
	/// </summary>
	[Icon( "🚦" )]
	States = 1 << 1,

	/*
	/// <summary>
	/// Redirect signals to other connected nodes.
	/// <br /> <br />
	/// <b> NOTE: </b> Not yet implemented.
	/// </summary>
	[Icon( "📡" )]
	Relay = 1 << 2,

	/// <summary>
	/// Keep an updated list of all other nodes in the network for quick lookups.
	/// <br /> <br />
	/// <b> NOTE: </b> Not yet implemented.
	/// </summary>
	[Icon( "📶" )]
	Router = 1 << 3,
	*/
}
