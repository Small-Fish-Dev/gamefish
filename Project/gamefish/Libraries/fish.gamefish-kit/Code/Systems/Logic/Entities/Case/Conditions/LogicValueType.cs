using System;

namespace GameFish;

/// <summary>
/// The different types of values explicitly supported by the logic system.
/// </summary>
[Group( Library.NAME )]
[DefaultValue( Number )]
public enum LogicValueType
{
	Number,
	Boolean,
	String,
	Toggle,
	// Custom,
}

partial class Library
{
	public static bool Compare( this LogicValueType type, object a, object b, in NumberComparison compare )
	{
		if ( a is null || b is null )
			return false;

		return type switch
		{
			LogicValueType.Number => CompareNumber( a, b, compare ),
			LogicValueType.Boolean => a is IEquatable<bool> aBool && CompareBoolean( aBool, b ),
			LogicValueType.String => a is string aString && CompareString( aString, b ),
			LogicValueType.Toggle => a is ToggleCommand aToggle && CompareEnum( aToggle, b ),
			_ => false,
		};
	}

	// TODO: Number comparison types (<, >).
	public static bool CompareNumber( object a, object b, NumberComparison compare )
	{
		if ( a is int aInt )
			return CompareInt( aInt, b, compare );

		if ( a is float aFloat )
			return CompareFloat( aFloat, b, compare );

		if ( a is double aDouble )
			return CompareDouble( aDouble, b, compare );

		return false;
	}

	public static bool CompareInt( in int a, object b, in NumberComparison compare )
	{
		return b switch
		{
			int n => CompareInt( in a, n, in compare ),
			uint n => CompareInt( in a, n, in compare ),
			long n => CompareInt( in a, n, in compare ),
			float n => CompareInt( in a, n, in compare ),
			double n => CompareInt( in a, n, in compare ),
			_ => false
		};
	}

	public static bool CompareFloat( in float a, object b, in NumberComparison compare )
	{
		return b switch
		{
			int n => Compare( in a, n, in compare ),
			uint n => Compare( in a, n, in compare ),
			long n => Compare( in a, n, in compare ),
			ulong n => Compare( in a, n, in compare ),
			float n => Compare( in a, n, in compare ),
			double n => Compare( in a, n, in compare ),
			_ => false
		};
	}

	public static bool CompareDouble( in double a, object b, in NumberComparison compare )
	{
		return b switch
		{
			int n => Compare( in a, n, in compare ),
			uint n => Compare( in a, n, in compare ),
			long n => Compare( in a, n, in compare ),
			ulong n => Compare( in a, n, in compare ),
			float n => Compare( in a, n, in compare ),
			double n => Compare( in a, n, in compare ),
			_ => false
		};
	}

	public static bool Compare( in float a, float b, in NumberComparison compare )
	{
		return compare switch
		{
			NumberComparison.Equal => a == b,
			NumberComparison.Lesser => a > b,
			NumberComparison.Greater => a < b,
			NumberComparison.LesserEqual => a >= b,
			NumberComparison.GreaterEqual => a <= b,
			_ => false
		};
	}

	public static bool Compare( in float a, double b, in NumberComparison compare )
	{
		return compare switch
		{
			NumberComparison.Equal => a == b,
			NumberComparison.Lesser => a > b,
			NumberComparison.Greater => a < b,
			NumberComparison.LesserEqual => a >= b,
			NumberComparison.GreaterEqual => a <= b,
			_ => false
		};
	}

	public static bool Compare( in double a, double b, in NumberComparison compare )
	{
		return compare switch
		{
			NumberComparison.Equal => a == b,
			NumberComparison.Lesser => a > b,
			NumberComparison.Greater => a < b,
			NumberComparison.LesserEqual => a >= b,
			NumberComparison.GreaterEqual => a <= b,
			_ => false
		};
	}

	public static bool CompareBoolean<TBool>( TBool a, object b )
		where TBool : notnull, IEquatable<bool>
	{
		if ( b is not TBool bNum )
			return false;

		return a?.Equals( bNum ) is true;
	}

	// TODO: String comparison types(approximate, exact case).
	public static bool CompareString<TString>( TString a, object b )
		where TString : IEquatable<string>
	{
		return a?.Equals( b ) is true;
	}

	public static bool CompareEnum<Enum>( Enum a, object b )
	{
		if ( b is not Enum bEnum )
			return false;

		return a.Equals( bEnum );
	}
}
