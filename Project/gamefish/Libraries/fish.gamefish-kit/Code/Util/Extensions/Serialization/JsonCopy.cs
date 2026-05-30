using System;
using System.Text.Json;

namespace GameFish;

partial class Library
{
	/// <summary>
	/// Safely gets a copy of the object using Json serialization. <br />
	/// May have performance impacts if used constantly.
	/// </summary>
	/// <returns> A deep copy of the object using Json(or <paramref name="default"/>). </returns>
	public static object CreateJsonCopy( object obj, object @default )
	{
		if ( obj is null )
			return @default;

		try
		{
			return JsonSerializer.Deserialize( JsonSerializer.Serialize( obj ), obj.GetType() );
		}
		catch ( Exception e )
		{
			Print.WarnFrom( $"{obj}", $"Failed to copy. Exception: {e}" );
			return @default;
		}
	}

	/// <inheritdocs cref="CreateJsonCopy" />
	public static object JsonCopy( this object obj, object @default )
		=> CreateJsonCopy( obj, @default );

	/// <summary>
	/// Safely gets a copy of this object as <typeparamref name="T"/> using Json serialization. <br />
	/// May have performance impacts if used constantly.
	/// </summary>
	/// <returns> A deep copy of the object using Json(or <paramref name="default"/>). </returns>
	public static T CreateJsonCopy<T>( T obj = default, T @default = default )
	{
		if ( obj is null )
			return @default;

		try
		{
			return JsonSerializer.Deserialize<T>( JsonSerializer.Serialize( obj ) );
		}
		catch ( Exception e )
		{
			Print.WarnFrom( $"{obj}", $"Failed to copy. Exception: {e}" );
			return @default;
		}
	}

	/// <inheritdocs cref="CreateJsonCopy{T}" />
	public static T JsonCopy<T>( this T obj, T @default = default )
		=> CreateJsonCopy<T>( obj, @default );
}
