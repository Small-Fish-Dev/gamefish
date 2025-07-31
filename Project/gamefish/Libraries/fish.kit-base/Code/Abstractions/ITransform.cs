namespace GameFish;

public interface ITransform
{
	public Vector3 Position { get => GetPosition(); set => TrySetPosition( in value ); }
	public Rotation Rotation { get => GetRotation(); set => TrySetRotation( in value ); }
	public Vector3 Scale { get => GetScale(); set => TrySetScale( in value ); }

	/// <returns> The transform's world position. </returns>
	public Vector3 GetPosition();
	/// <returns> The transform's world rotation. </returns>
	public Rotation GetRotation();
	/// <returns> The transform's world scale. </returns>
	public Vector3 GetScale();

	/// <summary>
	/// The center of the object, such as its mass center or hull center. <br />
	/// Can be used to avoid calculating its bounds every time(expensive!).
	/// </summary>
	public Vector3 Center => GetPosition();

	private static bool IsValid( in float n )
		=> !float.IsNaN( n ) && !float.IsInfinity( n );

	public static bool IsValid( in Vector3 v ) => !v.IsNaN && !v.IsInfinity;
	public static bool IsValid( in Rotation r ) => IsValid( r.x ) && IsValid( r.y ) && IsValid( r.z ) && IsValid( r.w );
	public static bool IsValid( in Transform t ) => IsValid( t.Position ) && IsValid( t.Rotation ) && IsValid( t.Scale );

	/// <summary>
	/// Safely sets the position.
	/// </summary>
	/// <returns> If the position wasn't a NaN or infinity. </returns>
	public bool TrySetPosition( in Vector3 newPos );

	/// <summary>
	/// Safely sets the rotation.
	/// </summary>
	/// <returns> If the rotation wasn't a NaN or infinity. </returns>
	public bool TrySetRotation( in Rotation rNew );

	/// <summary>
	/// Safely sets the scale.
	/// </summary>
	/// <returns> If the scale wasn't a NaN or infinity. </returns>
	public bool TrySetScale( in Vector3 newScale );
}
