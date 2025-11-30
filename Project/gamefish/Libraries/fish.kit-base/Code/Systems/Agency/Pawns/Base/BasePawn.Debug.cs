using System.Text.Json.Serialization;

namespace GameFish;

partial class BasePawn
{
	/// <summary>
	/// Is this controlled by a player agent?
	/// </summary>
	[Title( "Is Player" )]
	[Property, ReadOnly, JsonIgnore]
	[Feature( PAWN ), Group( DEBUG ), Order( DEBUG_ORDER )]
	public bool InspectorIsPlayer => IsPlayer;

	/// <summary>
	/// The agent controlling this pawn. Could be a player or an NPC.
	/// </summary>
	[Title( "Owner" )]
	[Property, JsonIgnore]
	[Feature( PAWN ), Group( DEBUG ), Order( DEBUG_ORDER )]
	public Agent InspectorOwner
	{
		get => Owner;
		set => Owner = value;
	}
}
