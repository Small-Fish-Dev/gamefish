using System;

namespace GameFish;

partial class Random
{
	public static double GetTotalWeight<T>( Dictionary<T, float> dict )
	{
		double weight = 0d;

		if ( dict is null )
			return weight;

		foreach ( float wgt in dict.Values )
			weight += wgt.Positive();

		return weight;
	}

	/// <summary>
	/// Pick a random key from a dictionary with the likelihood being increased by its value. <br />
	/// It's probably better that you <see cref="WeightedList{T}.TryPick(out T, T)"/>  instead.
	/// </summary>
	/// <param name="dict"> The probabilities. </param>
	/// <param name="result"> The random result(or <c>default</c>). </param>
	/// <returns> If a value was able to be retrieved. </returns>
	public static bool TryGetWeighted<T>( Dictionary<T, float> dict, out T result )
		=> TryGetWeighted( dict, GetTotalWeight( dict ), out result );

	/// <summary>
	/// Pick a random key from a dictionary with the likelihood being increased by its value. <br />
	/// This version of the method requires that you have predetermined its maximum weight. <br />
	/// It's probably better that you <see cref="WeightedList{T}.TryPick(out T, T)"/>  instead.
	/// </summary>
	/// <param name="dict"> The probabilities. </param>
	/// <param name="totalWeight"> The pre-defined total weight. </param>
	/// <param name="result"> The random result(or <c>default</c>). </param>
	/// <param name="defaultFirst"> Chooses the first entry(if any) upon failure. </param>
	/// <returns> If a value was able to be retrieved. </returns>
	public static bool TryGetWeighted<T>( Dictionary<T, float> dict, in double totalWeight, out T result, bool defaultFirst = true )
	{
		if ( dict is null )
		{
			Print.WarnFrom( $"{typeof( Random )}.{nameof( TryGetWeighted )}", "passed in dictionary was null!" );

			result = default;
			return false;
		}

		// Pick a random number from what's possible and find a match.
		var count = dict.Count;

		double randomValue = Math.Max( 0d, totalWeight ) * Double( 0f, 1f );

		for ( var i = 0; i < count; i++ )
		{
			var (key, chance) = dict.ElementAtOrDefault( i );

			randomValue -= chance;

			if ( randomValue <= 0 )
			{
				result = key;
				return true;
			}
		}

		// Failed to find a result somehow.
		result = defaultFirst ? dict.Keys.FirstOrDefault() : default;

		return false;
	}
}
