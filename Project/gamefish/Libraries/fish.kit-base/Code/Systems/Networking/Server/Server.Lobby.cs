using System;
using Sandbox.Network;

namespace GameFish;

partial class Server
{
	public virtual string DefaultKickReason { get; set; } = "Kicked by the lobby host.";
	public virtual string ShutdownKickReason { get; set; } = "Shutting down.";

	/// <summary>
	/// Initializes the server(if possible).
	/// By default this starts a new lobby, but you can prevent this.
	/// </summary>
	public virtual void Open()
	{
		if ( !Networking.IsHost )
			return;

		if ( Networking.IsActive )
		{
			this.Warn( "Tried to open a lobby while already in one." );
			return;
		}

		TryCreateLobby();
	}

	/// <summary>
	/// Disconnects the server.
	/// Optionally kicks all users first.
	/// </summary>
	/// <param name="kickAll"> Kick all players beforehand? </param>
	/// <param name="kickReason"> The reason to display(instead of default) if kicking. </param>
	public virtual void Close( bool kickAll = false, string kickReason = null )
	{
		if ( !Networking.IsHost )
			return;

		if ( kickAll )
		{
			try
			{
				KickAll( kickReason ?? ShutdownKickReason );
			}
			catch ( Exception e )
			{
				this.Warn( $"{nameof( Close )} {nameof( KickAll )} exception: {e}" );
			}
		}

		try
		{
			Networking.Disconnect();
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( Close )} network disconnect exception: {e}" );
		}

		OnShutdown();
	}

	/// <summary>
	/// Allows you to configure the next lobby created. <br />
	/// This is a good place to apply user settings.
	/// </summary>
	/// <param name="context"> What kind of lobby? Could be an enum or a string. </param>
	/// <param name="privacy"> The privacy level. Typically defaults to public. </param>
	/// <returns> The new lobby's settings. </returns>
	public virtual LobbyConfig GetLobbyConfig( object context = null, LobbyPrivacy? privacy = null )
	{
		var cfg = new LobbyConfig();

		if ( privacy.HasValue )
			cfg.Privacy = privacy.Value;

		return cfg;
	}

	/// <summary>
	/// Attempts to start a new lobby of some kind.
	/// </summary>
	/// <remarks>
	/// Will use <see cref="GetLobbyConfig"/> if <paramref name="cfgOverride"/> is not specified.
	/// </remarks>
	/// <param name="context"> What kind of lobby? Could be an enum or a string. </param>
	/// <param name="cfgOverride"> The configuration to force. </param>
	/// <returns> If the new lobby could be created. </returns>
	public virtual bool TryCreateLobby( object context = null, LobbyConfig? cfgOverride = null )
	{
		if ( Networking.IsActive )
		{
			this.Warn( "Lobby creation failed: must close the active lobby first." );
			return false;
		}

		var cfg = cfgOverride ?? GetLobbyConfig();

		Networking.CreateLobby( cfg );

		return true;
	}

	protected virtual void OnShutdown()
	{
		// Dedicated servers should never hang.
		if ( Application.IsDedicatedServer )
			Shutdown();
	}

	/// <summary>
	/// Safely closes the game/server.
	/// </summary>
	protected void Shutdown()
	{
		try
		{
			Game.Close();
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( Shutdown )} exception: {e}" );
		}
	}

	/// <summary>
	/// Safely kicks a connection for the specified(or default) reason.
	/// </summary>
	public virtual void Kick( Connection cn, string reason = null )
	{
		if ( !Networking.IsHost || cn is null )
			return;

		reason ??= DefaultKickReason;

		try
		{
			cn.Kick( reason );
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( Kick )} exception: {e}" );
		}
	}

	/// <summary>
	/// Kicks every non-host connection for the specified(or default) reason.
	/// Only works for the lobby host.
	/// </summary>
	/// <param name="reason"></param>
	public virtual void KickAll( string reason = null )
	{
		if ( !Networking.IsHost )
			return;

		reason ??= DefaultKickReason;

		foreach ( var cn in Connection.All?.ToArray() ?? [] )
		{
			if ( cn is null || cn.IsHost || cn == Connection.Local )
				continue;

			Kick( cn, reason );
		}
	}
}
