using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace GameFish.Nodes;

/// <summary>
/// Lets you add default links in the inspector.
/// </summary>
public struct DefaultLink : IEqualityComparer<DefaultLink>, IValid
{
	/// <summary>
	/// Returns if the node to link with is valid and that its component is enabled.
	/// </summary>
	[Hide, JsonIgnore]
	public readonly bool IsValid => Node.IsValid() && Node.Active;

	/// <summary>
	/// The node we're linked to.
	/// </summary>
	public NodeEntity Node { get; set; }

	/// <summary>
	/// The way this node interacts with the other node.
	/// </summary>
	[EnumButtonGroup]
	[WideMode( HasLabel = false )]
	public LinkProtocol Protocol { get; set; } = LinkProtocol.Peer;

	[Hide, JsonIgnore]
	public readonly bool IsPeer => Protocol is LinkProtocol.Peer;

	[Hide, JsonIgnore]
	public readonly bool IsParent => Protocol is LinkProtocol.Parent;

	[Hide, JsonIgnore]
	public readonly bool IsState => Protocol is LinkProtocol.State;

	[Hide, JsonIgnore]
	public readonly bool IsBlocked => Protocol is LinkProtocol.Blocked;

	public DefaultLink() { }

	public DefaultLink( NodeEntity node, LinkProtocol type )
	{
		Node = node;
		Protocol = type;
	}

	public override readonly int GetHashCode()
		=> Node.AsValid()?.GetHashCode() ?? 0;

	public readonly int GetHashCode( [DisallowNull] DefaultLink obj )
		=> obj.GetHashCode();

	public readonly bool Equals( DefaultLink x, DefaultLink y )
		=> x.Node == y.Node;
}
