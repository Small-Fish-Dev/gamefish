using System.Text.Json.Serialization;

namespace GameFish;

partial class Server
{
	[Property, ReadOnly, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( SERVER ), Group( DEBUG ), Order( DEBUG_ORDER )]
	public List<Pawn> AllPawns => [.. Pawn.GetAll<Pawn>()];

	[Property, ReadOnly, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( SERVER ), Group( DEBUG ), Order( DEBUG_ORDER )]
	public List<Pawn> ActivePawns => [.. Pawn.GetAllActive<Pawn>()];

	/// <summary>
	/// Looks for a <see cref="Server"/> singleton to see if cheating is globally enabled.
	/// If one wasn't found it will check <see cref="Application.CheatsEnabled"/> instead.
	/// </summary>
	public static bool CheatsEnabled => Instance is var sv && sv.IsValid()
		? sv.IsCheatingEnabled is true
		: Application.CheatsEnabled;

	/// <summary>
	/// Allows custom logic to enable cheats(such as a sandbox mode).
	/// Defaults to checking <see cref="Application.CheatsEnabled"/>.
	/// </summary>
	public virtual bool IsCheatingEnabled => Application.CheatsEnabled;

	/// <returns> If this particular connection is allowed to cheat. </returns>
	public static bool CanCheat( Connection cn )
		=> Instance?.AllowCheating( cn ) is true;

	/// <returns> If this particular connection is allowed to cheat. </returns>
	public virtual bool AllowCheating( Connection cn )
		=> IsCheatingEnabled;
}
