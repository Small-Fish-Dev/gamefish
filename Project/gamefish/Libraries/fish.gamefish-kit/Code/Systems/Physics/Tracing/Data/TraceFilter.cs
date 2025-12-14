using Sandbox;

namespace GameFish;

/// <summary>
/// Filter settings used when tracing. <br />
/// With thse files you don't have to re-configure every single
/// prefab manually whenever you want to make a related change.
/// </summary>
[AssetType( Name = "Trace Filter", Extension = "trfilter", Category = Library.NAME )]
public partial class TraceFilter : GameResource
{
	protected override Bitmap CreateAssetTypeIcon( int width, int height )
		=> CreateSimpleAssetTypeIcon( "airline_stops", width, height, Color.White, Color.Black );

	/// <summary>
	/// If enabled: trace against hitboxes.
	/// </summary>
	[Title( "Hitboxes" )]
	[Group( TAGS, StartFolded = true )]
	public bool UseHitboxes { get; set; }

	/// <summary>
	/// If enabled: if an object has ANY <b>ignore</b> tags then it must have ANY <b>hit</b> tags.
	/// </summary>
	[Title( "Whitelist" )]
	[Group( TAGS, StartFolded = true )]
	public bool UseTagWhitelist { get; set; }

	/// <summary>
	/// If enabled: only consider objects with ANY of these tags.
	/// </summary>
	[Group( TAGS )]
	[Title( "Tags (hit)" )]
	public TagFilter TagsHit { get; set; } = new( false, [TAG_SOLID, TAG_PAWN] );

	/// <summary>
	/// If enabled: ignore objects with ANY of these tags.
	/// </summary>
	[Group( TAGS )]
	[Title( "Tags (ignore)" )]
	public TagFilter TagsIgnore { get; set; } = new( false, [TAG_TRIGGER] );

	/// <summary>
	/// If enabled: only trace against objects that have ALL of these tags.
	/// </summary>
	[Group( TAGS )]
	[Title( "Tags (require)" )]
	public TagFilter TagsRequire { get; set; } = new( false, [] );

	/// <summary>
	/// How should the trace treat triggers/solids?
	/// </summary>
	[Group( TRIGGERS )]
	[Title( "Inclusion" )]
	public TraceTriggerType TriggerType { get; set; }
}
