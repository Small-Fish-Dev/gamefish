using System.Text.Json.Serialization;

namespace GameFish;

partial class Server
{
	protected const int DEBUG_CLIENTS_ORDER = DEBUG_ORDER + 10;
	protected const int DEBUG_PAWNS_ORDER = DEBUG_ORDER + 20;

	/// <summary>
	/// Shows if <see cref="Networking.IsActive"/> is <c>true</c>.
	/// </summary>
	[Property]
	[Feature( DEBUG ), Group( NETWORKING ), Order( DEBUG_ORDER )]
	public bool IsNetworkingActive => Networking.IsActive;

	[Title( "Clients" )]
	[Property, ReadOnly, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( DEBUG ), Group( CLIENTS ), Order( DEBUG_CLIENTS_ORDER )]
	protected List<Client> InspectorAllClients => [.. AllClients];

	[Title( "Pawns" )]
	[Property, ReadOnly, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( DEBUG ), Group( PAWNS ), Order( DEBUG_PAWNS_ORDER )]
	protected List<Pawn> InspectorActivePawns => [.. Pawn.GetAllActive()];

	/// <summary>
	/// Cleans up all disconnected player clients.
	/// </summary>
	[Button]
	[Feature( DEBUG ), Group( CLIENTS ), Order( DEBUG_CLIENTS_ORDER - 1 )]
	public virtual void ValidateClients()
	{
		foreach ( var cl in PlayerClients.ToArray() )
			if ( !cl.Connected )
				cl.DestroyGameObject();
	}
}
