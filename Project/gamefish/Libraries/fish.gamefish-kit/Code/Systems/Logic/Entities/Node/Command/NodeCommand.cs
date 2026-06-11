using System.Text.Json.Serialization;

namespace GameFish.Nodes;

/// <summary>
/// Some inspector-friendly networkable instructions for a node.
/// </summary>
[Icon( "📝" )]
[Group( Library.GROUP_LOGIC )]
public struct NodeCommand
{
	private const int ORDER_TOP = -100;

	/// <summary>
	/// The node that we're telling to do something.
	/// </summary>
	[Order( ORDER_TOP )]
	[WideMode( HasLabel = false )]
	public NodeEntity Node { get; set; }

	/// <summary>
	/// The type of command that the node should run.
	/// </summary>
	[Order( ORDER_TOP )]
	[WideMode( HasLabel = false )]
	public CommandProtocol Protocol { get; set; }

	/// <summary>
	/// The pattern and comparison mode to look for the method(s).
	/// <br /> <br />
	/// <b> NOTE: </b> This can and will run multiple
	/// methods if they match the pattern you specify.
	/// </summary>
	[WideMode( HasLabel = false )]
	[ShowIf( nameof( IsMethod ), true )]
	public StringMatch Method { get; set; } = new( "", StringCompare.Caseless );

	/// <summary>
	/// The way the target should toggle(if enabled).
	/// </summary>
	[EnumButtonGroup]
	[WideMode( HasLabel = false )]
	[ShowIf( nameof( Protocol ), CommandProtocol.Toggle )]
	public ToggleCommand Toggle { get; set; } = ToggleCommand.Enable;

	/// <summary>
	/// The state the target node should select.
	/// </summary>
	[WideMode( HasLabel = false )]
	[InlineEditor( Label = false )]
	[ShowIf( nameof( Protocol ), CommandProtocol.State )]
	public StateCommand StateCommand { get; set; }

	/// <summary>
	/// The node that the target node should connect with.
	/// </summary>
	[WideMode( HasLabel = false )]
	[ShowIf( nameof( IsLink ), true )]
	public NodeEntity LinkNode { get; set; }

	/// <summary>
	/// The relationship the target node should have with the linked node.
	/// </summary>
	[WideMode( HasLabel = false )]
	[ShowIf( nameof( Protocol ), CommandProtocol.Link )]
	public LinkProtocol LinkProtocol { get; set; }

	[Hide, JsonIgnore]
	public readonly bool IsPing => Protocol.IsPing();

	[Hide, JsonIgnore]
	public readonly bool IsLink => Protocol.IsLink() || Protocol.IsDisconnect();

	[Hide, JsonIgnore]
	public readonly bool IsToggle => Protocol.IsToggle();

	[Hide, JsonIgnore]
	public readonly bool IsTerminate => Protocol.IsTerminate();

	[Hide, JsonIgnore]
	public readonly bool IsState => Protocol.IsState();

	[Hide, JsonIgnore]
	public readonly bool IsMethod => Protocol.IsMethod();

	public NodeCommand() { }

	public readonly bool TryExecute( object source )
	{
		if ( !Node.IsValid() || !Node.Active )
			return false;

		switch ( Protocol )
		{
			// Ping
			case CommandProtocol.Ping:
				return Node.IsNodeEnabled();

			// Connect
			case CommandProtocol.Link:
				return Node.TryLinkToNode( LinkNode, LinkProtocol );

			// Disconnect
			case CommandProtocol.Disconnect:
				return Node.TryDisconnect( LinkNode );

			// Toggle
			case CommandProtocol.Toggle:
				return Node.TryToggle( Toggle );

			// Terminate
			case CommandProtocol.Terminate:
				Node.DestroyGameObject();
				return Node?.GameObject.IsDestroyed() is true;

			// State
			case CommandProtocol.State:
				return StateCommand.TryRun( Node );

			// Method
			case CommandProtocol.Method:
				if ( Node == source )
				{
					Node.Warn( "Tried to call a method on itself. Could crash so that's not supported yet!" );
					return false;
				}

				return Node.TryRunMethod( Method );
		}

		return false;
	}
}
