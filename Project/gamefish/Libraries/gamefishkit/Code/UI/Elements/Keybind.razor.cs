using System;
using GameFish;

namespace GameFish.Razor;

/// <summary>
/// An image for a key/button you can press.
/// </summary>
partial class Keybind
{
	[Parameter]
	public string Action
	{
		get => _action;
		set
		{
			if ( _action == value )
				return;

			_action = value;
			Update();
		}
	}

	protected string _action;


	/// <summary>
	/// The text to display alongside the bind.
	/// </summary>
	[Parameter]
	public string Text { get; set; }


	[Parameter]
	public InputGlyphSize Size { get; set; }

	[Parameter]
	public bool Outline { get; set; } = true;

	public Texture Texture
	{
		get => _texture ??= GetTexture();
		set => _texture = value ?? GetTexture();
	}

	protected Texture _texture;

	protected override void OnParametersSet()
	{
		base.OnParametersSet();

		Update();
	}

	public virtual void Update()
		=> Set( Action, Text, Size, Outline );

	public virtual void Set( string action, string text = null, in InputGlyphSize size = InputGlyphSize.Small, in bool outline = true )
	{
		Text = text;

		Size = size;
		Action = action;
		Outline = outline;

		Texture = GetTexture();
	}

	public virtual Texture GetTexture()
		=> Input.GetGlyph( Action, Size, Outline );

	protected override int BuildHash()
		=> HashCode.Combine( Action, Text, Size, Size, Outline, Texture );
}
