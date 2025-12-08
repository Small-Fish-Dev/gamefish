using System;

namespace GameFish;

partial class Library
{
	/// <summary>
	/// <see cref="Math.Abs(int)"/>
	/// </summary>
	public static int Abs( this in int n )
		=> Math.Abs( n );

	/// <summary>
	/// <see cref="Math.Sign(int)"/>
	/// </summary>
	public static int Sign( this in int n )
		=> Math.Sign( n );

	/// <returns> A sign that's never zero(will be 1 instead). </returns>
	public static int Direction( this in int n )
		=> n.Sign() == 1 ? 1 : -1;

	/// <returns> If the number was within the range. </returns>
	public static bool Within( this in int n, in int min, in int max )
		=> n >= min && n <= max;

	/// <returns> If the number was within the range. </returns>
	public static bool Within( this in int n, in float min, in float max )
		=> n >= min && n <= max;

	/// <returns> If the number was within the range. </returns>
	public static bool Within( this in int n, in IntRange range )
		=> range.Within( in n );

	/// <returns> If the number was within the range. </returns>
	public static bool Within( this in int n, in FloatRange range )
		=> range.Within( n );

	/// <summary>
	/// <see cref="MathX.Clamp"/>
	/// </summary>
	public static int Clamp( this in int n, in IntRange range )
		=> n.Clamp( range.Min, range.Max );

	/// <summary>
	/// <see cref="Math.Min(int,int)"/>
	/// </summary>
	public static int Min( this int a, in int b )
		=> Math.Min( a, b );

	/// <summary>
	/// <see cref="Math.Max(int,int)"/>
	/// </summary>
	public static int Max( this int a, in int b )
		=> Math.Max( a, b );

	/// <returns> A number that's at least zero. </returns>
	public static int Positive( this in int n )
		=> n > 0 ? n : 0;

	/// <returns> A number that's at most zero. </returns>
	public static int Negative( this in int n )
		=> n < 0f ? n : -0;

	/// <returns> A number that's at least this far away from zero. </returns>
	public static int NonZero( this in int n, in int epsilon = 1 )
		=> n.Abs() < epsilon ? epsilon * n.Direction() : n;

	/// <returns> This number to the specified power. </returns>
	public static float Pow( this in int n, in float power )
		=> MathF.Pow( n, power );

	/// <returns> The square root of this number. </returns>
	public static float Sqrt( this in int n )
		=> MathF.Sqrt( n );
}
