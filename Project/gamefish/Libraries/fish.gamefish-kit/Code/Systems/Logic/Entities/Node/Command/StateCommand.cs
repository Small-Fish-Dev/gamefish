namespace GameFish.Nodes;

/// <summary>
/// A command for a node enabled as a state machine.
/// </summary>
public struct StateCommand
{
	public enum Instruction
	{
		/// <summary>
		/// Set the state directly.
		/// </summary>
		[Icon( "🍇" )]
		Select,

		/// <summary>
		/// Choose the next/previous state.
		/// <br /> <br />
		/// <b> NOTE: </b> Not yet implemented.
		/// </summary>
		[Hide]
		[Icon( "♻" )]
		Cycle,

		/// <summary>
		/// Deselect any active state.
		/// </summary>
		[Icon( "🧼" )]
		Clear,
	}

	public enum CyleMode
	{
		[Icon( "👉" )]
		Forward,

		[Icon( "👈" )]
		Backward,

		[Icon( "🎲" )]
		Random,

		[Icon( "🏳" )]
		First,

		[Icon( "🏴" )]
		Last
	}

	/// <summary>
	/// How we're trying to modify the state machine.
	/// </summary>
	[Title( "Instruction" )]
	[WideMode( HasLabel = false )]
	public Instruction Type { get; set; } = Instruction.Select;

	/// <summary>
	/// The state the target node should select.
	/// </summary>
	[WideMode( HasLabel = false )]
	[ShowIf( nameof( Type ), Instruction.Select )]
	public NodeEntity State { get; set; }

	/// <summary>
	/// The state the target node should select.
	/// </summary>
	[WideMode( HasLabel = false )]
	[ShowIf( nameof( Type ), Instruction.Cycle )]
	public CyleMode Cycle { get; set; }

	public StateCommand() { }

	public StateCommand( in Instruction type )
	{
		Type = type;
	}

	/// <summary>
	/// Attempts to run a command on a state machine node.
	/// </summary>
	/// <param name="node"> The node to execute the command on. </param>
	/// <returns> If the execution was successful. </returns>
	public readonly bool TryRun( NodeEntity node )
	{
		if ( !node.IsValid() || !node.Active )
			return false;

		if ( !node.StatesEnabled )
			return false;

		switch ( Type )
		{
			case Instruction.Select:
				return node.TrySelectState( State );

			// TODO: State cycling.
			case Instruction.Cycle:
				break;

			case Instruction.Clear:
				return node.TrySelectState( null );
		}

		return false;
	}
}
