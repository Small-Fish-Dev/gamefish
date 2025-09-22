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
	/// <param name="fwd"> Vector pointing in that direction. </param>
	/// <param name="side"> Relative horizontal vector. </param>
	public static void Separate( this Vector3 v, in Vector3 dir, out Vector3 fwd, out Vector3 side )
	{
		side = v.SubtractDirection( dir );
		fwd = v - side;
	}

	/// <summary>
	/// Separate a vector into its forward/sideways components using its normal(direction).
	/// </summary>
	/// <param name="v"></param>
	/// <param name="fwd"> Vector pointing in that direction. </param>
	/// <param name="side"> Relative horizontal vector. </param>
	public static void Separate( this Vector3 v, out Vector3 fwd, out Vector3 side )
		=> v.Separate( v.Normal, out fwd, out side );

	/// <remarks>
	/// Useful for velocity.
	/// </remarks>
	/// <param name="v"></param>
	/// <param name="dir"> The forward-facing direction. </param>
	/// <returns> The forward component of the vector using the specified direction. </returns>
	public static Vector3 Forward( this Vector3 v, in Vector3 dir )
		=> v - v.SubtractDirection( dir );

	/// <remarks>
	/// Useful for velocity.
	/// </remarks>
	/// <param name="v"></param>
	/// <param name="dir"> The sideways-facing direction. </param>
	/// <returns> The sideways component of the vector using the specified direction. </returns>
	public static Vector3 Sideways( this Vector3 v, in Vector3 dir )
		=> v.SubtractDirection( dir );

	/// <summary>
	/// <see cref="Vector3.Clamp(Vector3,Vector3)"/>
	/// </summary>
	public static Vector3 Clamp( this Vector3 v, in BBox bounds )
		=> v.Clamp( bounds.Mins, bounds.Maxs );

	/// <summary>
	/// Apply an amount of friction to the current velocity.
	/// </summary>
	public static Vector3 WithFriction( this Vector3 v, in Friction f, in float deltaTime )
		=> v.WithFriction( f.Value * deltaTime, f.StopSpeed );
}
