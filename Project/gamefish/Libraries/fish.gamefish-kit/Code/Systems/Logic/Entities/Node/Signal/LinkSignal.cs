namespace GameFish.Nodes;

/// <summary>
/// It's like an internet packet that's designed to be routed through various connections.
/// </summary>
public readonly struct LinkSignal
{
	/// <summary>
	/// What is this packet trying to do, or want done?
	/// </summary>
	public CommandProtocol Protocol { get; }

	/// <summary>
	/// Where did this signal originate from?
	/// </summary>
	public NodeEntity Sender { get; }

	/// <summary>
	/// The nodes we're trying to send this signal to.
	/// </summary>
	public List<NodeEntity> Destinations { get; }

	/// <summary>
	/// The instructions to be delivered.
	/// </summary>
	public NodeCommand[] Commands { get; }

	/// <summary>
	/// A timer for when the signal is meant to arrive.
	/// </summary>
	[Hide]
	public TimeUntil Arrival { get; } = 0f;

	public LinkSignal() { }

	public LinkSignal( NodeEntity sender, SignalConfig cfg )
	{
		Sender = sender;
		
		// Destinations = sender?.FindNodes( cfg.Addresses );

		Commands = cfg.Commands;

		Arrival = cfg.Delay;
	}
}
