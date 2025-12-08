using System;
using System.Runtime.CompilerServices;
using System.Text;
using Sandbox.Diagnostics;
using Sandbox.UI;

namespace GameFish;

/// <summary>
/// Makes logging to the console more convenient.
/// </summary>
public static class Print
{
	public const string TYPE_TRACE = "Trace";
	public const string TYPE_INFO = "Info";
	public const string TYPE_WARN = "Warn";
	public const string TYPE_ERROR = "Error";
	public const string TYPE_UNKNOWN = "???";

	public static Dictionary<string, Logger> Loggers { get; set; } = [];

	/// <returns> An existing/new <see cref="Logger"/>. </returns>
	public static Logger GetLogger( string typeName )
	{
		typeName ??= TYPE_UNKNOWN;

		if ( (Loggers ??= []).TryGetValue( typeName, out var logger ) )
			return logger;

		return Loggers[typeName] = new( typeName );
	}

	/// <returns> An existing/new <see cref="Logger"/> using this object's type. </returns>
	public static Logger GetLogger( object source )
	{
		var typeName = source?.GetType()?.ToSimpleString( includeNamespace: false );

		if ( string.IsNullOrEmpty( typeName ) )
			typeName = TYPE_UNKNOWN;

		return GetLogger( typeName );
	}


	/// <summary>
	/// Converts the provided message to a <see cref="FormattableString"/>
	/// so that all references are clickable in the console. <br />
	/// </summary>
	/// <remarks>
	/// Probably not very optimized at all.
	/// </remarks>
	public static FormattableString Format( object[] message )
	{
		if ( message is null )
			return $"";

		var builder = new StringBuilder();

		for ( var i = 0; i < message.Length; i++ )
			builder.Append( $"{{{i}}}" );

		return FormattableStringFactory.Create( builder.ToString(), message );
	}


	/// <summary> A trace log using the specified logger. </summary>
	public static void LoggerTrace( string logger, FormattableString message )
		=> GetLogger( logger ).Trace( message );

	/// <summary> An info log using the specified logger(instead of "Info"). </summary>
	public static void LoggerInfo( string logger, FormattableString message )
		=> GetLogger( logger ).Info( message );

	/// <summary> A warning log using the specified logger(instead of "Warn"). </summary>
	public static void LoggerWarn( string logger, FormattableString message )
		=> GetLogger( logger ).Warning( message );

	/// <summary> An error log using the specified logger(instead of "Error"). </summary>
	public static void LoggerError( string logger, FormattableString message )
		=> GetLogger( logger ).Error( message );


	public static void Trace( FormattableString message )
		=> LoggerTrace( TYPE_TRACE, message );

	public static void Info( FormattableString message )
		=> LoggerInfo( TYPE_INFO, message );

	public static void Warn( FormattableString message )
		=> LoggerWarn( TYPE_WARN, message );

	public static void Error( FormattableString message )
		=> LoggerError( TYPE_ERROR, message );


	public static void Trace( params object[] message )
		=> Trace( Format( message ) );

	public static void Info( params object[] message )
		=> Info( Format( message ) );

	public static void Warn( params object[] message )
		=> Warn( Format( message ) );

	public static void Error( params object[] message )
		=> Error( Format( message ) );


	/// <summary>
	/// Prepends text in brackets to the log message with clickable reference support.
	/// </summary>
	public static void TraceFrom( FormattableString source, FormattableString message )
		=> Trace( $"[{source}] {message}" );

	/// <summary>
	/// Prepends text in brackets to the log message with clickable reference support.
	/// </summary>
	public static void InfoFrom( FormattableString source, FormattableString message )
		=> Info( $"[{source}] {message}" );

	/// <summary>
	/// Prepends text in brackets to the log message with clickable reference support.
	/// </summary>
	public static void WarnFrom( FormattableString source, FormattableString message )
		=> Warn( $"[{source}] {message}" );

	/// <summary>
	/// Prepends text in brackets to the log message with clickable reference support.
	/// </summary>
	public static void ErrorFrom( FormattableString source, FormattableString message )
		=> Error( $"[{source}] {message}" );


	/// <summary>
	/// Prepends a clickable reference of the object to the log message.
	/// </summary>
	/// <param name="source"> The object you'll be able to click on. </param>
	/// <param name="message"> 💬 </param>
	public static void TraceFrom( object source, FormattableString message )
		=> TraceFrom( $"{source}", message );

	/// <summary>
	/// Prepends a clickable reference of the object to the log message.
	/// </summary>
	/// <param name="source"> The object you'll be able to click on. </param>
	/// <param name="message"> 💬 </param>
	public static void InfoFrom( object source, FormattableString message )
		=> InfoFrom( $"{source}", message );

