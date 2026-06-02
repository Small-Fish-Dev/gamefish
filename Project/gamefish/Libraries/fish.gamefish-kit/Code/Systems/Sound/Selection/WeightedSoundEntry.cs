namespace GameFish;

public sealed class WeightedSoundEntry : SoundEntry
{
	/// <summary>
	/// Higher numbers increase the likelihood of playing this entry.
	/// </summary>
	[WideMode]
	[Step( 1f )]
	[Range( 1f, 1000f, clamped: false )]
	public float Weight { get; set; } = 100f;
}
