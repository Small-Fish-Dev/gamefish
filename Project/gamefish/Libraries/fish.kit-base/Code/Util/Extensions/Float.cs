using System;

namespace GameFish;

public static partial class GameFish
{
	/// <summary>
	/// <see cref="MathF.Abs"/>
	/// </summary>
	public static float Abs( this float n )
		=> MathF.Abs( n );

	/// <summary>
	/// <see cref="MathF.Sign"/>
	/// </summary>
	public static int Sign( this float n )
		=> MathF.Sign( n );

	/// <summary>
	/// <see cref="MathX.Clamp"/>
	/// </summary>
	public static float Clamp( this float n, in FloatRange range )
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
	public static float Positive( this float n )
		=> n > 0f ? n : 0f;
}
