namespace GameFish;

[Icon( "checklist" )]
public struct LogicalCase
{
	[Title( "Type" )]
	public LogicValueType ValueType { get; set; }

	/// <summary>
	/// The number to make the comparison with.
	/// </summary>
	[Title( "Value" )]
	[ShowIf( nameof( ValueType ), LogicValueType.Number )]
	public float NumberValue { get; set; } = 1f;

	/// <summary>
	/// The different ways you can compare numbers.
	/// </summary>
	[EnumButtonGroup]
	[Title( "Comparison" )]
	[WideMode( HasLabel = true )]
	public NumberComparison Comparison { get; set; }

	[Title( "Value" )]
	[ShowIf( nameof( ValueType ), LogicValueType.Boolean )]
	public bool BooleanValue { get; set; } = true;

	[Title( "Value" )]
	[ShowIf( nameof( ValueType ), LogicValueType.String )]
	public string StringValue { get; set; } = "hi";

	[Title( "Value" )]
	[WideMode( HasLabel = false ), EnumButtonGroup]
	[ShowIf( nameof( ValueType ), LogicValueType.Toggle )]
	public ToggleCommand ToggleValue { get; set; } = ToggleCommand.Disable;

	public LogicalCase() { }

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
