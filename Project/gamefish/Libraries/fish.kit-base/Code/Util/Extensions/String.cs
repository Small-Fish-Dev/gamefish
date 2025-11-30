using System;

namespace GameFish;

partial class Library
{
	/// <summary>
	/// Tells if you if the string is null or has literally nothing in it(not even spaces).
	/// </summary>
	/// <returns> <see cref="string.IsNullOrEmpty(string?)"/> </returns>
	public static bool IsEmpty( this string str )
		=> string.IsNullOrEmpty( str );

	/// <summary>
	/// Tells you if the string is null, literally empty or made only of spaces.
	/// </summary>
	/// <returns> <see cref="string.IsNullOrWhiteSpace(string?)"/> </returns>
	public static bool IsBlank( this string str )
		=> string.IsNullOrWhiteSpace( str );

	/// <summary>
	/// Lets you specify a different string if this one is null or literally empty(no spaces).
	/// </summary>
	/// <returns> Either <paramref name="str"/>(if not null or empty) or <paramref name="default"/>. </returns>
	public static string DefaultEmpty( this string str, in string @default )
		=> str.IsEmpty() ? @default : str;

	/// <summary>
	/// Lets you specify a different string if this one is null, literally empty or only made of spaces.
	/// </summary>
	/// <returns> Either <paramref name="str"/>(if not null, empty or whitespace) or <paramref name="default"/>. </returns>
	public static string DefaultBlank( this string str, in string @default )
		=> str.IsBlank() ? @default : str;

	/// <summary>
	/// Takes a set of strings and turns them into one string(without a separator).
	/// </summary>
	/// <returns> A single string made by combining a set of strings. </returns>
	public static string Implode( this IEnumerable<string> pieces )
		=> string.Concat( pieces ?? [] );

	/// <summary>
	/// Takes a set of strings and turns them into one string with a separator between them.
	/// </summary>
	/// <returns> A single string made by combining a set of strings with <paramref name="separator"/> between them. </returns>
	public static string Implode( this IEnumerable<string> pieces, string separator )
		=> string.Join( separator ?? "", pieces ?? [] );

	/// <summary>
	/// Takes a single string and breaks it up into pieces where it matches the separator.
	/// </summary>
	/// <returns> A set of strings made by splitting a string with <paramref name="separator"/> between them. </returns>
	public static string[] Explode( this string str, string separator = "," )
		=> str?.Split( separator ?? "," ) ?? [];
}
