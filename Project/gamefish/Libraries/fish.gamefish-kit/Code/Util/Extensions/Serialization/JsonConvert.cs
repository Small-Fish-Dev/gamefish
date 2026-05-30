using System;
using System.Text.Json;

namespace GameFish;

partial class Library
{
	/// <summary>
	/// Safely creates a Json string representing the object. <br />
	/// May have performance impacts if used constantly.
	/// </summary>
	/// <returns> A string copy of the object using Json(or <c>null</c>). </returns>
	public static string GetJsonString( object obj, Type type, string @default = null, JsonSerializerOptions options = null )
	{
		if ( obj is null || type is null )
			return @default;

		try
		{
			return JsonSerializer.Serialize( obj, options ) ?? @default;
		}
		catch ( Exception e )
		{
			Print.WarnFrom( $"{obj}", $"Failed to convert to Json string. Exception: {e}" );
			return @default;
		}
	}

	/// <inheritdoc cref="GetJsonString" />
	public static string ToJsonString( this object obj, string @default = null, JsonSerializerOptions options = null )
		=> GetJsonString( obj, obj?.GetType(), @default, options );

	/// <inheritdoc cref="GetJsonString" />
	public static string ToJsonString( this object obj, Type type, string @default = null, JsonSerializerOptions options = null )
		=> GetJsonString( obj, type, @default, options );

	/// <inheritdoc cref="GetJsonString" />
	public static string ToJsonString<T>( this object obj, string @default = null, JsonSerializerOptions options = null )
		=> GetJsonString( obj, typeof( T ), @default, options );

	/// <summary>
	/// Safely creates a Json object from its string representation. <br />
	/// May have performance impacts if used constantly.
	/// </summary>
	/// <returns> A new instance(or <paramref name="default"/>). </returns>
	public static object GetJsonObject( string str, Type type, object @default = default, JsonSerializerOptions options = null )
	{
		if ( str.IsBlank() || type is null )
			return @default;

		try
		{
			return JsonSerializer.Deserialize( str, type, options ) ?? @default;
		}
		catch ( Exception e )
		{
			Print.WarnFrom( $"{typeof( Library )}", $"Failed to convert string to {type}. Exception: {e}" );
			return @default;
		}
	}

	/// <inheritdoc cref="GetJsonObject" />
	public static T GetJsonObject<T>( string str, T @default = default, JsonSerializerOptions options = null )
	{
		var obj = GetJsonObject( str, typeof( T ), @default, options );

		if ( obj is T newObject )
			return newObject ?? @default;

		return @default;
	}

	/// <inheritdoc cref="GetJsonObject" />
	public static T ToJsonObject<T>( this string str, T @default = default, JsonSerializerOptions options = null )
		=> GetJsonObject( str, @default, options );

	/// <inheritdoc cref="GetJsonObject" />
	public static object ToJsonObject( this string str, Type type, object @default = default, JsonSerializerOptions options = null )
		=> GetJsonObject( str, type, @default, options );
}
