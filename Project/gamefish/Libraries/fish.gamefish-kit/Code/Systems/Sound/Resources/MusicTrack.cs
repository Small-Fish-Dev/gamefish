using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Used to identify and store information about a team.
/// </summary>
[Icon( "music_note" )]
[AssetType( Name = "Music Track", Extension = "mutrack", Category = Library.NAME )]
public partial class MusicTrack : GameResource
{
	protected const int ORDER_TRACK = 10;
	protected const int ORDER_DISPLAY = 20;

	protected override Bitmap CreateAssetTypeIcon( int width, int height )
		=> CreateSimpleAssetTypeIcon( "music_note", width, height, Color.Parse( "#338AB3" ), Color.White );

	/// <summary>
	/// The music's source file.
	/// </summary>
	[Group( TRACK ), Order( ORDER_TRACK )]
	public SoundFile File { get; set; }

	/// <summary>
	/// The volume scale.
	/// </summary>
	[Group( TRACK ), Order( ORDER_TRACK )]
	[Range( 0f, 2f, clamped: false ), Step( 0.01f )]
	public float Volume { get; set; } = 1f;

	[Group( DISPLAY ), Order( ORDER_DISPLAY )]
	public string Name { get; set; }

	[Group( DISPLAY ), Order( ORDER_DISPLAY )]
	public string Author { get; set; }

	public virtual bool TryPreload()
	{
		if ( !File.IsValid() )
			return false;

		if ( !File.IsLoaded )
			File.Preload();

		return true;
	}
}
