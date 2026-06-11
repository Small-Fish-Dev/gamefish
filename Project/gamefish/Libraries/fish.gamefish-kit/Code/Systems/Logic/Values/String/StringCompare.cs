using System;
using System.Text.RegularExpressions;

namespace GameFish;

/// <summary>
/// The supported ways that you can compare strings.
/// </summary>
[Group( Library.NAME )]
[DefaultValue( Contains )]
public enum StringCompare
{
	/// <summary>
	/// The two strings must be the exact same letter for letter.
	/// </summary>
	Exact = 0,

	/// <summary>
	/// The strings must be the same(except upper/lowercase).
	/// </summary>
	Caseless = 1,

	/// <summary>
	/// The string contains the other string(ignoring case).
	/// </summary>
	Contains = 2,

	/// <summary>
	/// Looks for wildcards(<c>*</c>) to easily find partial matches.
	/// </summary>
	Partial = 3,

	/// <summary>
	/// Evaluates the string using a <b>regular expression</b>(regexp).
	/// <br /> <br />
	/// <b> NOTE: </b> Uses symbols as filtering instructions.
	/// Search "how to use regexp" online 
	/// </summary>
	Expression = 4,
}

partial class Library
{
	public static bool Matches( this string input, in string pattern, in StringCompare c )
	{
		if ( input is null || pattern is null )
			return false;

		var fResult = c switch
		{
			StringCompare.Exact => input.Equals( pattern, StringComparison.InvariantCulture ),
			StringCompare.Caseless => input.Equals( pattern, StringComparison.InvariantCultureIgnoreCase ),
			StringCompare.Contains => input.Contains( pattern, StringComparison.InvariantCultureIgnoreCase ),
			StringCompare.Partial => Regex.IsMatch( input, Regex.Escape( pattern ).Replace( "\\*", ".*" ), RegexOptions.None, TimeSpan.FromSeconds( 0.1f ) ),
			StringCompare.Expression => Regex.IsMatch( input, pattern, RegexOptions.None, TimeSpan.FromSeconds( 0.1f ) ),
			_ => false,
		};

		return fResult;
	}

	public static bool Matches( this string input, in StringMatch match )
		=> Matches( input, match.Pattern, match.Comparison );
}
