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
	public static float Clamp( this float n, FloatRange range )
		=> n.Clamp( range.Min, range.Max );
}
