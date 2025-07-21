namespace GameFish;

/// <summary>
/// Something capable of control over other objects. <br />
/// It may be a player(real/fake) or an NPC.
/// </summary>
[Icon( "psychology" )]
[EditorHandle( Icon = "psychology" )]
public abstract partial class Agent : Component
{
	public const string FEATURE_AGENT = "🧠 Agent";
	public const string GROUP_ID = "🆔 Identity";

	/// <summary>
	/// Is this meant to be owned by a player?
	/// </summary>
	[Property, Feature( FEATURE_AGENT )]
	public virtual bool IsPlayer { get; } = false;

	/// <summary>
	/// Which pawns are known to be under this agent's control?
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	[Property, ReadOnly, Feature( FEATURE_AGENT )]
	public NetList<BasePawn> Pawns { get; set; }

	public virtual Identity Identity { get; protected set; }
	public virtual Connection Connection => Connection.Host;

	/// <summary>
	/// If NPC/Bot: always true. ('cause they in the matrix or some shit) <br />
	/// If <see cref="Client"/>: if the connection exists and is active.
	/// </summary>
	public virtual bool Connected => true;

	/// <summary>
	/// If NPC/Bot: always false. <br />
	/// If Client: if our <see cref="Identity"/> has the specified connection.
	/// </summary>
	public virtual bool CompareConnection( Connection cn )
		=> false;

	// public override string ToString()
	// => $"{GetType().ToSimpleString( includeNamespace: false )}";

	/// <summary>
	/// The display name of this guy/gal/whatever.
	/// </summary>
	public virtual string Name
		=> GetType().ToSimpleString();

	protected override void OnUpdate()
	{
		base.OnUpdate();

		SimulatePawns( Time.Delta );
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		UpdateNetworking();
	}

	public virtual void UpdateNetworking()
	{
		if ( !Networking.IsHost )
			return;

		GameObject?.NetworkSetup(
			cn: Connection,
			orphanMode: NetworkOrphaned.Destroy,
			ownerTransfer: OwnerTransfer.Fixed,
			netMode: NetworkMode.Object,
			ignoreProxy: false
		);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		DropAllPawns();
	}

	public void DropAllPawns()
	{
		if ( !Networking.IsHost || Pawns is null )
			return;

		// rndtrash: another place where the "Collection was modified" exception occurs
		foreach ( var pawn in Pawns.ToList() )
			if ( pawn.IsValid() && pawn.Agent == this )
				pawn.Agent = null;

		Pawns?.Clear();
	}

	/// <returns> A random default spawn point's transform(if any). </returns>
	public virtual Transform? GetSpawnPoint()
	{
		return Game.Random.FromArray( Scene?.GetAll<SpawnPoint>()?.ToArray() )
			?.WorldTransform.WithScale( 1f );
	}

	/// <summary>
	/// Spawns a <see cref="BasePawn"/> prefab and assigns it to this agent.
	/// </summary>
	/// <param name="prefab"></param>
	/// <param name="dropAll"> Set this as the only owned pawn? </param>
	public BasePawn SetPawn( PrefabFile prefab, bool dropAll = true )
		=> SetPawn<BasePawn>( prefab, dropAll: dropAll );

	/// <summary>
	/// Spawns a <typeparamref name="TPawn"/> prefab and assigns it to this agent.
	/// </summary>
	/// <param name="prefab"></param>
	/// <param name="dropAll"> Set this as the only owned pawn? </param>
	public TPawn SetPawn<TPawn>( PrefabFile prefab, bool dropAll = true ) where TPawn : BasePawn
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

		var spawnPoint = GetSpawnPoint();

		if ( !spawnPoint.HasValue )
		{
			this.Warn( $"failed to find valid spawn point when setting pawn prefab:[{prefab}]" );
			return null;
		}

		if ( !prefab.TrySpawn( spawnPoint.GetValueOrDefault().WithScale( 1f ), out var go ) )
			return null;

