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
}
