namespace GameFish;

/// <summary>
/// Allows running logic that varies by the input value.
/// <br /> <br />
/// <b> LOGIC: </b> Activating this will have it use the value as input.
/// <br /> <br />
/// <b> LOGIC: </b> Activation(s) from condtions will relay the provided input.
/// <code> logic_case </code>
/// </summary>
[Icon( "checklist" )]
[EditorHandle( Icon = "checklist" )]
public partial class LogicCaseEntity : LogicEntity
{
	protected const int CASE_ORDER = LOGIC_ORDER - 1000;

	protected const int CASE_DEBUG_ORDER = CASE_ORDER - 50;
	protected const int CASE_CONDITIONS_ORDER = CASE_ORDER + 10;

	/// <summary>
	/// If enabled: print case matching results.
	/// </summary>
	[Property]
	[Title( "Logging (case)" )]
	[Order( CASE_DEBUG_ORDER )]
	[Feature( CASE ), Group( DEBUG )]
	public bool DebugLogCase { get; set; } = false;

	/// <summary>
	/// If the input matches any of these cases it will trigger its logic.
	/// </summary>
	[Property]
	[Order( CASE_CONDITIONS_ORDER )]
	[Feature( CASE ), Group( CONDITIONS )]
	[InlineEditor( Label = false ), WideMode( HasLabel = false )]
	public virtual List<LogicalCondition> Conditions { get; set; } = [new()];

	protected virtual bool TryExecute( object value )
	{
		var matchCount = 0;

		foreach ( var c in Conditions )
			if ( c.TryEvaluate( source: this, value: value ) )
				matchCount++;

		if ( DebugLogCase )
			this.Log( $"Matched {matchCount} cases with value:[{value}]" );

		return matchCount > 0
			? LogicAction.TryExecute( OnSuccessLogic, source: this )
			: LogicAction.TryExecute( OnFailureLogic, source: this );
	}
}
