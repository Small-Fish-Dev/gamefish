using System;

namespace GameFish;

/// <summary>
/// Min and max integers without the fixed value setting.
/// </summary>
public struct IntRange
{
	[KeyProperty] public int Min { readonly get => _min; set => _min = value; }
	[KeyProperty] public int Max { readonly get => _max; set => _max = value; }

	[Hide] private int _min;
	[Hide] private int _max;

	public IntRange() { }
	public IntRange( int min, int max ) { Min = min; Max = max; }

	/// <returns> If a number is within <see cref="Min"/> and <see cref="Max"/>. </returns>
	public readonly bool Within( in int n )
		=> n >= _min && n <= _max;

	/// <returns> If a number is within <see cref="Min"/> and <see cref="Max"/>. </returns>
	public readonly bool Within( in float n )
		=> n >= _min && n <= _max;

	public readonly float Lerp( [Range( 0f, 1f )] in float frac, in bool clamp = true )
		=> MathX.Lerp( _min, _max, frac, clamp: clamp );

	public readonly float Remap( in float value, in float from = 0f, in float to = 1f, in bool clamp = true )
		=> MathX.Remap( value, _min, _max, from, to, clamp );

	public readonly int RemapFloor( in float value, in float from = 0f, in float to = 1f, in bool clamp = true )
		=> Remap( value, from, to, clamp ).FloorToInt();

	public readonly int RemapCeil( in float value, in float from = 0f, in float to = 1f, in bool clamp = true )
		=> Remap( value, from, to, clamp ).CeilToInt();

	public readonly int GetRandom()
		=> Random.Int( _min, _max );

	public static implicit operator IntRange( in Range r ) => new( r.Start.Value, r.End.Value );
	public static implicit operator IntRange( in RangedFloat r ) => new( r.Min.FloorToInt(), r.Max.CeilToInt() );
}
