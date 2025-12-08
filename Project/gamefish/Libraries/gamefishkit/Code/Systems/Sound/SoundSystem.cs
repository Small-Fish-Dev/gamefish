namespace GameFish;

public partial class SoundSystem : Component
{
	private static SoundSystem _instance;
	public static SoundSystem Instance
	{
		get => _instance.GetSingleton();
		set => _instance = value;
	}

	/// <summary>
	/// Duckable sound sources will have their overall volume scaled by this amount.
	/// </summary>
	public static Fraction DuckedVolume { get; set; } = 1f;
}
