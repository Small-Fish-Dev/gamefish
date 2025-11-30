using System;

namespace GameFish;

partial class Server
{
	[Sync( SyncFlags.FromHost )]
	public NetDictionary<Guid, Identity> IdentityHistory { get; set; }

	/// <summary> A list of every active client in the scene. </summary>
	protected static IEnumerable<Client> AllClients => Instance?.Scene.GetAll<Client>() ?? [];

	/// <summary> A list of clients(including bots) guaranteed to be valid. </summary>
	public static IEnumerable<Client> ValidClients => AllClients.Where( cl => cl.IsValid() );

	/// <summary> A list of player-only clients guaranteed to be valid. </summary>
	public static IEnumerable<Client> PlayerClients => ValidClients.Where( cl => cl.IsPlayer );

	/// <summary> A list of valid player-only clients that are actively connected. </summary>
	public static IEnumerable<Client> ConnectedClients => PlayerClients.Where( cl => cl.Connected );

	/// <summary>
	/// Called when a new connection has become fully active on the server.
	/// </summary>
	protected virtual void OnJoined( Connection cn )
	{
		if ( cn is null )
			return;

		var cl = AssignClient( PlayerClientPrefab, cn );

		if ( !cl.IsValid() )
		{
			this.Warn( $"Connection:[{cn}] was spawned as invalid Client:[{cn}]" );
			Kick( cn, "Failed to create Client!" );
			return;
		}

		OnClientJoined( cn, cl );
	}

	/// <summary>
	/// Called after a client has fully connected and either found
	/// or has had its <see cref="Client"/> object created. <br />
	/// By default this creates a <see cref="PlayerPawnPrefab"/> for them.
	/// </summary>
	protected virtual void OnClientJoined( Connection cn, Client cl )
	{
		if ( !cl.IsValid() )
			return;

		if ( !cl.Pawn.IsValid() )
			cl.CreatePawn( PlayerPawnPrefab );
	}

	/// <summary>
	/// Called when a new <see cref="Client"/> object has been initially created.
	/// By default this shrimply assigns their connection to the component.
	/// </summary>
	protected virtual void OnClientCreated( Connection cn, Client cl )
	{
		if ( !cl.IsValid() )
			return;

		cl.AssignConnection( cn, out _ );
	}

	/// <summary>
	/// Called when a connection belonging to this client is destroyed.
	/// </summary>
	protected virtual void OnClientDisconnected( Connection cn, Client cl )
	{
		cl?.DestroyGameObject();
	}

	/// <summary>
	/// Cleans up all disconnected player clients.
	/// </summary>
	protected virtual void ValidateClients()
	{
		foreach ( var cl in ValidClients )
		{
			if ( cl.IsPlayer && cl?.Network?.Owner?.IsActive is not true )
				cl.DestroyGameObject();

			if ( !cl.Pawn.IsValid() )
				return;
		}
	}

	/// <summary>
	/// Finds an existing(or creates a new) <see cref="Client"/> object.
	/// </summary>
	protected virtual Client AssignClient( PrefabFile prefab, Connection cn = null )
		=> AssignClient<Client>( prefab, cn );

	/// <summary>
	/// Finds an existing(or creates a new) <typeparamref name="TClient"/>.
	/// </summary>
	protected virtual TClient AssignClient<TClient>( PrefabFile prefab, Connection cn = null ) where TClient : Client
	{
		if ( !Networking.IsHost )
		{
			this.Warn( $"tried to assign client prefab:[{prefab}] on non-host!" );
			return null;
		}

		if ( cn is null )
		{
			this.Warn( $"tried to assign a client to a null connection!" );
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

			OnClientCreated( cn, cl );
		}

		return cl as TClient;
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
