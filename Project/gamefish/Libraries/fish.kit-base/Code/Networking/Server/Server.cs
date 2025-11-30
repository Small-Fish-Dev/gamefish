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
	protected override bool? IsNetworkedOverride => true;

	/// <summary>
	/// Shows if <see cref="Networking.IsActive"/> is <c>true</c>.
	/// </summary>
	[Property]
	[Feature( SERVER ), Group( DEBUG ), Order( DEBUG_ORDER )]
	public bool IsNetworkingActive => Networking.IsActive;

	[Property, Title( "Client Prefab" )]
	[Feature( SERVER ), Group( AGENT ), Order( -1 )]
	public PrefabFile PlayerClientPrefab { get; set; }

	[Feature( SERVER ), Group( PAWN )]
	[Property, Title( "Player Prefab" )]
	public virtual PrefabFile PlayerPawnPrefab { get; set; }

	[Feature( SERVER ), Group( PAWN )]
	[Property, Title( "Spectator Prefab" )]
	public virtual PrefabFile SpectatorPawnPrefab { get; set; }

	/// <summary>
	/// Time in seconds since the Unix epoch.
	/// </summary>
	public static long Time => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

	/// <summary>
	/// Decides if a networked lobby should be automatically opened.
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

		OnJoined( cn );
	}

	public virtual void OnDisconnected( Connection cn )
	{
		if ( TryFindClient( cn, out var cl ) )
			OnClientDisconnected( cn, cl );
	}
}
