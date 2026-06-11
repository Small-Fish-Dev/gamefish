using System;
using System.Text.Json.Serialization;

namespace GameFish.Nodes;

/// <summary>
/// A string-identified signalable function for a specific node.
/// </summary>
[Icon( "⚡" )]
[Group( Library.GROUP_LOGIC )]
public struct NodeMethod
{
	/// <summary>
	/// The identifying string of the method.
	/// <br /> <br />
	/// <b> NOTE: </b> Other nodes can find and call this
	/// method using various pattern matching modes.
	/// </summary>
	[Group( METHOD )]
	[WideMode( HasLabel = false )]
	public string Name { get; set; } = "Method";

	[Group( METHOD )]
	[InlineEditor( Label = false ), WideMode( HasLabel = false )]
	public List<LogicAction> LogicActions { get; set; } = [new( LogicAction.ActionType.Node )];

	public NodeMethod() { }

	/// <summary>
	/// Creates a default method with a preset name.
	/// </summary>
	public NodeMethod( string name )
	{
		Name = name;
	}

	public readonly bool TryRun( NodeEntity source, object value = null )
	{
		if ( !source.IsValid() || !source.Active )
			return false;

		if ( source.GameObject.IsDestroyed() )
			return false;

		if ( source.DebugLogNode )
			source.Log( $"Running method:[{Name}]" );

		return LogicAction.TryExecute( LogicActions, source: source, value: value );
	}
}
