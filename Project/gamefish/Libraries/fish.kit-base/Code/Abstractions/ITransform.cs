namespace GameFish;

/// <summary>
/// Provides methods to handle transformations safely
/// and an optimized way of getting the center.
/// </summary>
public interface ITransform
{
	/// <summary>
	/// The effective center of the object, such as its mass/hull center.
	/// Can be used to avoid calculating its bounds every time(expensive!).
	/// </summary>
	public Vector3 Center { get; }

	public static bool IsValid( in float n )
		=> !float.IsNaN( n ) && !float.IsInfinity( n );

	public static bool IsValid( in Vector3 v ) => !v.IsNaN && !v.IsInfinity;
	public static bool IsValid( in Rotation r ) => IsValid( r.x ) && IsValid( r.y ) && IsValid( r.z ) && IsValid( r.w );
	public static bool IsValid( in Transform t ) => IsValid( t.Position ) && IsValid( t.Rotation ) && IsValid( t.Scale );

	/// <summary>
	/// Allows this object to specify how it is teleported. <br />
	/// Example: you could have a wheel of a car teleport the entire car.
	/// </summary>
	/// <returns> If the teleport was a success. </returns>
	public virtual bool TryTeleport( in Transform tDest )
		=> false;
}
