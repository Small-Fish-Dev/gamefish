using System;

namespace GameFish;

partial class Library
{
	/// <returns> If <paramref name="a"/> equals <paramref name="b"/>. </returns>
	public static bool Is( this in bool a, in bool b )
		=> a == b;

	/// <returns> If <paramref name="a"/> does not equal <paramref name="b"/>. </returns>
	public static bool Not( this in bool a, in bool b )
		=> a != b;

	/// <returns> The only other value that boolean could be. </returns>
	public static bool Flip( this in bool b )
		=> !b;

	/// <returns> <c>1</c> if <c>true</c>, <c>-1</c> if <c>false</c>. </returns>
	public static int Direction( this in bool b )
		=> b ? 1 : -1;
}
