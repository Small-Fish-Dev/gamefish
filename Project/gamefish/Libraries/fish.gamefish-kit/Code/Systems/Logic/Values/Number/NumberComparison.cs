namespace GameFish;

/// <summary>
/// The supported ways that you can compare numbers.
/// </summary>
[Group( Library.NAME )]
[DefaultValue( Equal )]
public enum NumberComparison
{
	/// <summary>
	/// The values are the same.
	/// </summary>
	[Title( "==" )]
	Equal,

	/// <summary>
	/// The number is less than the other.
	/// </summary>
	[Title( "<" )]
	Lesser,

	/// <summary>
	/// The number is less than or equal to the other.
	/// </summary>
	[Title( "<=" )]
	LesserEqual,

	/// <summary>
	/// The number is greater than the other.
	/// </summary>
	[Title( ">" )]
	Greater,

	/// <summary>
	/// The number is greater than or equal to the other.
	/// </summary>
	[Title( ">=" )]
	GreaterEqual,
}
