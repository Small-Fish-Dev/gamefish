using System.Text.Json.Serialization;

namespace GameFish;

partial class Agent
{
	/// <summary>
	/// What specific pawn(if any) is under this agent's control?
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	public Pawn Pawn
	{
		get => _pawn;

		protected set
		{
			var oldPawn = _pawn;
			_pawn = value;

			OnSetPawn( newPawn: _pawn, oldPawn: oldPawn );
		}
	}

	protected Pawn _pawn;

	[Title( "Pawn" )]
	[Property, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( AGENT ), Group( DEBUG )]
	protected Pawn InspectorPawn
	{
		get => Pawn;
		set => TryAssignPawn( value );
	}

	protected virtual void OnSetPawn( Pawn newPawn, Pawn oldPawn = null )
	{
		if ( newPawn == oldPawn )
			return;

		if ( oldPawn.IsValid() )
			OnPawnLost( oldPawn );

		if ( newPawn.IsValid() )
			OnPawnTaken( newPawn );
	}

	/// <summary>
	/// Called when a pawn we owned was removed.
	/// </summary>
	protected virtual void OnPawnLost( Pawn pawn )
	{
	}

	/// <summary>
	/// Called when a new pawn has been acquired.
	/// </summary>
	protected virtual void OnPawnTaken( Pawn pawn )
	{
	}

	/// <summary>
	/// Called by the host to register a pawn assigned to this agent.
	/// </summary>
	public virtual bool TryAssignPawn( Pawn pawn )
	{
		if ( !Networking.IsHost )
			return false;

		if ( !this.IsValid() || !Scene.IsValid() )
			return false;

		if ( !pawn.IsValid() || !pawn.AllowOwnership( this ) )
			return false;

		var oldPawn = Pawn;

		if ( oldPawn.IsValid() && !oldPawn.TryDropOwner( this ) )
			return false;

		if ( !pawn.TryAssignOwner( this ) )
			return false;

		Pawn = pawn;

		this.Log( $"Added pawn:[{pawn}]" );

		return true;
	}

	/// <summary>
	/// Tries to drop a specific pawn.
	/// </summary>
	/// <returns> If we owned that pawn and it was dropped. </returns>
	public bool TryDropPawn( Pawn pawn )
	{
		if ( !Networking.IsHost )
			return false;

		// Only one pawn, so if that's not it then uhh..
		if ( Pawn != pawn )
			return false;

		if ( pawn is not null )
			if ( !pawn.TryDropOwner( this ) )
				return false;

		Pawn = null;

		return true;
	}

	/// <summary>
	/// Spawns a <see cref="GameFish.Pawn"/> prefab and assigns it to this agent.
	/// </summary>
	/// <param name="prefab"></param>
	public Pawn CreatePawn( PrefabFile prefab )
		=> CreatePawn<Pawn>( prefab );

	/// <summary>
	/// Spawns a <typeparamref name="TPawn"/> prefab and assigns it to this agent.
	/// </summary>
	/// <param name="prefab"></param>
	public TPawn CreatePawn<TPawn>( PrefabFile prefab ) where TPawn : Pawn
	{
		if ( !Networking.IsHost )
		{
			this.Warn( $"tried to spawn/set pawn prefab:[{prefab}] on non-host" );
			return null;
		}

		if ( !prefab.IsValid() )
		{
			this.Warn( $"tried to set null pawn prefab of type:[{typeof( TPawn )}]" );
			return null;
		}

		var spawnPoint = FindSpawnPoint();

		if ( !spawnPoint.HasValue )
		{
			this.Warn( $"failed to find valid spawn point when setting pawn prefab:[{prefab}]" );
			return null;
		}

		if ( !prefab.TrySpawn( spawnPoint.Value.WithScale( Vector3.One ), out var go ) )
			return null;

		return SetPawn<TPawn>( go, failDestroy: true );
	}

	/// <summary>
	/// Assigns a pawn to this agent from an existing object.
	/// </summary>
	/// <param name="go"> The <see cref="GameObject"/> with <typeparamref name="TPawn"/> on it. </param>
	/// <param name="failDestroy"> Destroy the object upon failure? </param>
	public TPawn SetPawn<TPawn>( GameObject go, bool failDestroy = false ) where TPawn : Pawn
	{
		if ( !Networking.IsHost )
		{
			this.Warn( $"tried to set pawn object:[{go}] on non-host" );
			return null;
		}

		if ( !go.IsValid() )
		{
			this.Warn( $"tried to set pawn as invalid object:[{go}]" );
			return null;
		}

		var failed = false;
		var newPawn = go.Components.Get<TPawn>( true );

		if ( !newPawn.IsValid() )
		{
			failed = true;
			this.Warn( $"failed to find type:[{typeof( TPawn )}] on object:[{go}]" );
		}

		if ( !TryAssignPawn( newPawn ) )
		{
			failed = true;
			this.Warn( $"failed to set pawn:[{newPawn}]" );
		}

		if ( failed && failDestroy )
		{
			this.Warn( $"failure detected. destroying pawn object:[{go}]" );
			go.Destroy();

			return null;
		}

		return newPawn;
	}

	/// <summary>
	/// Sends a request to the host to take a pawn.
	/// </summary>
	public AttemptStatus RequestTakePawn( Pawn pawn )
	{
		if ( !pawn.IsValid() || !pawn.AllowOwnership( this ) )
			return AttemptStatus.Failure;

		RpcRequestTakePawn( pawn );

		return AttemptStatus.Active;
	}

	[Rpc.Host( NetFlags.Reliable | NetFlags.OwnerOnly )]
	protected void RpcRequestTakePawn( Pawn pawn )
	{
		TryTakePawn( pawn );
	}

	public virtual AttemptStatus TryTakePawn( Pawn pawn )
	{
		AttemptStatus Result( in AttemptStatus result )
		{
			RpcTryTakePawnHostResponse( pawn, result );
			return result;
		}

		// Only hosts can process pawn takeover attempts.
		if ( !Network.IsOwner )
			return Result( AttemptStatus.Active );

		if ( !Connected )
			return Result( AttemptStatus.Failure );

		if ( !pawn.IsValid() )
			return Result( AttemptStatus.Failure );

		if ( !pawn.TryAssignOwner( this ) )
			return Result( AttemptStatus.Failure );

		return Result( AttemptStatus.Success );
	}

	/// <summary>
	/// This is the method you want to call so the host can tell the owner what happened.
	/// </summary>
	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	protected void RpcTryTakePawnHostResponse( Pawn pawn, AttemptStatus result )
	{
		if ( pawn.IsValid() )
			OnTryTakePawnResponse( pawn, result );
	}

	protected virtual void OnTryTakePawnResponse( Pawn pawn, in AttemptStatus result )
	{
	}
}
