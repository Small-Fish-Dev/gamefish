using System;

namespace GameFish;

/// <summary>
/// Min and max floats without a fixed value setting.
/// </summary>
public struct FloatRange
{
	[KeyProperty] public float Min { get; set; }
	[KeyProperty] public float Max { get; set; }

	public FloatRange() { }
	public FloatRange( float min, float max ) { Min = min; Max = max; }

	public static implicit operator FloatRange( Range r ) => new( r.Start.Value, r.End.Value );
	public static implicit operator FloatRange( RangedFloat r ) => new( r.Min, r.Max );
}