		return SetPawn<TPawn>( go, dropAll: dropAll, failDestroy: true );
	}

	/// <summary>
	/// Assigns a pawn to this agent from an existing object.
	/// </summary>
	/// <param name="go"> The <see cref="GameObject"/> with <typeparamref name="TPawn"/> on it. </param>
	/// <param name="dropAll"> Set this as the only owned pawn? </param>
	/// <param name="failDestroy"> Destroy the object upon failure? </param>
	public TPawn SetPawn<TPawn>( GameObject go, bool dropAll = true, bool failDestroy = false ) where TPawn : BasePawn
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

		var pComp = go.Components.Get<TPawn>( true );

		if ( !pComp.IsValid() )
		{
			this.Warn( $"failed to find type:[{typeof( TPawn )}] on object:[{go}]" );

			if ( failDestroy )
			{
				this.Warn( $"failure detected. destroying object:[{go}]" );
				go.Destroy();
			}

			return null;
		}

		pComp.Agent = this;

		if ( dropAll && Pawns is not null )
		{
			// rndtrash: make a new list to avoid a weird "Collection was modified" exception
			foreach ( var p in Pawns.Where( p => p.IsValid() && p != pComp ).ToList() )
				p.Agent = null;
		}

		return pComp;
	}

	/// <summary>
	/// Called by the host to register a pawn assigned to this agent.
	/// </summary>
	public virtual bool AddPawn( BasePawn pawn )
	{
		if ( !Networking.IsHost )
			return false;

		if ( !this.IsValid() || !Scene.IsValid() || Scene.IsEditor )
			return false;

		if ( !pawn.IsValid() )
			return false;

		if ( IsPlayer && Identity.Type is ClientType.User )
		{
			// Must have an active, valid connection.
			if ( !Connected )
				return false;

			var cn = Connection;

			if ( cn is null )
			{
				this.Warn( "had a null connection while taking a pawn" );
				return false;
			}

			if ( !pawn.Network.AssignOwnership( Connection ) )
				return false;
		}

		if ( Pawns is null )
			Pawns = [pawn];
		else if ( !Pawns.Contains( pawn ) )
			Pawns.Add( pawn );

		this.Log( $"added pawn:[{pawn}]" );

		ValidatePawns();

		OnGainPawn( pawn );

		return true;
	}

	public virtual bool RemovePawn( BasePawn pawn )
	{
		if ( !Networking.IsHost )
			return false;

		if ( !this.IsValid() || !Scene.IsValid() || Scene.IsEditor )
			return false;

		if ( pawn is null )
			return false;

		ValidatePawns();

		OnLosePawn( pawn );

		return true;
	}

	/// <summary>
	/// Called after a pawn we owned was confirmed to be removed.
	/// </summary>
	protected virtual void OnLosePawn( BasePawn pawn )
	{
	}

	/// <summary>
	/// Called after a pawn we didn't own is confirmed to be owned.
	/// </summary>
	protected virtual void OnGainPawn( BasePawn pawn )
	{
	}

	/// <summary>
	/// Clears out references to invalid or unowned pawns.
	/// </summary>
	protected virtual void ValidatePawns()
	{
		if ( !Networking.IsHost )
			return;

		if ( !this.IsValid() || !Scene.IsValid() || Scene.IsEditor )
			return;

		if ( Pawns is null )
			return;

		var toRemove = new List<BasePawn>();

		foreach ( var pawn in Pawns )
		{
			if ( !pawn.IsValid() || pawn.Agent != this )
			{
				this.Log( $"removed invalid pawn:[{pawn}]" );
				toRemove.Add( pawn );
			}
		}

		toRemove.ForEach( cl => Pawns.Remove( cl ) );
	}

	/// <summary>
	/// Sends a request to the host to take a pawn.
	/// </summary>
	public AttemptStatus RequestTakePawn( BasePawn pawn )
	{
		if ( !pawn.IsValid() || !pawn.AllowOwnership( this ) )
			return AttemptStatus.Failure;

		RpcRequestTakePawn( pawn );

		return AttemptStatus.Active;
	}

	[Rpc.Host( NetFlags.Reliable | NetFlags.OwnerOnly )]
	protected void RpcRequestTakePawn( BasePawn pawn )
	{
		TryTakePawn( pawn );
	}

	public virtual AttemptStatus TryTakePawn( BasePawn pawn )
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

		if ( !pawn.IsValid() || !pawn.AllowOwnership( this ) )
			return Result( AttemptStatus.Failure );

		pawn.Agent = this;

		RpcTryTakePawnHostResponse( pawn, AttemptStatus.Success );

		return AttemptStatus.Success;
	}

	/// <summary>
	/// This is the method you want to call so the host can tell the owner what happened.
	/// </summary>
	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	protected void RpcTryTakePawnHostResponse( BasePawn pawn, AttemptStatus result )
	{
		if ( pawn.IsValid() )
			OnTryTakePawnResponse( pawn, result );
	}

	protected virtual void OnTryTakePawnResponse( BasePawn pawn, in AttemptStatus result )
	{
	}

	/// <returns> If this computer can tell these pawns what to do. </returns>
	public virtual bool CanOperate()
	{
		if ( !this.IsValid() || !Scene.IsValid() || Scene.IsEditor )
			return false;

		return !IsProxy;
	}

	protected virtual void SimulatePawns( in float deltaTime )
	{
		if ( Pawns is null )
			return;

		foreach ( var pawn in Pawns )
			if ( pawn.CanOperate() )
				pawn.FrameOperate( in deltaTime );
	}
}
