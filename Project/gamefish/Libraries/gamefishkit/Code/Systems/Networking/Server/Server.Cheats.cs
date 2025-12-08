using System.Text.Json.Serialization;

namespace GameFish;

partial class Server
{
	protected const int CHEATING_ORDER = DEBUG_ORDER + 1;

	/// <summary>
	/// Does the server logic have cheating enabled?
	/// <br /> <br />
	/// <b> NOTE: </b> This is usually always on in-editor.
	/// </summary>
	[Title( "Enabled" )]
	[Property, ReadOnly, JsonIgnore]
	[Feature( DEBUG ), Group( CHEATING ), Order( CHEATING_ORDER )]
	protected bool InspectorCheatsEnabled => CheatsEnabled;

	/// <summary>
	/// Allows custom logic to enable cheats(such as a sandbox mode).
	/// Defaults to checking <see cref="Application.CheatsEnabled"/>.
	/// </summary>
	public virtual bool CheatsEnabled => Application.CheatsEnabled;

	/// <summary>
	/// Looks for a <see cref="Server"/> singleton to see if cheating is globally enabled.
	/// If one wasn't found it will check <see cref="Application.CheatsEnabled"/> instead.
	/// </summary>
	public static bool IsCheatingEnabled()
	{
		if ( TryGetInstance( out var sv ) )
			return sv.CheatsEnabled;

		return Application.CheatsEnabled;
	}

	/// <returns> If this particular connection is allowed to cheat. </returns>
	public static bool IsCheatingEnabled( Connection cn )
	{
		if ( TryGetInstance( out var sv ) )
			return sv.CanCheat( cn );

		// Default to global cheats being enabled.
		return IsCheatingEnabled();
	}

	/// <returns> If this particular connection is allowed to cheat. </returns>
	public virtual bool CanCheat( Connection cn )
		=> IsCheatingEnabled();
}
