using System;

namespace GameFish;

/// <summary>
/// Tells you how long your code block took to execute.
/// </summary>
/// <remarks>
/// Made originally by ubre at Small Fish. <br />
/// <b> Usage: </b> <c>using ( CodeTimer( "Example" ) ) { ... }</c>
/// </remarks>
public sealed class CodeTimer : IDisposable
{
	public const string NAME_DEFAULT = "Timer";

	private readonly DateTime _started;
	private readonly string _name;

	public CodeTimer( string timerName = NAME_DEFAULT, bool startMessage = true )
	{
		_name = timerName ?? NAME_DEFAULT;
		_started = DateTime.Now;

		if ( startMessage )
			Print.InfoFrom( this, $"Starting {_name}..." );
	}

	public CodeTimer( string timerName, string startMessage )
	{
		_name = timerName;
		_started = DateTime.Now;

		if ( !startMessage.IsBlank() )
			Print.InfoFrom( this, startMessage );
	}

	public void Dispose()
	{
		Print.InfoFrom( this, $"{_name} took {(DateTime.Now - _started).TotalMilliseconds}ms" );
	}
}
