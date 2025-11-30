using System;
using System.Text;
using GameFish;

namespace GameFish.Razor;

/// <summary>
/// Displays text using <see cref="DisplayText"/> data.
/// </summary>
partial class TextLine
{
	[Parameter]
	public DisplayText Line
	{
		get => _line;
		set
		{
			OnSetLine( ref value );
			_line = value;
		}
	}

	protected DisplayText _line;

	/// <summary>
	/// Scales text font size of the title/headings/content etc.
	/// <br /> <br />
	/// <b> NOTE: </b> A value of <c>1.0</c> is equal to <c>100%</c> size.
	/// </summary>
	[Parameter]
	public float Scale { get; set; } = 1f;

	public string LineStyle { get; protected set; }

	protected virtual void OnSetLine( ref DisplayText line )
	{
	}

	protected override void OnParametersSet()
	{
		base.OnParametersSet();

		LineStyle = GetLineStyle( Line.Element );
	}

	protected virtual string GetLineStyle( in DisplayElement e )
	{
		var str = new StringBuilder();

		if ( GetTextSize( in e ) is int size )
			str.Append( $"font-size: {size}px;" );

		if ( GetTextFont( in e ) is string font && !font.IsBlank() )
			str.Append( $"font-family: \"{font}\";" );

		if ( GetTextWeight( in e ) is int weight )
			str.Append( $"font-weight: {weight}px;" );

		if ( GetTextColor( in e ) is Color c )
			str.Append( $"color: {c.Hex};" );

		return str.ToString() ?? "";
	}

	protected virtual int? GetTextSize( in DisplayElement e )
	{
		var size = Line.Size ?? GetElementSize( e );

		if ( !size.HasValue )
			return null;

		// Apply scale.
		return (size.Value * Scale).CeilToInt();
	}

	protected virtual int? GetTextWeight( in DisplayElement e )
		=> Line.Weight;

	protected virtual Color? GetTextColor( in DisplayElement e )
		=> Line.Color;

	protected virtual string GetTextFont( in DisplayElement e )
		=> Line.Font;

	/// <returns> The default unscaled size of an element. </returns>
	protected virtual int? GetElementSize( in DisplayElement e )
	{
		return e switch
		{
			DisplayElement.Title => 64,
			DisplayElement.Heading1 => 48,
			DisplayElement.Heading2 => 32,
			DisplayElement.Heading3 => 24,
			DisplayElement.Heading4 => 20,
			DisplayElement.Heading5 => 16,
			DisplayElement.Heading6 => 12,
			DisplayElement.Paragraph => 18,
			DisplayElement.SuperScript => 18,
			DisplayElement.SubScript => 18,
			DisplayElement.Footer => 14,
			_ => null,
		};
	}

	protected override int BuildHash()
		=> HashCode.Combine( Line, LineStyle );
}
