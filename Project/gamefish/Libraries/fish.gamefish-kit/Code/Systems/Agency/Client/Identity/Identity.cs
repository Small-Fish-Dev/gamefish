using System;

namespace GameFish;

/// <summary>
/// Networkable identifying client information for data/session persistence. <br />
/// In other words: the key used to remember stuff about clients between sessions.
/// </summary>
public partial struct Identity : IValid
{
	/// <returns> If the <see cref="Type"/> was properly configured. </returns>
	public readonly bool IsValid => Type is not ClientType.Invalid;

	public long Created { get; private set; } = Server.Time;

	/// <summary>
	/// Is this client a player or a bot?
	/// </summary>
	public ClientType Type { get; private set; } = ClientType.Invalid;

	/// <summary>
	/// The component that owns this identity.
	/// </summary>
	public Client Client { get; private set; }

	/// <summary>
	/// The last known display name of the client.
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// The identification key. <br />
	/// Probably from a user's connection.
	/// </summary>
	public Guid Id { get; private set; }

	/// <summary> Their Steam ID. Might be for a fake client. </summary>
	public SteamId SteamId { get; private set; }

	/// <summary> The user's IP. Used to find this identity after rejoining. </summary>
	public string Address { get; private set; }

	/// <summary>
	/// The current instance of a user's connection.
	/// </summary>
	public Connection Connection
	{
		readonly get => Type is ClientType.User ? Connection.Find( Id ) : null;
		private set
		{
			Id = value?.Id ?? Id;
			Address = value?.Address;
			Name = value?.DisplayName ?? Name;
			SteamId = value?.SteamId ?? default;
		}
	}

	public readonly bool CompareConnection( Connection cn )
	{
		if ( cn is null )
			return false;

		return Connection == cn;
	}

	public override readonly string ToString() => $"Identity|{Type}|\"{Name}\"";

	public Identity() { }

	public Identity( Client cl, Connection cn )
	{
		if ( !cl.IsValid() )
		{
			Log.Warning( $"{this} was created with an invalid client (cn: {cn?.ToString() ?? "null"}, client:{Client})" );
			return;
		}

		Type = cl.IsPlayer ? ClientType.User : ClientType.Bot;

		Client = cl;
		Connection = cn;

		Server.Instance?.RegisterIdentity( ref this );

		Print.InfoFrom( this, "created" );
	}

	public override readonly int GetHashCode() => HashCode.Combine( Created.GetHashCode(), Id.GetHashCode(), Name?.GetHashCode() );

	public static bool operator ==( Identity a, Identity b ) => a.IsValid() && (a.Id == b.Id || a.SteamId == b.SteamId);
	public static bool operator !=( Identity a, Identity b ) => !(a == b);

	public override readonly bool Equals( object obj )
	{
		if ( obj is Identity id )
			return this == id;

		return base.Equals( obj );
	}
}
