namespace GameFish;

/// <summary>
/// Allows running logic that varies by the input value.
/// <br /> <br />
/// <b> LOGIC: </b> Activating this will have it use the value as input.
/// <br /> <br />
/// <b> LOGIC: </b> Activation(s) from conditions will relay the provided input.
/// <code> logic_case </code>
/// </summary>
[Icon( "checklist" )]
[EditorHandle( Icon = "checklist" )]
public partial class LogicCaseEntity : LogicEntity
{
	protected const int LOGIC_CASES_ORDER = LOGIC_ORDER + 10;

	/// <summary>
	/// If enabled: print case matching results.
	/// </summary>
	[Property]
	[Title( "Logging (case)" )]
	[Order( LOGIC_DEBUG_ORDER )]
	[Feature( LOGIC ), Group( DEBUG )]
	public bool DebugLogCase { get; set; } = false;

	/// <summary>
	/// If the input matches any of these cases it will trigger its logic.
	/// </summary>
	[Property]
	[Order( LOGIC_CASES_ORDER )]
	[Feature( LOGIC ), Group( CASES )]
	[InlineEditor( Label = false ), WideMode( HasLabel = false )]
	public virtual List<LogicalCase> Cases { get; set; } = [new()];

	protected virtual bool TryExecute( object value )
	{
		var matchCount = 0;

		foreach ( var c in Cases )
			if ( c.TryEvaluate( source: this, value: value ) )
				matchCount++;

		if ( DebugLogCase )
			this.Log( $"Matched {matchCount} cases with value:[{value}]" );

		return matchCount > 0
			? LogicAction.TryExecute( OnSuccessLogic, source: this )
			: LogicAction.TryExecute( OnFailureLogic, source: this );
	}
}
