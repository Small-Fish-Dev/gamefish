namespace GameFish;

/// <summary>
/// A sound file with a volume and support for start/end times.
/// </summary>
[GameResource( "Music File", extension: "mufile", description: "A sound file with a volume and start/end times." )]
public class MusicFile : GameResource
{
	public const string GROUP_RANGE = "⏱ Range";

	public SoundFile SoundFile { get; set; }

	/// <summary>
	/// The default volume.
	/// </summary>
	public Fraction Volume { get; set; }

	/// <summary>
	/// Indicates that the range was configured(hopefully correctly).
	/// </summary>
	[Group( GROUP_RANGE )]
	public bool HasAudibleRange => AudibleRange.Min >= 0f && AudibleRange.Max > 0f;

	/// <summary>
	/// The start/end time in seconds when this is audible. <br />
	/// Used in cases where music needs to line up. <br />
	/// If the max is zero or less then the sound's duration is used.
	/// </summary>
	[Group( GROUP_RANGE )]
	public FloatRange AudibleRange { get; set; } = new( 0f, 0f );
}
