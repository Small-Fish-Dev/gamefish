using System;
using System.Text.Json.Serialization;
using Sandbox.Audio;

namespace GameFish;

/// <summary>
/// An entry in a <see cref="SoundTable"/> that lets
/// you select various types of sounds and pick them randomly.
/// </summary>
public class SoundEntry : IValid
{
	protected const int VOLUME_ORDER = 10;
	protected const int PITCH_ORDER = 20;
	protected const int MIXER_ORDER = 30;
	protected const int FALLOFF_ORDER = 40;
	protected const int FOLLOW_ORDER = 50;

	public const float VOLUME_MIN = 0.01f;
	public const float VOLUME_MAX = 3f;

	public const float PITCH_MIN = 0.05f;
	public const float PITCH_MAX = 10f;

	/// <summary>
	/// Indicates if the sound file for the chosen type is properly defined.
	/// </summary>
	[Hide, JsonIgnore]
	public bool IsValid => IsSoundEvent
		? SoundEvent.IsValid()
		: SoundFile.IsValid();

	/// <summary>
	/// Supported file types for sound selection.
	/// </summary>
	[Hide]
	[DefaultValue( SoundEvent )]
	public enum FileType
	{
		/// <summary>
		/// A <c>.sound</c> file.
		/// </summary>
		[Title( "Event" )]
		SoundEvent,

		/// <summary>
		/// A raw sound file such as <c>.mp3</c>, <c>.wav</c>, <c>.ogg</c> etc.
		/// </summary>
		[Title( "File" )]
		SoundFile,

		// TODO: SoundEffect file (.gfsfx)
	}

	/// <summary>
	/// The different properties of the sound that you can override.
	/// </summary>
	[Hide]
	[Flags]
	public enum Features
	{
		/// <summary>
		/// If enabled: override what the volume will be.
		/// </summary>
		[Icon( "🎚" )]
		Volume = 1 << 0,

		/// <summary>
		/// If enabled: override what the pitch will be.
		/// </summary>
		[Icon( "👂" )]
		Pitch = 1 << 1,

		/// <summary>
		/// If enabled: override the volume's falloff over distance.
		/// </summary>
		[Icon( "🗻" )]
		Falloff = 1 << 2,

		/// <summary>
		/// If enabled: specify the sound's mixer.
		/// </summary>
		[Icon( "🎛" )]
		Mixer = 1 << 3,
	}

	public FileType Type { get; set; }

	[Hide, JsonIgnore]
	public bool IsSoundFile => Type is FileType.SoundFile;

	[Hide, JsonIgnore]
	public bool IsSoundEvent => Type is FileType.SoundEvent;

	[Title( "Event" )]
	[ShowIf( nameof( IsSoundEvent ), true )]
	public SoundEvent SoundEvent { get; set; }

	[Title( "File" )]
	[ShowIf( nameof( IsSoundFile ), true )]
	public SoundFile SoundFile { get; set; }

	/// <summary>
	/// The different properties of the sound that you can override.
	/// </summary>
	[EnumButtonGroup]
	[WideMode( HasLabel = true )]
	public Features Overrides { get; set; }

	[Hide, JsonIgnore]
	public bool HasVolume => Overrides.HasFlag( Features.Volume );

	[Hide, JsonIgnore]
	public bool HasPitch => Overrides.HasFlag( Features.Pitch );

	[Hide, JsonIgnore]
	public bool HasFalloff => Overrides.HasFlag( Features.Falloff );

	[Hide, JsonIgnore]
	public bool HasMixer => Overrides.HasFlag( Features.Mixer );

	[Group( VOLUME )]
	[Order( VOLUME_ORDER )]
	[WideMode( HasLabel = false )]
	[Range( 0.1f, 2f, clamped: false )]
	[ShowIf( nameof( HasVolume ), true )]
	public RangedFloat Volume
	{
		get => _vol;
		set => _vol = value.Clamp( VOLUME_MIN, VOLUME_MAX );
	}

	[Hide, JsonIgnore]
	private RangedFloat _vol = 1f;

	[Group( PITCH )]
	[Order( PITCH_ORDER )]
	[WideMode( HasLabel = false )]
	[ShowIf( nameof( HasPitch ), true )]
	[Range( PITCH_MIN, PITCH_MAX, clamped: false )]
	public RangedFloat Pitch
	{
		get => _pitch;
		set => _pitch = value.Clamp( PITCH_MIN, PITCH_MAX );
	}

	[Hide, JsonIgnore]
	private RangedFloat _pitch = 1f;

	/// <summary>
	/// The maximum distance the sound can travel.
	/// <br /> <br />
	/// <b> NOTE: </b> Volume is scaled by the falloff curve.
	/// </summary>
	[Step( 1f )]
	[Group( FALLOFF )]
	[Title( "Distance" )]
	[Order( FALLOFF_ORDER )]
	[Range( 1000f, 15000f, clamped: false )]
	[ShowIf( nameof( HasFalloff ), true )]
	public float Distance { get; set; } = 5000f;

	/// <summary>
	/// The specific mixer to use for this sound.
	/// </summary>
	[Step( 1f )]
	[Group( MIXER )]
	[Order( MIXER_ORDER )]
	[WideMode( HasLabel = false )]
	[ShowIf( nameof( HasMixer ), true )]
	public MixerHandle Mixer { get; set; } = Sandbox.Audio.Mixer.Default;

	/// <summary>
	/// How to scale the volume by distance, from in your ear to far away.
	/// </summary>
	[Group( FALLOFF )]
	[Title( "Scaling" )]
	[Order( FALLOFF_ORDER )]
	[ShowIf( nameof( HasFalloff ), true )]
	public Curve FalloffCurve { get; set; }
		= new(
			// FYI: Values were copied using the inspector.
			new( 0f, 1f, 0f, 0f ),
			new( 0.046875f, 0.75277776f, 15.142873f, -15.142873f ),
			new( 0.1f, 0.33888888f, 2.758621f, -2.758621f ),
			new( 0.35f, 0.06666666f, 0.15999971f, -0.15999971f ),
			new( 1f, 0f, 0f, 0f )
		)
		{
			TimeRange = new( 0f, 1f ),
			ValueRange = new( 0f, 1f ),
		};
}
