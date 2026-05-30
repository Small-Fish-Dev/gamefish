namespace GameFish;

partial class Library
{
	/// <summary>
	/// <see cref="Vector3.Direction(in Vector3, in Vector3)"/>
	/// </summary>
	/// <returns> The direction from one position to another. </returns>
	public static Vector3 Direction( this Vector3 from, in Vector3 to )
		=> Vector3.Direction( from, to );

	/// <summary>
	/// Zeroes out the <c>Z</c> axis then normalizes and scales the vector.
	/// </summary>
	/// <param name="v"></param>
	/// <param name="len"> The amount to scale the normalized vector. </param>
	/// <returns> The vector flattened and normalized to a 2D length. </returns>
	public static Vector3 Flatten( this Vector3 v, in float len = 1f )
		=> v.WithZ( 0f ).Normal * len;

	/// <summary>
	/// Zeroes out the <c>Z</c> axis then normalizes and scales the vector.
	/// Fails if the result is a zero vector(where x/y/z are all zero).
	/// </summary>
	/// <param name="v"></param>
	/// <param name="vf"> The flattened, normalized and scaled vector. </param>
	/// <param name="len"> The amount to scale the normalized vector. </param>
	/// <returns> If the vector could be flattened without becoming a zero vector. </returns>
	public static bool TryFlatten( this Vector3 v, out Vector3 vf, in float len = 1f )
	{
		vf = v.Flatten( len );
		return vf != default;
	}

	/// <summary>
	/// Projects a vector onto a plane's normal.
	/// </summary>
	/// <param name="v"></param>
	/// <param name="planeNormal"> The plane's normal that the vector will reflect against. </param>
	/// <returns> A vector reflected against a normal. </returns>
	public static Vector3 Reflect( this Vector3 v, in Vector3 planeNormal )
		=> Vector3.Reflect( in v, in planeNormal );

	/// <summary>
	/// Projects a vector onto a plane's normal.
	/// </summary>
	/// <param name="v"></param>
	/// <param name="planeNormal"> The plane's normal that the vector will lay flat on. </param>
	/// <returns> A vector projected against a normal. </returns>
	public static Vector3 PlaneProject( this Vector3 v, in Vector3 planeNormal )
		=> Vector3.VectorPlaneProject( in v, in planeNormal );

	/// <summary>
	/// Projects a vector onto a plane's normal then normalizes and scales it.
	/// </summary>
	/// <param name="v"></param>
	/// <param name="planeNormal"> The plane's normal that the vector will lay flat on. </param>
	/// <param name="length"> The resulting length of the vector(if it's not zero). </param>
	/// <returns> A vector projected against a normal with a specific length. </returns>
	public static Vector3 PlaneProject( this Vector3 v, in Vector3 planeNormal, in float length )
		=> v.PlaneProject( in planeNormal ).Normal * length;

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
	/// <param name="forward"> The forward-facing direction. </param>
	/// <returns> The forward component of the vector in the specified direction. </returns>
	public static Vector3 Forward( this Vector3 v, in Vector3 forward )
		=> v - v.Horizontal( forward );

	/// <remarks>
	/// Useful for velocity.
	/// </remarks>
	/// <param name="v"></param>
	/// <param name="vertical"> The upward-facing direction. </param>
	/// <returns> The horizontal component of the vector from the specified direction. </returns>
	public static Vector3 Horizontal( this Vector3 v, in Vector3 vertical )
		=> v.SubtractDirection( vertical );

	/// <summary>
	/// <see cref="Vector3.Clamp(Vector3, Vector3)"/>
	/// </summary>
	public static Vector3 Clamp( this Vector3 v, in BBox bounds )
		=> v.Clamp( bounds.Mins, bounds.Maxs );

	/// <summary>
	/// Apply an amount of friction to the current velocity.
	/// </summary>
	public static Vector3 WithFriction( this Vector3 v, in Friction f, in float deltaTime )
		=> v.WithFriction( f.Value * deltaTime, f.StopSpeed );
}
