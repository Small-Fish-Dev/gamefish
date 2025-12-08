namespace GameFish;

/// <summary>
/// A sound file with a volume and support for start/end times.
/// </summary>
[AssetType( Name = "Music File", Extension = "mufile", Category = "Game Fish" )]
public partial class MusicFile : GameResource
{
	protected const string RANGE = "⏱ Range";

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
		=> CreateSimpleAssetTypeIcon( "music_note", width, height, Color.Parse( "#338AB3" ), Color.White );

	/// <summary>
	/// The music's source file.
	/// </summary>
	[Group( SOUNDS )]
	public SoundFile SoundFile { get; set; }

	/// <summary>
	/// The volume override(if defined).
	/// </summary>
	[Group( SOUNDS )]
	[DefaultValue( 1f )]
	[Range( 0f, 2f, clamped: false ), Step( 0.01f )]
	public float? Volume { get; set; }

	/// <summary>
	/// Indicates that the range was configured.
	/// </summary>
	[Group( RANGE )]
	[Title( "Valid Range" )]
	public bool HasAudibleRange => AudibleRange.Min >= 0f && AudibleRange.Max > 0f;

	/// <summary>
	/// The start/end time in seconds when this is audible.
	/// Used in cases where music needs to line up. <br />
	/// If the max is zero or less then the sound's duration is used.
	/// </summary>
	[Group( RANGE )]
	public FloatRange AudibleRange { get; set; } = new( 0f, 0f );
}
