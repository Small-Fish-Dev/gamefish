using System;
using System.Text.Json.Serialization;

namespace GameFish;

partial class DataFile<TDataComp, TDataClass>
{
	protected const int SET_ORDER = ORDER + 9;

	/// <summary> Print debug logs? </summary>
	[Feature( SAVING ), Order( ORDER )]
	[Property, Group( DEBUG ), Title( "Logging" )]
	public bool DebugLogging { get; set; } = false;

	[JsonIgnore]
	[Header( "Set" )]
	[Feature( SAVING ), Order( SET_ORDER )]
	[Property, Group( DEBUG ), Title( "Key" )]
	protected string DebugSetKey { get; set; }

	[JsonIgnore]
	[Feature( SAVING ), Order( SET_ORDER )]
	[Property, Group( DEBUG ), Title( "Type" )]
	protected Type DebugSetType { get; set; }

	[JsonIgnore]
	[Feature( SAVING ), Order( SET_ORDER )]
	[Property, Group( DEBUG ), Title( "Value" )]
	protected string DebugSetValue { get; set; }

	[Feature( SAVING ), Order( SET_ORDER )]
	[Button, Group( DEBUG ), Title( "Set Value" )]
	protected void DebugSet()
	{
		try
		{
			Set( DebugSetKey, Json.Deserialize( DebugSetValue, DebugSetType ) );
		}
		catch ( Exception e )
		{
			this.Warn( e );
		}
	}

	/// <summary>
	/// Logs to console if <see cref="DebugLogging"/> is enabled.
	/// </summary>
	protected void DebugLog( FormattableString log )
	{
		if ( DebugLogging )
			this.Log( log );
	}

	/// <summary>
	/// Warns in console if <see cref="DebugLogging"/> is enabled.
	/// </summary>
	protected void DebugWarn( FormattableString log )
	{
		if ( DebugLogging )
			this.Warn( log );
	}
}
