using System;

namespace GameFish;

/// <summary>
/// Types of operations you can perform with two numbers.
/// </summary>
[DefaultValue( Add )]
public enum NumberOperation
{
	/// <summary>
	/// No operation.
	/// </summary>
	None,

	/// <summary>
	/// Assigns the value directly.
	/// </summary>
	[Title( "=" )]
	// [Icon( "edit" )]
	Set,

	/// <summary>
	/// Increases one number by the other.
	/// </summary>
	[Title( "+=" )]
	// [Icon( "add" )]
	Add,

	/// <summary>
	/// Decreases one number by the other.
	/// </summary>
	[Title( "-=" )]
	// [Icon( "remove" )]
	Subtract,

	/// <summary>
	/// Scales a number by the other.
	/// </summary>
	[Title( "*=" )]
	// [Icon( "clear" )]
	Multiply,

	/// <summary>
	/// Makes a number a dividend of the other.
	/// <br />
	/// <b> EXAMPLE: </b> <c> 10 / 5 = 2 </c>
	/// </summary>
	[Title( "/=" )]
	// [Icon( "border_all" )]
	Divide,

	/// <summary>
	/// Multiplies the number by the specified power.
	/// <br />
	/// <b> EXAMPLE: </b> <c> 5 ^ 3 = 125 </c>
	/// </summary>
	[Title( "^=" )]
	// [Icon( "superscript" )]
	Exponent,

	/// <summary>
	/// Finds the remainder of division.
	/// <br />
	/// <b> EXAMPLE: </b> <c> 20.5 % 8 = 4.5 </c>
	/// </summary>
	[Title( "%=" )]
	// [Icon( "percent" )]
	Modulo,

	/// <summary>
	/// Results in the lesser number.
	/// <br />
	/// <b> EXAMPLE: </b> <c>6</c> is less than <c>9</c>, so the result is <c>6</c>.
	/// </summary>
	[Title( "Min" )]
	// [Icon( "chevron_left" )]
	Min,

	/// <summary>
	/// Results in the greater number.
	/// <br />
	/// <b> EXAMPLE: </b> <c>400</c> is greater than <c>20</c>, so the result is <c>400</c>.
	/// </summary>
	[Title( "Max" )]
	// [Icon( "chevron_right" )]
	Max,
}

partial class Library
{
	/// <inheritdoc cref="OperateFloat" />
	public static float Operate( this float a, in float b, in NumberOperation op )
		=> OperateFloat( in a, in b, in op );

	/// <inheritdoc cref="OperateInteger" />
	public static int Operate( this int a, in float b, in NumberOperation op, in NumberRounding r = NumberRounding.Floor )
		=> OperateInteger( a, in b, in op, in r );

	/// <inheritdoc cref="OperateDouble" />
	public static double Operate( this double a, in double b, in NumberOperation op )
		=> OperateDouble( a, in b, in op );

	/// <summary>
	/// Performs the specified operation on an integer and then rounds the result.
	/// </summary>
	/// <param name="a"> The value to peform the operation on. </param>
	/// <param name="b"> The number to operate with. </param>
	/// <param name="op"> The operation to use. </param>
	/// <param name="r"> The rounding method to apply after the operation(to keep it an integer). </param>
	/// <returns> The result of the operation(if any). </returns>
	public static int OperateInteger( in int a, in float b, in NumberOperation op, in NumberRounding r = NumberRounding.Floor )
		=> OperateFloat( a, in b, in op ).RoundInt( r );

	/// <summary>
	/// Performs the specified operation on one number using another.
	/// </summary>
	/// <param name="a"> The value to peform the operation on. </param>
	/// <param name="b"> The number to operate with. </param>
	/// <param name="op"> The operation to use. </param>
	/// <returns> The result of the operation(if any). </returns>
	public static float OperateFloat( in float a, in float b, in NumberOperation op )
	{
		return op switch
		{
			NumberOperation.Set => b,
			NumberOperation.Add => a + b,
			NumberOperation.Subtract => a - b,
			NumberOperation.Multiply => a * b,
			NumberOperation.Divide => a / b,
			NumberOperation.Exponent => a.Pow( b ),
			NumberOperation.Modulo => a % b,
			NumberOperation.Min => (a < b) ? a : b,
			NumberOperation.Max => (a > b) ? a : b,
			_ => a,
		};
	}

	/// <summary>
	/// Performs the specified operation on one number using another.
	/// </summary>
	/// <param name="a"> The value to peform the operation on. </param>
	/// <param name="b"> The number to operate with. </param>
	/// <param name="op"> The operation to use. </param>
	/// <returns> The result of the operation(if any). </returns>
	public static double OperateDouble( in double a, in double b, in NumberOperation op )
	{
		return op switch
		{
			NumberOperation.Set => b,
			NumberOperation.Add => a + b,
			NumberOperation.Subtract => a - b,
			NumberOperation.Multiply => a * b,
			NumberOperation.Divide => a / b,
			NumberOperation.Exponent => Math.Pow( a, b ),
			NumberOperation.Modulo => a % b,
			NumberOperation.Min => (a < b) ? a : b,
			NumberOperation.Max => (a > b) ? a : b,
			_ => a,
		};
	}
}
