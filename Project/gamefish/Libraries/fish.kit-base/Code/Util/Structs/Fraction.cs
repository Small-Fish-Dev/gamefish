using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// A <see cref="float"/> that is always between 0 and 1.
/// </summary>
public struct Fraction
{
	[KeyProperty]
	[Range( 0f, 1f ), Step( 0.001f )]
	public float Value { readonly get => _value; set => _value = value.Clamp( 0f, 1f ); }

	[Hide, JsonIgnore]
	private float _value;

	public Fraction() { }
	public Fraction( in float val ) => Value = val;

	public static implicit operator Fraction( in float val ) => new( val );
	public static implicit operator float( in Fraction val ) => val._value;

	public static bool operator ==( in Fraction a, in Fraction b ) => a._value == b._value;
	public static bool operator !=( in Fraction a, in Fraction b ) => !(a == b);

	public static bool operator ==( in float a, in Fraction b ) => a == b.Value;
	public static bool operator !=( in float a, in Fraction b ) => !(a == b);

	public readonly override bool Equals( object obj ) => obj is Fraction frac && this == frac;

	public readonly override int GetHashCode() => System.HashCode.Combine( Value );
}
