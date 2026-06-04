namespace GameFish;

/// <summary>
/// Contains a series of checks that must pass for the condition to succeed and fire its logic.
/// </summary>
[Icon( "checklist" )]
public struct LogicalCondition
{
	private const int CASES_ORDER = 1;
	private const int FUNCTIONS_ORDER = 2;

	[Group( CASES )]
	[Order( CASES_ORDER )]
	[WideMode( HasLabel = false )]
	[InlineEditor( Label = false )]
	public List<LogicalCase> Cases { get; set; } = [new()];

	/// <summary>
	/// Logic to run if all of every case evaluates as true.
	/// </summary>
	[Title( "On True" )]
	[Group( FUNCTIONS )]
	[Order( FUNCTIONS_ORDER )]
	[WideMode( HasLabel = true )]
	[InlineEditor( Label = true )]
	public List<LogicAction> OnTrueLogic { get; set; }

	public LogicalCondition() { }

	public readonly bool TryEvaluate( object source, object value )
	{
		if ( Cases is null || Cases.Count <= 0 )
			return false;

		foreach ( var c in Cases )
			if ( !c.Matches( value ) )
				return false;

		LogicAction.TryExecute( OnTrueLogic, source: source, value: value );

		// What's important here is that the conditions were satisfied.
		return true;
	}
}
