namespace GameFish.Razor;

/// <summary>
/// The HTML element type.
/// </summary>
[DefaultValue( Paragraph )]
public enum DisplayElement : int
{
	/// <summary>
	/// <see cref="Paragraph"/>
	/// </summary>
	Default = 0,

	Title,

	Heading1,
	Heading2,
	Heading3,
	Heading4,
	Heading5,
	Heading6,

	Paragraph,

	SuperScript,
	SubScript,

	Footer,
}

