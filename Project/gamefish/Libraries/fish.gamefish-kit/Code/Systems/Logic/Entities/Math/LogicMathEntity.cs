namespace GameFish;

/// <summary>
/// Performs calculations on logical values.
/// <br /> <br />
/// <b> NOTE: </b> Pairs very well with <see cref="LogicCounterEntity"/>).
/// <br /> <br />
/// <b> NOTE: </b> Also works with any component implementing <see cref="ILogicValue"/>).
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
	[InlineEditor]
	[Title( "Logging (math)" )]
	[Order( MATH_DEBUG_ORDER )]
	[Feature( MATH ), Group( DEBUG )]
	public bool DebugLogMath { get; set; } = false;

	/// <summary>
	/// The component with the value to affect with our math.
	/// </summary>
	[Property]
	[InlineEditor]
	[Feature( MATH )]
	[Order( MATH_ORDER )]
	public virtual ILogicValue Target { get; set; }

	/// <summary>
	/// Operates on the target value using this value if none other
	/// is specified(such as through activating this with a number).
	/// </summary>
	[Property]
	[InlineEditor]
	[Feature( MATH )]
	[Title( "Value" )]
	[Order( MATH_ORDER )]
	public virtual float DefaultValue { get; set; } = 1f;

	/// <summary>
	/// Decides what to do with the target's value.
	/// </summary>
	[Property]
	[InlineEditor]
	[Feature( MATH )]
	[Order( MATH_ORDER )]
	[WideMode( HasLabel = true ), EnumButtonGroup]
	public virtual NumberOperation Operation { get; set; } = NumberOperation.Add;

	public virtual bool TryOperate( in float value )
	{
		if ( Target is not Component c )
			return false;

		if ( c is null || c.GameObject.IsDestroyed() )
			return false;

		if ( c is not ILogicValue lv )
			return false;

		return TryOperate( lv, value );
	}

	public virtual bool TryOperate( ILogicValue lv, in float value )
		=> TryOperate( lv, Operation, in value );

	protected virtual bool TryOperate( ILogicValue lv, in NumberOperation op, in float value )
	{
		if ( op is NumberOperation.None )
			return false;

		if ( lv?.Value is not float fValue )
			return false;

		if ( DebugLogMath )
			this.Log( $"performing {op} with value:[{value}] on target:[{lv}]" );

		// Allows writing the previous value somewhere.
		LogicAction.TryExecute( PreOperationLogic, this, value );

		// Do the actual math.
		fValue = fValue.Operate( value, op );

		// Allows writing the result of the math.
		LogicAction.TryExecute( PostOperationLogic, this, fValue );

		// Success/failure callbacks for logic and/or effects.
		if ( lv.TrySetValue( fValue, out var result ) )
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
