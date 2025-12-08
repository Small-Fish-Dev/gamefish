using System;

namespace GameFish;

partial class Library
{
	/// <summary>
	/// <see cref="MathF.Abs"/>
	/// </summary>
	public static float Abs( this in float n )
		=> MathF.Abs( n );

	/// <summary>
	/// <see cref="MathF.Sign"/>
	/// </summary>
	public static int Sign( this in float n )
		=> float.IsNaN( n ) ? 0 : MathF.Sign( n );

	/// <returns> A sign that's never zero(will be 1 instead). </returns>
	public static float Direction( this in float n )
		=> n.Sign() == 1 ? 1 : -1;

	/// <returns> If the number was within the range. </returns>
	public static bool Within( this in float n, in float min, in float max )
		=> n >= min && n <= max;

	/// <returns> If the number was within the range. </returns>
	public static bool Within( this in float n, in IntRange range )
		=> range.Within( in n );

	/// <returns> If the number was within the range. </returns>
	public static bool Within( this in float n, in FloatRange range )
		=> range.Within( in n );

	/// <summary>
	/// <see cref="MathF.Round(float)"/>
	/// </summary>
	public static float Round( this in float n )
		=> MathF.Round( n );

	/// <summary>
	/// <see cref="MathF.Round(float, int)"/>
	/// </summary>
	public static float Round( this in float n, in int digits )
		=> MathF.Round( n, digits );

	/// <summary>
	/// <see cref="MathX.Clamp"/>
	/// </summary>
	public static float Clamp( this in float n, in FloatRange range )
		=> n.Clamp( range.Min, range.Max );

	/// <summary>
	/// <see cref="MathF.Min"/>
	/// </summary>
	public static float Min( this float a, in float b )
		=> MathF.Min( a, b );

	/// <summary>
	/// <see cref="MathF.Max"/>
	/// </summary>
	public static float Max( this float a, in float b )
		=> MathF.Max( a, b );

	/// <returns> A number that's at least zero. </returns>
	public static float Positive( this in float n )
		=> n > 0f ? n : 0f;

	/// <returns> A number that's at most zero. </returns>
	public static float Negative( this in float n )
		=> n < 0f ? n : float.NegativeZero;

	/// <returns> A number that's at least this far away from zero. </returns>
	public static float NonZero( this in float n, in float epsilon = float.Epsilon )
		=> n.Abs() < epsilon ? epsilon * n.Direction() : n;

	/// <returns> This number to the specified power. </returns>
	public static float Pow( this in float n, in float power )
		=> MathF.Pow( n, power );

	/// <returns> The square root of this number. </returns>
	public static float Sqrt( this in float n )
		=> MathF.Sqrt( n );
}
