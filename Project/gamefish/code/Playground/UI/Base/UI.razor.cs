using System;
using System.Text.Json.Serialization;
using GameFish;

namespace Playground.Razor;

/// <summary>
/// This demo's main user interface.
/// </summary>
partial class UI
{
	public static UI Instance
	{
		get => _instance.GetSingleton();
		protected set => _instance = value;
	}

	[SkipHotload]
	private static UI _instance;

	public bool InGame => this.InGame();
	public bool InEditor => this.InEditor();

	public static bool InMainMenu => SceneSettings.Instance?.IsMainMenu is true;
	public static bool InMenu => InMainMenu || Instance?.IsMenuOpen is true;

	[Property, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( Strings.UI ), Group( DEBUG )]
	public bool IsMenuOpen { get; set; }

	protected override int BuildHash()
		=> HashCode.Combine( InMainMenu, IsMenuOpen, EditorMenu.IsOpen );
}
