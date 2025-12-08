namespace GameFish;

partial class Actor
{
	/// <summary>
	/// The fixed randomization seed. Auto-generated.
	/// Lets you produce consistent offsets using randomization methods.
	/// </summary>
	[Sync]
	public int Seed
	{
		// Auto-generate the seed if we own this.
		get => !_seed.HasValue && !IsProxy ? ShuffleRandomSeed() : _seed ?? 0;
		set => _seed = value;
	}

	protected int? _seed;

	/// <returns> A random integer that is at least 1. </returns>
	protected static int RandomSeed => Random.Int( 1, 42069 );

	/// <summary>
	/// Tells the game to use this guy's randomization offset.
	/// </summary>
	public virtual void RestoreSeed() => Game.SetRandomSeed( Seed );

	/// <summary>
	/// Refreshes our randomization offset.
	/// </summary>
	public virtual void ResetSeed() => ShuffleRandomSeed();

	/// <summary>
	/// Sets <see cref="Seed"/> to a new, highly randomized number. <br />
	/// You probably only want to call this once(upon start), if ever.
	/// </summary>
	protected virtual int ShuffleRandomSeed( int shuffles = 69 )
	{
		// Randomization of the randomization.
		for ( var i = 0; i < shuffles; i++ )
			Game.SetRandomSeed( RandomSeed );

		return Seed = RandomSeed;
	}
}
