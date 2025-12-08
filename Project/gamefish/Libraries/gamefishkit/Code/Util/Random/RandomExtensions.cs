namespace GameFish;

partial class Library
{
	/// <returns> Integer between 0 and <paramref name="n"/>. </returns>
	public static int Random( this int n )
		=> n switch
		{
			0 => 0,
			< 0 => GameFish.Random.Int( n, 0 ),
			_ => GameFish.Random.Int( 0, n )
		};

	/// <returns> Float between 0 and <paramref name="n"/>. </returns>
	public static float Random( this float n )
		=> n switch
		{
			0f => 0f,
			< 0f => GameFish.Random.Float( n, 0f ),
			_ => GameFish.Random.Float( 0f, n )
		};

	/// <summary>
	/// Gets a random value from this set.
	/// </summary>
	/// <returns> An random <typeparamref name="T"/> within(or <paramref name="default"/>). </returns>
	public static T PickRandom<T>( this IEnumerable<T> set, T @default = default )
		=> GameFish.Random.From( set, @default );

	/// <summary>
	/// Gets and then removes a random <typeparamref name="T"/> from this list.
	/// </summary>
	/// <returns> An random <typeparamref name="T"/> within(or <paramref name="default"/>). </returns>
	public static T TakeRandom<T>( this IList<T> list, T @default = default )
		=> GameFish.Random.TryTake( list, out T value ) ? value : @default;

	/// <summary>
	/// Tries to pick and then remove a random <typeparamref name="T"/> from an <see cref="IList{T}"/>.
	/// </summary>
	/// <returns> If a random <typeparamref name="T"/> was found and thus removed. </returns>
	public static bool TryTakeRandom<T>( this IList<T> list, out T value )
		=> GameFish.Random.TryTake( list, out value );
}