	/// <summary>
	/// Prepends a clickable reference of the object to the log message.
	/// </summary>
	/// <param name="source"> The object you'll be able to click on. </param>
	/// <param name="message"> 💬 </param>
	public static void WarnFrom( object source, FormattableString message )
		=> WarnFrom( $"{source}", message );

	/// <summary>
	/// Prepends a clickable reference of the object to the log message.
	/// </summary>
	/// <param name="source"> The object you'll be able to click on. </param>
	/// <param name="message"> 💬 </param>
	public static void ErrorFrom( object source, FormattableString message )
		=> ErrorFrom( $"{source}", message );


	/// <summary>
	/// Prepends a clickable reference of the object to the log message.
	/// </summary>
	/// <param name="source"> The object you'll be able to click on. </param>
	/// <param name="message"> 💬 </param>
	public static void TraceFrom( object source, params object[] message )
		=> TraceFrom( source, Format( message ) );

	/// <summary>
	/// Prepends a clickable reference of the object to the log message.
	/// </summary>
	/// <param name="source"> The object you'll be able to click on. </param>
	/// <param name="message"> 💬 </param>
	public static void InfoFrom( object source, params object[] message )
		=> InfoFrom( source, Format( message ) );

	/// <summary>
	/// Prepends a clickable reference of the object to the log message.
	/// </summary>
	/// <param name="source"> The object you'll be able to click on. </param>
	/// <param name="message"> 💬 </param>
	public static void WarnFrom( object source, params object[] message )
		=> WarnFrom( source, Format( message ) );

	/// <summary>
	/// Prepends a clickable reference of the object to the log message.
	/// </summary>
	/// <param name="source"> The object you'll be able to click on. </param>
	/// <param name="message"> 💬 </param>
	public static void ErrorFrom( object source, params object[] message )
		=> ErrorFrom( source, Format( message ) );


	/*
			GameObject Extensions
	*/


	public static void Log( this GameObject obj, FormattableString message = null )
		=> InfoFrom( $"{obj}", message );

	public static void Warn( this GameObject obj, FormattableString message = null )
		=> WarnFrom( $"{obj}", message );

	public static void Error( this GameObject obj, FormattableString message = null )
		=> ErrorFrom( $"{obj}", message );


	public static void Log( this GameObject obj, params object[] message )
		=> obj.Log( Format( message ) );

	public static void Warn( this GameObject obj, params object[] message )
		=> obj.Warn( Format( message ) );

	public static void Error( this GameObject obj, params object[] message )
		=> obj.Error( Format( message ) );


	/*
			Component Extensions
	*/


	public static void Log( this Component c, FormattableString message = null )
		=> InfoFrom( $"{c}", message );

	public static void Warn( this Component c, FormattableString message = null )
		=> WarnFrom( $"{c}", message );

	public static void Error( this Component c, FormattableString message = null )
		=> ErrorFrom( $"{c}", message );


	public static void Log( this Component c, params object[] message )
		=> c.Log( Format( message ) );

	public static void Warn( this Component c, params object[] message )
		=> c.Warn( Format( message ) );

	public static void Error( this Component c, params object[] message )
		=> c.Error( Format( message ) );


	/*
			GameResource Extensions
	*/


	public static void Log( this GameResource res, FormattableString message = null )
		=> InfoFrom( $"{res}", message );

	public static void Warn( this GameResource res, FormattableString message = null )
		=> WarnFrom( $"{res}", message );

	public static void Error( this GameResource res, FormattableString message = null )
		=> ErrorFrom( $"{res}", message );


	public static void Log( this GameResource res, params object[] message )
		=> res.Log( Format( message ) );

	public static void Warn( this GameResource res, params object[] message )
		=> res.Warn( Format( message ) );

	public static void Error( this GameResource res, params object[] message )
		=> res.Error( Format( message ) );


	/*
			Panel Extensions
	*/


	public static void Log( this Panel p, FormattableString message = null )
		=> InfoFrom( $"{p}", message );

	public static void Warn( this Panel p, FormattableString message = null )
		=> WarnFrom( $"{p}", message );

	public static void Error( this Panel p, FormattableString message = null )
		=> ErrorFrom( $"{p}", message );


	public static void Log( this Panel p, params object[] message )
		=> p.Log( Format( message ) );

	public static void Warn( this Panel p, params object[] message )
		=> p.Warn( Format( message ) );

	public static void Error( this Panel p, params object[] message )
		=> p.Error( Format( message ) );
}
