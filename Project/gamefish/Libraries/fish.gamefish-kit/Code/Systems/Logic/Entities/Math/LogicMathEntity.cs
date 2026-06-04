namespace GameFish;

/// <summary>
/// Performs calculations on logical values.
/// <br /> <br />
/// <b> NOTE: </b> Pairs very well with <see cref="LogicCounterEntity"/>).
/// <br /> <br />
/// <b> NOTE: </b> Also works with any component implementing <see cref="ILogicValue"/>.
/// </summary>
[Icon( "calculate" )]
[EditorHandle( Icon = "calculate" )]
public partial class LogicMathEntity : LogicEntity
{
	protected const int MATH_ORDER = LOGIC_ORDER - 1000;

	protected const int MATH_DEBUG_ORDER = MATH_ORDER - 50;
	protected const int MATH_LOGIC_ORDER = MATH_ORDER + 10;

	/// <summary>
	/// If enabled: print math operations to console.
	/// </summary>
	[Property]
	[Title( "Logging (math)" )]
	[Order( MATH_DEBUG_ORDER )]
	[Feature( MATH ), Group( DEBUG )]
	public bool DebugLogMath { get; set; } = false;

	/// <summary>
	/// The component(s) with a logical value to affect with our math.
	/// <br /> <br />
	/// <b> LOGIC: </b> Looks for <see cref="ILogicValue"/>.
	/// That's what you should implement on your component
	/// for math operations on it to be supported.
	/// </summary>
	[Property]
	[WideMode( HasLabel = true )]
	[Order( MATH_LOGIC_ORDER + 1 )]
	[Feature( MATH ), Group( LOGIC )]
	public virtual List<ILogicValue> Targets { get; set; } = [null];

	/// <summary>
	/// The operations to be performed on the target component(s).
	/// </summary>
	[Property]
	[WideMode( HasLabel = true )]
	[InlineEditor( Label = true )]
	[Order( MATH_LOGIC_ORDER + 1 )]
	[Feature( MATH ), Group( LOGIC )]
	public List<MathOperation> Operations { get; set; } = [new()];

	public virtual bool TryOperate( float? value = null )
	{
		if ( Targets is null || Targets.Count <= 0 )
			return false;

		if ( Operations is null || Operations.Count <= 0 )
			return false;

		int opCount = 0;

		foreach ( var tgt in Targets )
		{
			if ( tgt is not Component c )
				continue;

			if ( c is null || c.GameObject.IsDestroyed() )
				continue;

			if ( c is not ILogicValue lv )
				continue;

			if ( TryOperate( lv, value ) )
				opCount++;
		}

		if ( opCount <= 0 )
			return false;

		if ( DebugLogMath )
			this.Log( $"Executed {opCount} operations successfully." );

		return true;
	}

	protected virtual bool TryOperate( ILogicValue lv, in float? value = null )
	{
		if ( lv?.Value is not float targetValue )
			return false;

		// Allows writing the previous value somewhere.
		// Multiple operations allow easily fine tuning the result.
		foreach ( var op in Operations )
		{
			targetValue = op.Operate( targetValue, out var opVal, in value );

			if ( DebugLogMath )
				this.Log( $"Operation: {targetValue} {op.Operation.String()} {opVal} -> {targetValue}" );
		}

		// Allows writing the result of the math.
		LogicAction.TryExecute( PostOperationLogic, this, targetValue );

		// Success/failure callbacks for logic and/or effects.
		if ( lv.TrySetValue( targetValue, out var result ) )
		{
			LogicAction.TryExecute( OnSuccessLogic, this, result );
			return true;
		}
		else
		{
			LogicAction.TryExecute( OnFailureLogic, this, result );
			return false;
		}
	}
}
