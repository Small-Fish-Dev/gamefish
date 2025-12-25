using System.Text.Json.Serialization;

namespace Playground;

public partial class EditorEntity : ModuleEntity
{
	protected const int EDITOR_ORDER = DEFAULT_ORDER - 1000;

	[Title( "Owner" )]
	[Property, JsonIgnore, ReadOnly]
	[Feature( EDITOR ), Group( ID ), Order( EDITOR_ORDER - 100 )]
	public SteamId InspectorOwner
	{
		get => Owner;
		set => Owner = value;
	}

	/// <summary>
	/// The client this entity belongs to.
	/// Probably the one that spawn/requested it.
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	public SteamId Owner { get; protected set; }
}
