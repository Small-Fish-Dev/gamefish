namespace GameFish;

/// <summary>
/// Types of number rounding operations.
/// </summary>
[DefaultValue( Round )]
public enum NumberRounding
{
	/// <summary>
	/// No rounding. Gives you the same number.
	/// </summary>
	None,

	/// <summary>
	/// Raises or lowers it to the nearest whole number.
	/// </summary>
	Round,

	/// <summary>
	/// Shaves off any decimals so that it's a whole number.
	/// </summary>
	Floor,

	/// <summary>
	/// Raises it up to the next highest whole number.
	/// </summary>
	Ceil,
}

partial class Library
{
	/// <summary>
	/// Rounds a number using a <see cref="NumberRounding"/> type.
	/// </summary>
	/// <param name="value"> The value to round. </param>
	/// <param name="method"> The rounding method to use. </param>
	/// <returns> The result of the rounding operation(if any). </returns>
	public static float Round( this float value, in NumberRounding method )
	{
		var fResult = method switch
		{
			NumberRounding.Round => value.Round(),
			NumberRounding.Floor => value.Floor(),
			NumberRounding.Ceil => value.CeilToInt(),
			_ => value,
		};

		return fResult;
	}

	/// <summary>
	/// Rounds a number to an integer using a <see cref="NumberRounding"/> type.
	/// </summary>
	/// <param name="value"> The value to round. </param>
	/// <param name="method"> The rounding method to use. </param>
	/// <returns> The result of the rounding operation(if any) as an integer. </returns>
	public static int RoundInt( this float value, in NumberRounding method )
	{
		var fResult = method switch
		{
			NumberRounding.Round => value.Round(),
			NumberRounding.Floor => value.Floor(),
			NumberRounding.Ceil => value.CeilToInt(),
			_ => value,
		};

		return (int)fResult;
	}
}
