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

	/// <returns> A random value from any kind of set(or <paramref name="default"/>). </returns>
	public static T From<T>( IEnumerable<T> set, in T @default = default )
	{
		if ( set is null )
			return @default;

		var count = set.Count();

		if ( count <= 0 )
			return @default;

		return set.ElementAt( (count - 1).Random() );
	}

	/// <summary>
	/// Tries to pick and then remove a random <typeparamref name="T"/> from an <see cref="IList{T}"/>.
	/// </summary>
	/// <returns> A random <typeparamref name="T"/> from a <see cref="IList{T}"/>. </returns>
	public static bool TryTake<T>( IList<T> list, out T value )
	{
		if ( list is null )
		{
			value = default;
			return false;
		}

		var count = list.Count;

		if ( count <= 0 )
		{
			value = default;
			return false;
		}

		var iRandom = (count - 1).Random();

		value = list.ElementAt( iRandom );
		list.RemoveAt( iRandom );

		return true;
	}

	/// <returns> A random value from an <see cref="Array"/>(or <paramref name="default"/>). </returns>
	public static T From<T>( T[] array, in T @default = default )
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
