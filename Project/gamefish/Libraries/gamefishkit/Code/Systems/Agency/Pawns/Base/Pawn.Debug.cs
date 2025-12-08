using System.Text.Json.Serialization;

namespace GameFish;

partial class Pawn
{
	/// <summary>
	/// Log ownership changes and such?
	/// </summary>
	[Property]
	[Title( "Logging" )]
	[Feature( PAWN ), Group( DEBUG ), Order( DEBUG_ORDER )]
	public bool DebugLogging { get; set; }

	/// <summary>
	/// Is this controlled by a player agent?
	/// </summary>
	[Title( "Is Player" )]
	[Property, ReadOnly, JsonIgnore]
	[Feature( PAWN ), Group( DEBUG ), Order( DEBUG_ORDER )]
	protected bool InspectorIsPlayer => IsPlayer;

	/// <summary>
	/// The agent controlling this pawn. Could be a player or an NPC.
	/// </summary>
	[Title( "Owner" )]
	[Property, JsonIgnore]
	[Feature( PAWN ), Group( DEBUG ), Order( DEBUG_ORDER )]
	protected Agent InspectorOwner
	{
		get => Owner;
		set => Owner = value;
	}
}
