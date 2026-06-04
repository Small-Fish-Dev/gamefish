namespace GameFish;

/// <summary>
/// The support ways that you can compare numbers.
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
	/// One number is less than the other.
	/// </summary>
	[Title( "<" )]
	Lesser,

	/// <summary>
	/// One number is less than or equal to the other.
	/// </summary>
	[Title( "<=" )]
	LesserEqual,

	/// <summary>
	/// One number is greater than the other.
	/// </summary>
	[Title( ">" )]
	Greater,

	/// <summary>
	/// One number is greater than or equal to the other.
	/// </summary>
	[Title( ">=" )]
	GreaterEqual,
}
