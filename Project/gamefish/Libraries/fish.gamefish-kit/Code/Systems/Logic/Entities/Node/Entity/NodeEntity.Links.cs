using System;
using System.Text.Json.Serialization;

namespace GameFish.Nodes;

partial class NodeEntity
{
	/// <summary>
	/// The node's public network ID. Kinda like a MAC address.
	/// <br /> <br />
	/// <b> NOTE: </b> You should really care to make
	/// this unique so that there are no conflicts.
	/// </summary>
	[Sync]
	[Property]
	[Title( "Name" )]
	[Order( NODE_LINKS_ORDER )]
	[Feature( NODE ), Group( LINKS )]
	public string NetworkName { get; set; }

	/// <summary>
	/// Sets it to the name of the object the node is on.
	/// </summary>
	[Button( "Reset" )]
	[Order( NODE_LINKS_ORDER )]
	[Feature( NODE ), Group( LINKS )]
	protected virtual void ResetNetworkName()
		=> NetworkName = GameObject?.Name;

	/// <summary>
	/// The links this node will try to establish when the node boots.
	/// </summary>
	[Property]
	[Title( "Defaults" )]
	[Order( NODE_LINKS_ORDER )]
	[Feature( NODE ), Group( LINKS )]
	[ShowIf( nameof( InEditor ), true )]
	[InlineEditor( Label = true ), WideMode( HasLabel = true )]
	public virtual List<DefaultLink> DefaultLinks { get; set; } = [new( null, LinkProtocol.Peer )];

	/// <summary>
	/// The other nodes this is connected to.
	/// </summary>
	[Property]
	[Title( "Active" )]
	[JsonIgnore, ReadOnly]
	[Order( NODE_LINKS_ORDER )]
	[Feature( NODE ), Group( LINKS )]
	[ShowIf( nameof( InGame ), true )]
	[WideMode( HasLabel = false ), InlineEditor( Label = false )]
	protected virtual Dictionary<NodeEntity, LinkProtocol> DebugActiveConnections
		=> Links?.ToDictionary() ?? DefaultLinks?.ToDictionary( nl => nl.Node, nl => nl.Protocol );

	/// <summary>
	/// One-way connections to other nodes and what we think of them.
	/// </summary>
	[Sync]
	public NetDictionary<NodeEntity, LinkProtocol> Links { get; protected set; }

	/// <summary>
	/// Queued up instructions keyed by a randomly generated ID.
	/// </summary>
	[Sync]
	public NetDictionary<Guid, LinkSignal> Signals { get; protected set; }

	/// <summary>
	/// Ensure links are the way they're meant to be. <br />
	/// Typically triggered by a change that may require some adjustments to links.
	/// </summary>
	protected virtual void UpdateLinks()
	{
		if ( IsProxy )
			return;

		UpdateLinkedStates();
	}

	/// <summary>
	/// Establish links as we've configured them.
	/// </summary>
	protected virtual void SetupDefaultLinks()
	{
		if ( DefaultLinks is null )
			return;

		// Links successfully established.
		int linkCount = 0;

		foreach ( var dl in DefaultLinks )
		{
			if ( !dl.IsValid )
				continue;

			if ( TryLinkToNode( dl.Node, dl.Protocol ) )
				linkCount++;
		}

		if ( DebugLogNode )
			this.Log( $"Established {linkCount} default links." );

		SetupDefaultStates();

		UpdateLinks();
	}

	/// <summary>
	/// A quick check to see if we have a valid, active and allowed link with that node.
	/// </summary>
	/// <param name="node"> The other node. </param>
	/// <returns> If there's any point in trying to speak with that node. </returns>
	public virtual bool IsLinkActive( NodeEntity node )
	{
		if ( GameObject.IsDestroyed() )
			return false;

		// We must be active ourselves.
		if ( !IsNodeEnabled() )
			return false;

		// The other node must be active too.
		if ( !node.IsValid() || !node.IsNodeEnabled() )
			return false;

		// If it's not in the table then we just aren't linked.
		if ( !TryGetLinkProtocol( node, out var ourProtocol ) )
			return false;

		// Link won't work if we blocked them.
		if ( ourProtocol is LinkProtocol.Blocked )
			return false;

		// Link also doesn't work if they blocked us.
		if ( node.TryGetLinkProtocol( this, out var theirProtocol ) )
			if ( theirProtocol is LinkProtocol.Blocked )
				return false;

		return true;
	}

	/// <summary>
	/// Looks up the node within our links to get what we think of it(even if disabled).
	/// </summary>
	/// <returns> If this has any relationship established with that node. </returns>
	public virtual bool TryGetLinkProtocol( NodeEntity node, out LinkProtocol protocol )
	{
		if ( !node.IsValid() || !node.Active )
			goto Fail;

		// Never try to signal ourselves.
		if ( node == this )
			goto Fail;

		if ( Links is null )
			goto Fail;

		if ( Links.TryGetValue( node, out protocol ) )
			return true;

		Fail:

		protocol = LinkProtocol.Blocked;

		return false;
	}

	/// <summary>
	/// A quick check to see if this are linked to that node(even if it's disabled).
	/// </summary>
	/// <returns> If the node is linked at all. </returns>
	public virtual bool IsLinked( NodeEntity node )
		=> TryGetLinkProtocol( node, out _ );

