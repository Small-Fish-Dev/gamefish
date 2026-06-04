namespace GameFish;

/// <summary>
/// Types of operations you can perform with two colors.
/// </summary>
[DefaultValue( Set )]
public enum ColorOperation
{
	/// <summary>
	/// No operation.
	/// </summary>
	None,

	/// <summary>
	/// Sets the color directly.
	/// </summary>
	[Title( "=" )]
	Set,

	/// <summary>
	/// Increases one color by the other.
	/// </summary>
	[Title( "+=" )]
	Add,

	/// <summary>
	/// Decreases one color by the other.
	/// </summary>
	[Title( "-=" )]
	Subtract,

	/// <summary>
	/// Scales one color by the other.
	/// </summary>
	[Title( "*=" )]
	Multiply,
}

partial class Library
{
	/// <inheritdoc cref="OperateColor" />
	public static Color Operate( this Color a, in Color b, in ColorOperation op )
		=> OperateColor( in a, in b, in op );

	/// <summary>
	/// Performs the specified operation on one color using another.
	/// </summary>
	/// <param name="a"> The value to peform the operation on. </param>
	/// <param name="b"> The color to operate with. </param>
	/// <param name="op"> The operation to use. </param>
	/// <returns> The result of the operation(if any). </returns>
	public static Color OperateColor( in Color a, in Color b, in ColorOperation op )
	{
		return op switch
		{
			ColorOperation.Set => b,
			ColorOperation.Add => a + b,
			ColorOperation.Subtract => a - b,
			ColorOperation.Multiply => a * b,
			_ => a,
		};
	}
}
