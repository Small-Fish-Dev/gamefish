namespace GameFish;

/// <summary>
/// Where and how the sound should be played.
/// </summary>
public struct SoundSettings
{
	/// <summary>
	/// The object this sound is attached to.
	/// </summary>
	public GameObject Following { get; set; }

	/// <summary>
	/// This is treated as local if <see cref="Following"/> is defined.
	/// </summary>
	public Transform? Transform { get; set; }

	public float? Pitch { get; set; }
	public float? Volume { get; set; }

	public string Mixer { get; set; }

	public SoundSettings() { }

	public SoundSettings( Vector3 worldPos )
	{
		Following = null;
		Transform = new( worldPos );
	}

	/// <summary>
	/// Creates a sound configuration that's relative to an object.
	/// </summary>
	/// <param name="obj"> The object the sound is relative to. </param>
	/// <param name="pos"> The local or world position. </param>
	/// <param name="isWorldPos"> Is <paramref name="pos"/> a world-space coordinate? </param>
	public SoundSettings( GameObject obj, in Vector3 pos, in bool isWorldPos )
	{
		Following = obj;

		var soundPos = isWorldPos && obj.IsValid()
			? obj.WorldTransform.PointToLocal( pos )
			: pos;

		Transform = new( pos: soundPos );
	}

	/// <returns> A sound configuration using a world-space point. </returns>
	public static SoundSettings InWorld( in Vector3 worldPos )
		=> new( worldPos );

	/// <returns> A sound configuration relative to an object given a world-space point. </returns>
	public static SoundSettings InWorld( GameObject obj, in Vector3 worldPos )
		=> new( obj, worldPos, isWorldPos: true );

	/// <returns> A sound configuration relative to an object given a local-space point. </returns>
	public static SoundSettings InLocal( GameObject obj, in Vector3 localPos )
		=> new( obj, localPos, isWorldPos: false );
}
