namespace GameFish;

public static partial class GameFish
{
	public static Vector3 Direction( this Vector3 from, in Vector3 to ) => Vector3.Direction( from, to );

	/// <summary>
	/// B - A = Δ <br />
	/// Add this to <paramref name="from"/> to get <paramref name="to"/>.
	/// </summary>
	public static Vector3 Offset( this Vector3 from, in Vector3 to ) => to - from;

	/// <summary>
	/// Separate a vector into its forward/sideways components using the direction specified.
	/// </summary>
	/// <param name="v"></param>
	/// <param name="dir"> The forward-facing direction. </param>
	/// <param name="fwd"> Velocity pointing in that direction. </param>
	/// <param name="side"> Relative horizontal velocity. </param>
	public static void Separate( this Vector3 v, in Vector3 dir, out Vector3 fwd, out Vector3 side )
	{
		side = v.SubtractDirection( dir );
		fwd = v - side;
	}

	/// <summary>
	/// Gets the forward component of a vector using the specified direction. <br />
	/// Useful for velocity.
	/// </summary>
	/// <param name="v"></param>
	/// <param name="dir"> The forward-facing direction. </param>
	/// <returns> The forward component of the vector using the specified direction. </returns>
	public static Vector3 Forward( this Vector3 v, in Vector3 dir )
	{
		// Log( "Vector3.Forward", v - v.SubtractDirection( dir ) );
		return v - v.SubtractDirection( dir );
	}

	/// <summary>
	/// Gets the sideways component of a vector using the specified direction. <br />
	/// Useful for velocity.
	/// </summary>
	/// <param name="v"></param>
	/// <param name="dir"> The sideways-facing direction. </param>
	/// <returns> The sideways component of the vector using the specified direction. </returns>
	public static Vector3 Sideways( this Vector3 v, in Vector3 dir )
	{
		// Log( "Vector3.Sideways", v.SubtractDirection( dir ) );
		return v.SubtractDirection( dir );
	}
}
