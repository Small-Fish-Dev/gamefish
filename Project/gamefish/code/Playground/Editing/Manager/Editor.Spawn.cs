namespace Fishbox;

partial class Editor
{
	/// <summary>
	/// Entities can be placed within this to snap them together.
	/// </summary>
	[Property]
	[Title( "Island" )]
	[Feature( EDITOR ), Group( PREFABS ), Order( PREFABS_ORDER - 1 )]
	public PrefabFile IslandPrefab { get; set; }

	public static EditorIsland FindIsland( GameObject obj )
	{
		if ( !obj.IsValid() )
			return null;

		return obj.Components?.Get<EditorIsland>( FindMode.EnabledInSelf | FindMode.InAncestors );
	}

	public static bool TryFindIsland( GameObject obj, out EditorIsland island )
		=> (island = FindIsland( obj )).IsValid();
}
