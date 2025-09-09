using System;

namespace GameFish;

/// <summary>
/// Min and max floats without a fixed value setting.
/// </summary>
public struct FloatRange
{
	[KeyProperty] public float Min { readonly get => _min; set => _min = value; }
	[KeyProperty] public float Max { readonly get => _max; set => _max = value; }

	[Hide] private float _min;
	[Hide] private float _max;

	public FloatRange() { }
	public FloatRange( float min, float max ) { Min = min; Max = max; }

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

	public readonly float GetRandom()
		=> Random.Float( _min, _max );

	public static implicit operator FloatRange( in Range r ) => new( r.Start.Value, r.End.Value );
	public static implicit operator FloatRange( in RangedFloat r ) => new( r.Min, r.Max );
}
