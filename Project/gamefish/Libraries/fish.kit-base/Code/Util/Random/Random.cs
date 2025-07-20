using System;

namespace GameFish;

/// <summary>
/// Convenience methods for randomization. <br />
/// Less typing means cleaner code. 🎲
/// </summary>
public static class Random
{
	/// <returns> True or false. </returns>
	public static bool CoinFlip => Game.Random.Int( 0, 1 ) == 1;

	/// <returns> Integer between 0 and <paramref name="max"/>. </returns>
	public static int Int( int max ) => Game.Random.Int( max );
	/// <returns> Integer between <paramref name="a"/> and <paramref name="b"/>. </returns>
	public static int Int( int a, int b ) => Game.Random.Int( a, b );
	/// <returns> Float between 0 and <paramref name="max"/>. </returns>
	public static float Float( float max ) => Game.Random.Float( max );
	/// <returns> Float between <paramref name="a"/> and <paramref name="b"/>. </returns>
	public static float Float( float a, float b ) => Game.Random.Float( a, b );

	public static float From( RangedFloat range )
	{
		return Float( range.Min, range.Max );
	}

	public static T From<T>( IEnumerable<T> list, T @default = default )
	{
		if ( list is null )
			return @default;

		return Game.Random.FromArray( list.ToArray(), @default );
	}

	public static T From<T>( List<T> list, T @default = default )
	{
		if ( list is null )
			return @default;

		return Game.Random.FromList( list, @default );
	}

	public static T From<T>( T[] array, T @default = default )
	{
		if ( array is null )
			return @default;

		return Game.Random.FromArray( array, @default );
	}

	/// <returns> A random value from an enumeration. </returns>
	public static T From<T>() where T : notnull, Enum
	{
		return From( Enum.GetValues( typeof( T ) ) as T[] );
	}
}
