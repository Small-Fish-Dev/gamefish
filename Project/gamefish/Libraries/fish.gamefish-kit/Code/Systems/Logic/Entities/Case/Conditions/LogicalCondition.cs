namespace GameFish;

/// <summary>
/// Contains a series of checks that must pass for the condition to succeed and fire its logic.
/// </summary>
[Icon( "checklist" )]
public struct LogicalCondition
{
	[WideMode( HasLabel = false )]
	[InlineEditor( Label = false )]
	public List<LogicalCase> Cases { get; set; } = [new()];

	/// <summary>
	/// Logic to run if all of this condition's cases are met.
	/// </summary>
	[Group( LOGIC )]
	[Title( "On Success" )]
	[WideMode( HasLabel = true )]
	[InlineEditor( Label = true )]
	public List<LogicAction> OnSuccessLogic { get; set; }

	public LogicalCondition() { }

	public readonly bool TryEvaluate( object source, object value )
	{
		if ( Cases is null || Cases.Count <= 0 )
			return false;

		foreach ( var c in Cases )
			if ( !c.Matches( value ) )
				return false;

		LogicAction.TryExecute( OnSuccessLogic, source: source, value: value );

		return true;
	}
}
