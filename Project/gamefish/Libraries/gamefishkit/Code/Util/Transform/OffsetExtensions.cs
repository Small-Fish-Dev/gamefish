namespace GameFish;

public static class OffsetExtensions
{
	/// <summary>
	/// Adds an offset's position/rotation relatively to a transform.
	/// </summary>
	/// <remarks> This can be useful on either world or local transforms. </remarks>
	public static Transform WithOffset( this in Transform t, in Offset offset )
		=> t.ToWorld( offset );

	/// <summary>
	/// Lerps a transform to an offset's position/rotation while preserving its original scale.
	/// </summary>
	/// <remarks>
	/// This is typically best used on local transforms.
	/// You probably don't want to use it on world transforms.
	/// </remarks>
	public static Transform LerpTo( this in Transform t, in Offset offset, in float frac )
		=> t.LerpTo( offset.Transform.WithScale( t.Scale ), frac );

	/// <summary>
	/// Sets a transform to an offset's position/rotation while preserving its original scale.
	/// </summary>
	/// <remarks>
	/// This is typically best used on local transforms.
	/// You probably don't want to use it on world transforms.
	/// </remarks>
	public static Transform SetOffset( this in Transform t, in Offset offset )
		=> offset.Transform.WithScale( t.Scale );

	/// <summary>
	/// Directly sets the object's local transform to the specified offset.
	/// </summary>
	public static void SetOffset( this GameObject self, in Offset offset )
	{
		if ( !self.IsValid() )
			return;

		var pos = offset.Position;
		var r = offset.Rotation;

		if ( !ITransform.IsValid( in pos ) || !ITransform.IsValid( in r ) )
			return;

		self.LocalPosition = pos;
		self.LocalRotation = r;
	}

	/// <summary>
	/// Sets an object's offset from the specified transform.
	/// </summary>
	public static void SetOffset( this GameObject self, in Transform t, in Offset offset )
	{
		if ( !self.IsValid() )
			return;

		var pos = t.PointToWorld( offset.Position );
		var r = t.RotationToWorld( offset.Rotation );

		if ( !ITransform.IsValid( in pos ) || !ITransform.IsValid( in r ) )
			return;

		self.WorldPosition = pos;
		self.WorldRotation = r;
	}

	/// <summary>
	/// Directly sets the local transform to the specified offset.
	/// </summary>
	public static void SetOffset( this Component self, in Offset offset )
		=> self?.GameObject?.SetOffset( offset );

	/// <summary>
	/// Sets an object's offset from the specified transform.
	/// </summary>
	public static void SetOffset( this Component self, in Transform t, in Offset offset )
		=> self?.GameObject?.SetOffset( t, offset );

	/// <summary>
	/// Sets an object's offset relative to another object.
	/// </summary>
	public static void SetOffset( this GameObject self, GameObject other, in Offset offset )
	{
		if ( self.IsValid() && other.IsValid() )
			self.SetOffset( other.WorldTransform, offset );
	}

	/// <summary>
	/// Sets an object's offset relative to another object.
	/// </summary>
	public static void SetOffset( this GameObject self, Component other, in Offset offset )
	{
		self?.SetOffset( other?.GameObject, offset );
	}

	/// <summary>
	/// Sets an object's offset relative to another object.
	/// </summary>
	public static void SetOffset( this Component self, GameObject other, in Offset offset )
		=> self?.GameObject?.SetOffset( other, offset );

	/// <summary>
	/// Sets an object's offset relative to another object.
	/// </summary>
	public static void SetOffset( this Component self, Component other, in Offset offset )
		=> self?.GameObject?.SetOffset( other?.GameObject, offset );
}
