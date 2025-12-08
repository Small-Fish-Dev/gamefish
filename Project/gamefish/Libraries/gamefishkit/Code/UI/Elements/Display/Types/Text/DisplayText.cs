using System;
using GameFish.Razor;
using Sandbox;

namespace GameFish.Razor;

/// <summary>
/// Tells the UI/HUD how to display a line of text.
/// <br /> <br />
/// <b> TIP: </b> Because this is a <c>struct</c> it can be networked directly,
/// such as within a <see cref="NetList{DisplayLine}"/>.
/// </summary>
public struct DisplayText : IValid
{
	[Hide]
	public readonly bool IsValid => !Text.IsBlank();

	/// <summary>
	/// Tells you if it's a title, heading or paragraph etc.
	/// </summary>
	[KeyProperty]
	public DisplayElement Element { get; set; } = DisplayElement.Paragraph;

	/// <summary>
	/// What's to be written.
	/// </summary>
	[KeyProperty]
	public string Text { get; set; }

	/// <summary>
	/// The text font family override.
	/// </summary>
	[FontName]
	public string Font { get; set; }

	/// <summary>
	/// The text weight(boldness) override.
	/// </summary>
	[Step( 1 )]
	[Range( 100, 1000, clamped: false )]
	public int? Weight { get; set; }

	/// <summary>
	/// The text size override.
	/// </summary>
	[Range( 1, 128, clamped: false )]
	public int? Size { get; set; }

	/// <summary>
	/// The text color override.
	/// </summary>
	[ColorUsage( IsHDR = false )]
	[DefaultValue( "#000000" )]
	public Color? Color { get; set; }

	/// <summary>
	/// Creates a blank, default(invalid) line of text.
	/// </summary>
	public DisplayText() { }

	/// <summary>
	/// Creates text display data from a string with no overrides.
	/// </summary>
	public DisplayText( string text )
	{
		Text = text;
	}

	/// <summary>
	/// Creates text display data with a variety of override options.
	/// </summary>
	public DisplayText( string text, DisplayElement? e = null, string font = null, int? weight = null, int? size = null, Color? color = null )
	{
		Text = text;

		// An invalid/unspecified value is not considered.
		if ( e.HasValue && e.Value > DisplayElement.Default )
			Element = e.Value;

		Font = font;
		Weight = weight;
		Size = size;
		Color = color;
	}

	public DisplayText WithText( string text )
		=> this with { Text = text };
}
