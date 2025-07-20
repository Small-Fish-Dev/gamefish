using System;

namespace GameFish;

/// <summary>
/// The networking manager.
/// </summary>
[Icon( "dns" )]
public partial class Server : Singleton<Server>, Component.INetworkListener
{
	public const string FEATURE_DEBUG = "🐞 Debug";

	[Property, Feature( Agent.FEATURE_AGENT )]
	public PrefabFile PlayerClientPrefab { get; set; }

	[Property, Feature( Agent.FEATURE_AGENT )]
	public PrefabFile PlayerPawnPrefab { get; set; }

	[Sync( SyncFlags.FromHost )]
	public NetList<Client> ClientList { get; set; }

	protected static IEnumerable<Client> AllClients => Instance?.ClientList ?? [];
	public static IEnumerable<Client> ValidClients => AllClients.Where( cl => cl.IsValid() );
	public static IEnumerable<Client> PlayerClients => ValidClients.Where( cl => cl.IsPlayer );
	public static IEnumerable<Client> ConnectedClients => PlayerClients.Where( cl => cl.Connected );

	[Sync( SyncFlags.FromHost )]
	public NetDictionary<Guid, Identity> IdentityHistory { get; set; }

	/// <summary>
	/// Time in seconds since the Unix epoch.
	/// </summary>
	public static long Time => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

	protected override void OnStart()
	{
		base.OnStart();

		Networking.CreateLobby( new() );
	}

	public void OnActive( Connection cn )
	{
		if ( !Networking.IsHost )
		{
			this.Warn( $"{nameof( OnActive )} called on non-host!" );
			return;
		}

		if ( cn is null )
		{
			this.Warn( $"Null connection joined! [{cn}]" );
			return;
		}

		AssignClient( PlayerClientPrefab, cn );
	}

	public void OnDisconnected( Connection cn )
	{
		var cl = FindClient( cn );

		if ( cl.IsValid() )
			cl.DestroyGameObject();
	}

	/// <summary>
	/// Finds an existing(or creates and registers a new) <see cref="Client"/> object.
	/// </summary>
	protected virtual Client AssignClient( PrefabFile prefab, Connection cn = null )
		=> AssignClient<Client>( prefab, cn );

	/// <summary>
	/// Finds an existing(or creates and registers a new) <typeparamref name="TClient"/> object.
	/// </summary>
	protected virtual TClient AssignClient<TClient>( PrefabFile prefab, Connection cn = null ) where TClient : Client
	{
		if ( !Networking.IsHost )
		{
			this.Warn( $"tried to assign client prefab:[{prefab}] on non-host" );
			return null;
		}

		Client cl = FindClient( cn );

		// Create a new client if we couldn't find an existing one.
		if ( !cl.IsValid() )
		{
			if ( !prefab.TrySpawn( out var go ) )
			{
				this.Warn( $"Failed to spawn client prefab:[{prefab}]!" );
				return null;
			}

			cl = go.Components.Get<TClient>( FindMode.EverythingInSelf );

			if ( !cl.IsValid() )
			{
				this.Warn( $"Failed to find type:[{typeof( TClient )}] component on object:[{go}]!" );
				go.Destroy();

				return null;
			}

			cl.AssignConnection( cn, out _ );

			RegisterClient( cl );
		}

		// Spawn and assign their default pawn.
		if ( cl.IsValid() )
			cl.SetPawn( PlayerPawnPrefab, dropAll: true );

		return cl as TClient;
	}

	public static Client FindClient( Connection cn )
	{
		if ( cn is null )
			return null;

		var existing = ValidClients.FirstOrDefault( cl => cl.CompareConnection( cn ) );

		return existing;
	}

	protected virtual void RegisterClient( Client cl )
	{
		ClientList ??= [];

		if ( !cl.IsValid() || ClientList.Contains( cl ) )
			return;

		ClientList.Add( cl );

		ValidateClients();
	}

	protected virtual void ValidateClients()
	{
		if ( !Scene.IsValid() || ClientList is null )
			return;

		var toRemove = new List<Client>();

		foreach ( var cl in ClientList )
			if ( !cl.IsValid() || cl.Scene != Scene )
				toRemove.Add( cl );

		toRemove.ForEach( cl => { ClientList.Remove( cl ); cl.Destroy(); } );
	}

	/// <summary>
	/// Adds an <see cref="Identity"/> to the history by its connection Id.
	/// </summary>
	/// <param name="id"></param>
	public virtual void RegisterIdentity( ref Identity id )
	{
		if ( !id.IsValid() || id.Id == default )
			return;

		IdentityHistory ??= [];
		IdentityHistory[id.Id] = id;
	}
}
