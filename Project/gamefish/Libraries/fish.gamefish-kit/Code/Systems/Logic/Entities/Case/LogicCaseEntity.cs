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
	[Feature( LOGIC )]
	[Order( LOGIC_ORDER )]
	[InlineEditor, WideMode( HasLabel = true )]
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
