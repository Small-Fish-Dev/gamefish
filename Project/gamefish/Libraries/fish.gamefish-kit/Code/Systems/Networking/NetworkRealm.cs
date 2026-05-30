namespace GameFish;

/// <summary>
/// Allows you to easily know/decide if logic is meant
/// to run depending on if you're the host or a client.
/// </summary>
[Icon( "🌐" )]
[Group( Library.NAME )]
[DefaultValue( Owner )]
public enum NetworkRealm
{
	/// <summary>
	/// The lobby's owner.
	/// </summary>
	[Icon( "👮" )]
	Host = 0,

	/// <summary>
	/// All actively connected non-hosts.
	/// </summary>
	[Icon( "👨‍💼" )]
	Clients = 1,

	/// <summary>
	/// Whoever the object belongs to.
	/// </summary>
	[Icon( "🔑" )]
	Owner = 2,

	/// <summary>
	/// Everyone in the lobby.
	/// </summary>
	[Icon( "🤝" )]
	Shared = 3,
}

partial class Library
{
	/// <summary>
	/// Allows you to easily know/decide if logic is meant
	/// to run depending on if you're the host or a client.
	/// </summary>
	/// <returns> If the object is meant to run in that network realm. </returns>
	public static bool InRealm( this NetworkRealm realm, object source )
	{
		return source switch
		{
			GameObject go => InRealm( go, in realm ),
			Component c => InRealm( c, in realm ),
			_ => false,
		};
	}

	public static bool InRealm( this GameObject.NetworkAccessor net, in NetworkRealm realm )
	{
		if ( net is null )
			return false;

		return realm switch
		{
			NetworkRealm.Host => Networking.IsHost,
			NetworkRealm.Clients => !Networking.IsHost,
			NetworkRealm.Owner => !net.IsProxy,
			NetworkRealm.Shared => true,
			_ => false,
		};
	}

	/// <inhereitdoc cref="InRealm(GameObject.NetworkAccessor, in NetworkRealm)" />
	public static bool InRealm( this GameObject obj, in NetworkRealm realm )
		=> obj?.Network?.InRealm( in realm ) is true;

	/// <inhereitdoc cref="InRealm(GameObject.NetworkAccessor, in NetworkRealm)" />
	public static bool InRealm( this Component c, in NetworkRealm realm )
		=> c?.Network?.InRealm( in realm ) is true;
}
