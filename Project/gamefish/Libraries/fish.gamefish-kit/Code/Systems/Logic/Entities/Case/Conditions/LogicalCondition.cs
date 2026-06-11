namespace GameFish;

/// <summary>
/// Something you check first to see if you should execute some logic.
/// </summary>
[Icon( "checklist" )]
public struct LogicalCondition
{
	[Title( "Type" )]
	public LogicValueType ValueType { get; set; }

	/// <summary>
	/// The number to make the comparison with.
	/// </summary>
	[Title( "Value" )]
	[ShowIf( nameof( ValueType ), LogicValueType.Number )]
	public float NumberValue { get; set; } = 1f;

	[EnumButtonGroup]
	[Title( "State" )]
	[WideMode( HasLabel = true )]
	[ShowIf( nameof( ValueType ), LogicValueType.Toggle )]
	public ToggleCommand ToggleValue { get; set; } = ToggleCommand.Disable;

	[Title( "True" )]
	[ShowIf( nameof( ValueType ), LogicValueType.Boolean )]
	public bool BooleanValue { get; set; } = true;

	[Title( "String" )]
	[ShowIf( nameof( ValueType ), LogicValueType.String )]
	public string StringValue { get; set; } = "hi";

	/// <summary>
	/// The different ways you can compare numbers.
	/// </summary>
	[EnumButtonGroup]
	[Title( "Comparison" )]
	[WideMode( HasLabel = true )]
	public NumberComparison Comparison { get; set; }

	public LogicalCondition() { }

	public readonly bool Matches( object value )
		=> ValueType.Compare( GetValue(), value, Comparison );

	public readonly object GetValue()
	{
		return ValueType switch
		{
			LogicValueType.Number => NumberValue,
			LogicValueType.Boolean => BooleanValue,
			LogicValueType.String => StringValue,
			LogicValueType.Toggle => ToggleValue,
			_ => null
		};
	}
}