	/// <summary>
	/// A quick check to know if we are linked to that node with the specified protocol.
	/// </summary>
	/// <returns> If the node is controlled as a state of ours. </returns>
	public virtual bool IsProtocol( NodeEntity node, in LinkProtocol p )
	{
		if ( TryGetLinkProtocol( node, out var protocol ) )
			return protocol == p;

		return false;
	}

	/// <summary>
	/// A quick check to know if that node is an actively linked <b>State</b> node of ours(even if disabled).
	/// </summary>
	/// <returns> If the node is controlled as a state of ours. </returns>
	public virtual bool IsState( NodeEntity node )
	{
		// If states aren't enabled then they can't be one.
		if ( !StatesEnabled )
			return false;

		return IsProtocol( node, LinkProtocol.State );
	}

	/// <summary>
	/// A quick check to know if either this or the other node are blocking the link.
	/// </summary>
	/// <returns> If the node is controlled as a state of ours. </returns>
	public virtual bool IsBlocking( NodeEntity node )
	{
		// If states aren't enabled then they can't be one.
		if ( !StatesEnabled )
			return false;

		if ( !node.IsValid() || !node.Active )
			return true;

		if ( IsProtocol( node, LinkProtocol.Blocked ) )
			return true;

		if ( node.IsProtocol( this, LinkProtocol.Blocked ) )
			return true;

		return false;
	}

	/// <summary>
	/// Attempts to establish(or modify) a one-way link from this to that node.
	/// </summary>
	/// <returns> If the node is now linked(newly or modified). </returns>
	public virtual bool TryLinkToNode( NodeEntity node, in LinkProtocol p )
	{
		// They might be blocked.
		if ( !IsLinkToNodeAllowed( node, in p ) )
		{
			TryDisconnect( node );
			return false;
		}

		return TrySetLink( node, in p );
	}

	/// <returns> If this node can link to that node. </returns>
	public virtual bool IsLinkToNodeAllowed( NodeEntity node, in LinkProtocol p )
	{
		// Never link during our dying breaths.
		if ( GameObject.IsDestroyed() || !Active )
			return false;

		if ( !node.IsValid() || !node.Active )
			return false;

		if ( IsBlocking( node ) )
			return false;

		return true;
	}

	/// <summary>
	/// Directly defines the one-way link from this node to the other without much care.
	/// </summary>
	/// <returns> If the link was possible to be made. </returns>
	protected virtual bool TrySetLink( NodeEntity otherNode, in LinkProtocol p )
	{
		// If we can't even modify the table then don't bother.
		if ( IsProxy )
			return false;

		if ( !otherNode.IsValid() )
			return false;

		Links ??= [];

		// Don't run callbacks if the link is already that type.
		if ( TryGetLinkProtocol( otherNode, out var np ) )
			if ( np == p )
				return true;

		Links[otherNode] = p;

		if ( DebugLogNode )
		{
			this.Log( $"Linked to node:[{otherNode}] with protocol:[{p}]" );

			if ( otherNode.DebugLogNode )
				otherNode.Log( $"The node:[{otherNode}] linked to us with protocol:[{p}]" );
		}

		try
		{
			OnLinked( otherNode, in p );
			otherNode.OnRemoteLinked( this, in p );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( TrySetLink )} error with node:[{otherNode}]. Exception: {e}" );
		}

		return true;
	}

	/// <summary>
	/// Directly defines the one-way link from this node to the other.
	/// </summary>
	/// <returns> If the link was possible to be made. </returns>
	public virtual bool TryDisconnect( NodeEntity otherNode )
	{
		// If we can't even modify the table then don't bother.
		if ( IsProxy )
			return false;

		if ( !otherNode.IsValid() )
			return false;

		if ( Links is null )
			return true;

		Links.Remove( otherNode );

		if ( DebugLogNode )
		{
			this.Log( $"Disconnected from node:[{otherNode}]" );

			if ( otherNode.DebugLogNode )
				otherNode.Log( $"The node:[{otherNode}] disconnected from us." );
		}

		try
		{
			OnDisconnected( otherNode );
			otherNode.OnRemoteDisconnected( this );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( TryDisconnect )} error with node:[{otherNode}]. Exception: {e}" );
		}

		return true;
	}

	/// <summary>
	/// A link was just established from this node to another node.
	/// </summary>
	/// <param name="otherNode"> The other node that this linked to. </param>
	/// <param name="p"> This node's opinion of the other one. </param>
	protected virtual void OnLinked( NodeEntity otherNode, in LinkProtocol p )
	{
	}

	/// <summary>
	/// A link was
	/// </summary>
	/// <param name="otherNode"> The other node that linked to this one. </param>
	protected virtual void OnDisconnected( NodeEntity otherNode )
	{
	}

	/// <summary>
	/// Repond to another node being either established or updated.
	/// </summary>
	/// <param name="otherNode"> The other node that linked to this one. </param>
	/// <param name="p"> The other node's opinion of this one. </param>
	protected virtual void OnRemoteLinked( NodeEntity otherNode, in LinkProtocol p )
	{
	}

	/// <summary>
	/// Repond to another node disconnecting from this other.
	/// </summary>
	/// <param name="otherNode"> The other node that linked to this one. </param>
	protected virtual void OnRemoteDisconnected( NodeEntity otherNode )
	{
	}
}
