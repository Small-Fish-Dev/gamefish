namespace GameFish;

public static class OffsetExtensions
{
	/// <summary>
	/// Lerps a transform(probably local!) to an offset's position/rotation while preserving its original scale.
	/// </summary>
	public static Transform LerpTo( this in Transform t, in Offset offset, in float frac )
		=> t.LerpTo( offset.Transform.WithScale( t.Scale ), frac );

	/// <summary>
	/// Sets a transform(probably local!) to an offset's position/rotation while preserving its original scale.
	/// </summary>
	public static Transform SetOffset( this in Transform t, in Offset offset )
		=> offset.Transform.WithScale( t.Scale );

	/// <summary>
	/// Directly sets the local transform to the specified offset.
	/// </summary>
	public static void SetOffset( this GameObject self, in Offset offset )
	{
		if ( !self.IsValid() )
			return;

		self.LocalPosition = offset.Position;
		self.LocalRotation = offset.Rotation;
	}

	/// <summary>
	/// Sets an object's offset from the specified transform.
	/// </summary>
	public static void SetOffset( this GameObject self, in Transform t, in Offset offset )
	{
		if ( self.IsValid() )
			self.WorldTransform = t.ToWorld( offset.Transform ).WithScale( self.WorldScale );
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
