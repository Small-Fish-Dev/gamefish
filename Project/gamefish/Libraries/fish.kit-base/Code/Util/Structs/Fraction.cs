namespace GameFish;

/// <summary>
/// A <see cref="float"/> that is always between 0 and 1.
/// </summary>
public struct Fraction
{
	[KeyProperty]
	[Range( 0f, 1f ), Step( 0.001f )]
	public float Value { readonly get => _value; set => _value = value.Clamp( 0f, 1f ); }

	[Hide]
	private float _value;

	public Fraction() { }
	public Fraction( in float val ) => Value = val;

	public static implicit operator Fraction( float val ) => new( val );

	public static bool operator ==( Fraction a, Fraction b ) => a.Value == b.Value;
	public static bool operator !=( Fraction a, Fraction b ) => !(a == b);

	public static bool operator ==( float a, Fraction b ) => a == b.Value;
	public static bool operator !=( float a, Fraction b ) => !(a == b);

	public readonly override bool Equals( object obj ) => obj is Fraction frac && this == frac;

	public readonly override int GetHashCode() => System.HashCode.Combine( Value );
}
