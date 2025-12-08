namespace GameFish;

partial class GameState
{
	protected const int PAWNS_PREFABS_ORDER = PAWNS_ORDER;

	/// <summary>
	/// The prefab to spawn for looking aroun and shit like that.
	/// </summary>
	[Property]
	[Title( "Spectator" )]
	[Feature( PAWNS ), Group( PREFABS ), Order( PAWNS_PREFABS_ORDER )]
	public virtual PrefabFile SpectatorPawnPrefab { get; set; }

	/// <summary>
	/// The prefab to spawn for a living player to use.
	/// </summary>
	[Property]
	[Title( "Player" )]
	[Feature( PAWNS ), Group( PREFABS ), Order( PAWNS_PREFABS_ORDER )]
	public virtual PrefabFile PlayerPawnPrefab { get; set; }

	/// <summary>
	/// Called on the host for each valid newly created client.
	/// <br /> <br />
	/// <b> NOTE: </b> This is a good time to give them a pawn.
	/// </summary>
	public virtual void OnClientSpawned( Client cl )
	{
		// If we're not in a menu then spawn them as a player by default.
		if ( IsPlaying )
		{
			if ( TryAssignPlayer( cl, out _, force: true, oldCleanup: true ) )
				return;
		}

		// Fallback to being a spectator.
		TryAssignSpectator( cl, out _, force: true, oldCleanup: true );
	}

	/// <summary>
	/// Ensures the client has a pawn for spectating.
	/// </summary>
	/// <param name="cl"> The ghost-to-be. </param>
	/// <param name="pawn"> The resulting pawn component(if any). </param>
	/// <param name="force"> Respawn them even if they're a spectator? </param>
	/// <param name="oldCleanup"> If they had a previous pawn should we always destroy it? </param>
	/// <returns> If they are a spectator now. </returns>
	public virtual bool TryAssignSpectator( Client cl, out SpectatorPawn pawn, bool force = false, bool oldCleanup = true )
	{
		pawn = null;

		if ( !Networking.IsHost || !cl.IsValid() )
			return false;

		if ( !force && (cl.Pawn as SpectatorPawn).IsValid() )
			return true;

		if ( !SpectatorPawnPrefab.IsValid() )
			return false;

		return TrySetPawn( cl, SpectatorPawnPrefab, out pawn, oldCleanup: oldCleanup );
	}

	/// <summary>
	/// Attempts to ensures the client has a playable pawn.
	/// </summary>
	/// <param name="cl"> The player-to-be. </param>
	/// <param name="pawn"> The resulting pawn component(if any). </param>
	/// <param name="force"> Respawn them even if they're a spectator? </param>
	/// <param name="oldCleanup"> If they had a previous pawn should we always destroy it? </param>
	/// <returns> If they have a playable pawn now. </returns>
	public virtual bool TryAssignPlayer( Client cl, out Pawn pawn, bool force = false, bool oldCleanup = true )
	{
		pawn = null;

		if ( !Networking.IsHost || !cl.IsValid() )
			return false;

		if ( !force && cl.Pawn.IsValid() && cl.Pawn is not SpectatorPawn )
			return true;

		if ( !PlayerPawnPrefab.IsValid() )
			return false;

		return TrySetPawn( cl, PlayerPawnPrefab, out pawn, oldCleanup: oldCleanup );
	}

	/// <summary>
	/// Attempts to assign a new pawn.
	/// </summary>
	/// <param name="agent"> The suit changer(player/NPC). </param>
	/// <param name="prefab"> The prefab with a <see cref="Pawn"/> component. </param>
	/// <param name="pawn"> The resulting pawn component(if any). </param>
	/// <param name="oldCleanup"> If they had a previous pawn should we always destroy it? </param>
	/// <returns> If the new pawn could be assigned. </returns>
	public virtual bool TrySetPawn( Agent agent, PrefabFile prefab, out Pawn pawn, bool oldCleanup = true )
		=> TrySetPawn<Pawn>( agent, prefab, out pawn, oldCleanup );

	/// <summary>
	/// Attempts to assign a new <typeparamref name="TPawn"/>.
	/// </summary>
	/// <param name="agent"> The suit changer(player/NPC). </param>
	/// <param name="prefab"> The prefab with a <see cref="Pawn"/> component. </param>
	/// <param name="pawn"> The resulting <typeparamref name="TPawn"/>(if any). </param>
	/// <param name="oldCleanup"> If they had a previous pawn should we always destroy it? </param>
	/// <returns> If the new <typeparamref name="TPawn"/> could be assigned. </returns>
	public virtual bool TrySetPawn<TPawn>( Agent agent, PrefabFile prefab, out TPawn pawn, bool oldCleanup = true )
		where TPawn : Pawn
	{
		pawn = null;

		if ( !Networking.IsHost || !agent.IsValid() )
			return false;

		if ( !prefab.IsValid() )
		{
			this.Warn( $"Tried to set invalid prefab:[{prefab}] on agent:[{agent}]." );
			return false;
		}

		var oldPawn = agent.Pawn;

		pawn = agent.SetPawnFromPrefab<TPawn>( prefab );

		if ( !pawn.IsValid() )
			return false;

		if ( oldCleanup && oldPawn.IsValid() )
			oldPawn.DestroyGameObject();

		return true;
	}

	/// <summary>
	/// Decides where pawns should spawn.
	/// </summary>
	/// <param name="agent"> Probably a <see cref="Client"/>. </param>
	/// <returns> Where they should spawn(or null). </returns>
	public virtual Transform? FindSpawnPoint( Agent agent )
		=> null;
}
