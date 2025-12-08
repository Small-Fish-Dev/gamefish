using System;

namespace GameFish;

/// <summary>
/// The networking manager.
/// It spawns players and manages clients.
/// <br /> <br />
/// <b> NOTE: </b> You are encouraged to inherit and override this component.
/// </summary>
[Icon( "dns" )]
public partial class Server : Singleton<Server>, Component.INetworkListener
{
	protected const int SERVER_ORDER = DEBUG_ORDER - 1000;

	protected const int AGENTS_ORDER = SERVER_ORDER - 10;
	protected const int PAWNS_ORDER = SERVER_ORDER - 5;

	/// <summary>
	/// The prefab with a <see cref="Client"/> component on its root.
	/// </summary>
	[Property]
	[Title( "Client" )]
	[Feature( SERVER ), Group( AGENTS ), Order( AGENTS_ORDER )]
	public PrefabFile PlayerClientPrefab { get; set; }

	/// <summary>
	/// The fallback prefab with a <see cref="Pawn"/> component on it.
	/// <br /> <br />
	/// <b> NOTE: </b> This is typically used only as a last resort such
	/// as if there is no active game manager/state available.
	/// </summary>
	[Property]
	[Title( "Default" )]
	[Feature( SERVER ), Group( PAWNS ), Order( PAWNS_ORDER )]
	public PrefabFile DefaultPawnPrefab { get; set; }

	/// <summary>
	/// Time in seconds since the Unix epoch.
	/// </summary>
	public static long Time => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

	/// <summary>
	/// If enabled: a networked lobby should auto-open.
	/// </summary>
	public virtual bool AutoStart => true;

	protected override void OnStart()
	{
		base.OnStart();

		if ( AutoStart )
			Open();
	}

	public virtual void OnActive( Connection cn )
	{
		if ( !Networking.IsHost )
		{
			this.Warn( $"{nameof( OnActive )} called on non-host!" );
			return;
		}

		if ( cn is null )
		{
			this.Warn( $"Null connection joined: [{cn}]" );
			return;
		}

		OnConnected( cn );
	}

	public virtual void OnDisconnected( Connection cn )
	{
		if ( TryFindClient( cn, out var cl ) )
			OnClientDisconnected( cn, cl );
	}
}
