namespace GameFish;

/// <summary>
/// An inspector-friendly string comparison/search helper.
/// </summary>
[Icon( "🧵" )]
[Group( Library.GROUP_LOGIC )]
public struct StringMatch : IValid
{
	/// <summary>
	/// The string to compare with target string(s).
	/// </summary>
	[KeyProperty]
	public string Pattern { get; set; }

	/// <summary>
	/// The way we'll compare the pattern with the target string(s).
	/// </summary>
	[KeyProperty]
	public StringCompare Comparison { get; set; } = StringCompare.Contains;

	/// <returns> If the pattern is defined and not blank. </returns>
	public readonly bool IsValid => !Pattern.IsBlank();

	public StringMatch() { }

	public StringMatch( string pattern )
	{
		Pattern = pattern;
	}

	public StringMatch( string pattern, StringCompare c )
	{
		Pattern = pattern;
		Comparison = c;
	}

	public readonly bool Matches( string input )
		=> input?.Matches( in this ) is true;
}
