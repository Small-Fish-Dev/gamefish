using GameFish;

namespace Playground;

/// <summary>
/// Allows for the editing of what's around you.
/// </summary>
[Group( NAME )]
[Icon( "edit_note" )]
public partial class Editor : Singleton<Editor>
{
	protected const int EDITOR_ORDER = DEFAULT_ORDER - 1000;

	protected const int TOOL_ORDER = EDITOR_ORDER + 10;
	protected const int PREFABS_ORDER = EDITOR_ORDER + 50;
	protected const int TRACING_ORDER = EDITOR_ORDER + 100;

	/*
	[Property, InlineEditor]
	[Title( "Draw Entity Boxes" )]
	[Feature( EDITOR ), Group( DEBUG )]
	public bool DrawEntityBounds { get; set; }
	*/

	protected override void OnUpdate()
	{
		base.OnUpdate();

		// There can only be one.. (active, at a time)
		if ( TryGetInstance( out var e ) && e != this )
			return;

		UpdateUI();

		UpdateGrid( Time.Delta );

		SimulateTool( Time.Delta, isFixedUpdate: false );

		UpdateTargetOutline();

		RenderNearbyHelpers();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		// There can only be one.. (active, at a time)
		if ( TryGetInstance( out var e ) && e != this )
			return;

		SimulateTool( Time.Delta, isFixedUpdate: true );
	}
}
