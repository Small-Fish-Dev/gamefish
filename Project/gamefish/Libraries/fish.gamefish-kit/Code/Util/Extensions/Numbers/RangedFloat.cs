namespace GameFish;

// Setting RangedFloat.Max also sets the type, so..
#pragma warning disable CS0618 // Type or member is obsolete

partial class Library
{
	/// <summary>
	/// Clamps all values of a <see cref="RangedFloat"/> to be at most the specified value.
	/// </summary>
	/// <returns> A <see cref="RangedFloat"/> that's at most the specified value. </returns>
	public static RangedFloat Min( this RangedFloat r, in float min )
	{
		r.x = r.x.Min( min );
		r.y = r.y.Min( min );

		return r;
	}

	/// <summary>
	/// Clamps all values of a <see cref="RangedFloat"/> to be at least the specified value.
	/// </summary>
	/// <returns> A <see cref="RangedFloat"/> that's at least the specified value. </returns>
	public static RangedFloat Max( this RangedFloat r, in float max )
	{
		r.x = r.x.Max( max );
		r.y = r.y.Max( max );

		return r;
	}

	/// <summary>
	/// Clamps all values of a <see cref="RangedFloat"/> to be at most a minimum and at least a maximum value.
	/// </summary>
	/// <returns> A <see cref="RangedFloat"/> with its fixed/min/max values clamped. </returns>
	public static RangedFloat Clamp( this RangedFloat r, in float min, in float max )
	{
		r.x = r.x.Clamp( min, max );
		r.y = r.y.Clamp( min, max );

		return r;
	}
}
