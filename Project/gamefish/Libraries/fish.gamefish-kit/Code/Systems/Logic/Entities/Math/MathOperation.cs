namespace GameFish;

public struct MathOperation
{
	/// <summary>
	/// The value to operate with if none is specified by input.
	/// </summary>
	[KeyProperty]
	[WideMode( HasLabel = true )]
	[HideIf( nameof( Input ), LogicInputHandling.Require )]
	public float Value { get; set; } = 1f;

	/// <summary>
	/// The different supported types of number math.
	/// </summary>
	[KeyProperty]
	[EnumButtonGroup]
	[WideMode( HasLabel = true )]
	public NumberOperation Operation { get; set; } = NumberOperation.Add;

	/// <summary>
	/// Determines if inputs(numbers passed in) are used, ignored or required.
	/// </summary>
	[KeyProperty]
	[WideMode( HasLabel = true )]
	public LogicInputHandling Input { get; set; } = LogicInputHandling.Ignore;

	public MathOperation() { }

	public MathOperation( in float value )
	{
		Value = value;
	}

	public MathOperation( in float value, NumberOperation op )
	{
		Value = value;
		Operation = op;
	}

	/// <summary>
	/// Operates on the target value with an optional input value
	/// to get the result of and value used for the operation.
	/// </summary>
	/// <param name="targetValue"> The value to perform the operation on. </param>
	/// <param name="opValue"> The value this decided to operate with. </param>
	/// <param name="input"> The (optional) input value. </param>
	/// <returns> The result of the operation. </returns>
	public readonly float Operate( in float targetValue, out float opValue, in float? input = null )
	{
		if ( Input is LogicInputHandling.Prefer )
		{
			// Use the input value if it's there.
			opValue = input ?? Value;

			return targetValue.Operate( opValue, Operation );
		}
		else if ( Input is LogicInputHandling.Ignore )
		{
			// Ignoring the input value.
			opValue = Value;

			return targetValue.Operate( Value, Operation );
		}
		else if ( Input is LogicInputHandling.Require )
		{
			if ( input is float fInput )
			{
				opValue = fInput;
				return targetValue.Operate( fInput, Operation );
			}
		}

		// Bad config or no input when required.
		opValue = 0;
		return targetValue;
	}
}
