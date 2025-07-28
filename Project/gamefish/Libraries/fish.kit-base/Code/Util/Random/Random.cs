using System;

namespace GameFish;

/// <summary>
/// 🎲 Convenience methods for randomization.
/// </summary>
public static partial class Random
{
	/// <returns> True or false. </returns>
	public static bool CoinFlip => Game.Random.Int( 0, 1 ) == 1;

	/// <returns> Integer between 0 and <paramref name="max"/>(or 1). </returns>
	public static int Int( int max = 1 ) => Game.Random.Int( max );
	/// <returns> Integer between <paramref name="a"/> and <paramref name="b"/>. </returns>
	public static int Int( int a, int b ) => Game.Random.Int( a, b );

	/// <returns> Float between 0 and <paramref name="max"/>(or 1). </returns>
	public static float Float( float max = 1f ) => Game.Random.Float( max );
	/// <returns> Float between <paramref name="a"/> and <paramref name="b"/>. </returns>
	public static float Float( float a, float b ) => Game.Random.Float( a, b );

	/// <returns> Double between 0 and <paramref name="max"/>(or 1). </returns>
	public static double Double( double max = 1d ) => Game.Random.Double( 0, max );
	/// <returns> Double between <paramref name="a"/> and <paramref name="b"/>. </returns>
	public static double Double( double a, double b ) => Game.Random.Double( a, b );

	public static float From( in RangedFloat range )
		=> Float( range.Min, range.Max );

	public static float From( in FloatRange range )
		=> range.GetRandom();

	public static float From( in IntRange range )
		=> range.GetRandom();

	/// <returns> A random value from any kind of list(or <paramref name="default"/>). </returns>
	public static T From<T>( IEnumerable<T> list, T @default = default )
	{
		if ( list is null )
			return @default;

		var count = list.Count();

		if ( count <= 0 )
			return @default;

		return list.ElementAtOrDefault( count.Random() );
	}

	/// <returns> A random value from a <see cref="List{T}"/>(or <paramref name="default"/>). </returns>
	public static T From<T>( List<T> list, T @default = default )
	{
		if ( list is null )
			return @default;

		return Game.Random.FromList( list, @default );
	}

	/// <returns> A random value from an <see cref="Array"/>(or <paramref name="default"/>). </returns>
	public static T From<T>( T[] array, T @default = default )
	{
		if ( array is null )
			return @default;

		return Game.Random.FromArray( array, @default );
	}

	/// <returns> A random value from an enumeration. </returns>
	public static T From<T>() where T : Enum
		=> From( Enum.GetValues( typeof( T ) ) as T[] );

	/// <returns> A random value from an enumeration without repetitions. </returns>
	public static T FromDistinct<T>() where T : Enum
		=> From( (Enum.GetValues( typeof( T ) ) as T[]).Distinct() );
}
