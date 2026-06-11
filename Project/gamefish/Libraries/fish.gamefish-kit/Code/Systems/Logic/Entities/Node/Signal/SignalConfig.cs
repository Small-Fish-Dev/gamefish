using System;

namespace GameFish.Nodes;

/// <summary>
/// An inspector-friendly <see cref="LinkSignal"/> creation helper.
/// </summary>
[Icon( "⚡" )]
[Group( Library.GROUP_LOGIC )]
public struct SignalConfig
{
	/*
	/// <summary>
	/// The way we'll find the nodes we want to run this on.
	/// </summary>
	[WideMode( HasLabel = true )]
	public StringMatch Address { get; set; } = new( "", StringCompare.Caseless );
	*/

	[WideMode( HasLabel = true )]
	[Range( 0f, 5f, clamped: false )]
	public float Delay { get; set; } = 0f;

	[Group( COMMAND )]
	[WideMode( HasLabel = false )]
	[InlineEditor( Label = false )]
	public NodeCommand[] Commands { get; set; } = [new()];

	public SignalConfig() { }

	/// <returns> A fresh signal ready to be sent. </returns>
	public readonly LinkSignal ToLinkSignal( NodeEntity sender )
		=> new( sender, this );
}
