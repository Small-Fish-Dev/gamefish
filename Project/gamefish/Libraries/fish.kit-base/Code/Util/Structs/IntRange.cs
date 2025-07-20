using System;

namespace GameFish;

/// <summary>
/// Min and max integers without the fixed value setting.
/// </summary>
public struct IntRange
{
	[KeyProperty] public int Min { get; set; }
	[KeyProperty] public int Max { get; set; }

	public IntRange() { }
	public IntRange( int min, int max ) { Min = min; Max = max; }

	public static implicit operator IntRange( Range r ) => new( r.Start.Value, r.End.Value );
	public static implicit operator IntRange( RangedFloat r ) => new( r.Min.FloorToInt(), r.Max.CeilToInt() );
}
