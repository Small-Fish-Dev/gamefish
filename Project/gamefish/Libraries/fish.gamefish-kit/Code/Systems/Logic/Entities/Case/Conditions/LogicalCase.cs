namespace GameFish;

/// <summary>
/// Contains a series of checks that must pass for the case to succeed and fire its logic.
/// </summary>
[Icon( "checklist" )]
public struct LogicalCase
{
	private const int CONDITIONS_ORDER = 1;
	private const int FUNCTIONS_ORDER = 2;

	[Group( CONDITIONS )]
	[Order( CONDITIONS_ORDER )]
	[WideMode( HasLabel = false )]
	[InlineEditor( Label = false )]
	public List<LogicalCondition> Conditions { get; set; } = [new()];

	/// <summary>
	/// Logic to run if all of every case in this condition is passed.
	/// </summary>
	[Title( "On True" )]
	[Group( FUNCTIONS )]
	[Order( FUNCTIONS_ORDER )]
	[WideMode( HasLabel = true )]
	[InlineEditor( Label = true )]
	public List<LogicAction> OnTrueLogic { get; set; }

	public LogicalCase() { }

	public readonly bool TryEvaluate( object source, object value )
	{
		if ( Conditions is null || Conditions.Count <= 0 )
			return false;

		foreach ( var c in Conditions )
			if ( !c.Matches( value ) )
				return false;

		LogicAction.TryExecute( OnTrueLogic, source: source, value: value );

		// What's important here is that the conditions were satisfied.
		return true;
	}
}
