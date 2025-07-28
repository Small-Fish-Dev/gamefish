namespace GameFish;

public static partial class RandomExtensions
{
	/// <returns> Integer between 0 and <paramref name="n"/>. </returns>
	public static int Random( this int n )
		=> n switch
		{
			0 => 0,
			< 0 => global::GameFish.Random.Int( n, 0 ),
			_ => global::GameFish.Random.Int( 0, n )
		};

	/// <returns> Float between 0 and <paramref name="n"/>. </returns>
	public static float Random( this float n )
		=> n switch
		{
			0f => 0f,
			< 0f => global::GameFish.Random.Float( n, 0f ),
			_ => global::GameFish.Random.Float( 0f, n )
		};
}
