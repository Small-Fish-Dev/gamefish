using System;

namespace GameFish;

public static partial class GameFish
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
}
