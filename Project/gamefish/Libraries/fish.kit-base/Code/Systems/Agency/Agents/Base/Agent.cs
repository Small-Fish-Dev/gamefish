namespace GameFish;

/// <summary>
/// Something capable of control over other objects. <br />
/// It may be a player(real/fake) or an NPC.
/// </summary>
[Icon( "psychology" )]
[EditorHandle( Icon = "psychology" )]
public abstract partial class Agent : ModuleEntity
{
	public const string FEATURE_AGENT = "🧠 Agent";
	public const string GROUP_ID = "🆔 Identity";

	/// <summary>
	/// Is this meant to be owned by a player?
	/// </summary>
	[Property, Feature( FEATURE_AGENT )]
	public virtual bool IsPlayer { get; protected set; } = false;

	/// <summary>
	/// What specific pawn(if any) is under this agent's control?
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	[Property, ReadOnly, Feature( FEATURE_AGENT )]
	public BasePawn Pawn { get; set; }

	[Property]
	[Title( "Set Pawn" )]
	[HideIf( nameof( InEditor ), true )]
	[Feature( FEATURE_AGENT ), Category( "Debug" )]
	public BasePawn SetDebugPawn
	{
		get => _debugPawn;
		set
		{
			_debugPawn = value;
			if ( !_debugPawn.IsValid() ) return;
			SetPawn<BasePawn>( _debugPawn.GameObject );
		}
	}

	private BasePawn _debugPawn;

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

		if ( this.IsOwner() )
			SimulatePawns( Time.Delta );
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		UpdateNetworking( Connection );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		TryDropPawn();
	}

	/// <returns> A random default spawn point's transform(if any). </returns>
	public virtual Transform? GetSpawnPoint()
	{
		return Game.Random.FromArray( Scene?.GetAll<SpawnPoint>()?.ToArray() )
			?.WorldTransform.WithScale( 1f );
	}

	protected bool InEditor => Scene?.IsEditor ?? true;

	/// <summary>
	/// Spawns a <see cref="BasePawn"/> prefab and assigns it to this agent.
	/// </summary>
	/// <param name="prefab"></param>
	public BasePawn SetPawn( PrefabFile prefab )
		=> SetPawn<BasePawn>( prefab );

	/// <summary>
	/// Spawns a <typeparamref name="TPawn"/> prefab and assigns it to this agent.
	/// </summary>
	/// <param name="prefab"></param>
	public TPawn SetPawn<TPawn>( PrefabFile prefab ) where TPawn : BasePawn
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

		return SetPawn<TPawn>( go, failDestroy: true );
	}

	/// <summary>
	/// Assigns a pawn to this agent from an existing object.
	/// </summary>
	/// <param name="go"> The <see cref="GameObject"/> with <typeparamref name="TPawn"/> on it. </param>
	/// <param name="failDestroy"> Destroy the object upon failure? </param>
	public TPawn SetPawn<TPawn>( GameObject go, bool failDestroy = false ) where TPawn : BasePawn
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

		if ( !TrySetPawn( newPawn ) )
		{
			failed = true;
			this.Warn( $"failed to set pawn:[{newPawn}]" );
		}

		if ( failed && failDestroy )
		{
			this.Warn( $"failure detected. destroying object:[{go}]" );
			go.Destroy();

			return null;
		}

		return newPawn;
	}

	/// <summary>
	/// Called by the host to register a pawn assigned to this agent.
	/// </summary>
	public virtual bool TrySetPawn( BasePawn pawn )
	{
		if ( !Networking.IsHost )
			return false;

		if ( !this.IsValid() || !Scene.IsValid() )
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

			pawn.UpdateNetworking( Connection );

			if ( pawn.Network.Owner != Connection )
			{
				this.Warn( $"Failed to assign ownership to Connection:[{Connection}]" );
				return false;
			}
		}

		if ( !pawn.TrySetOwner( this ) )
			Pawn = pawn;

		this.Log( $"added pawn:[{pawn}]" );

		return true;
	}

	public bool TryDropPawn()
	{
		if ( !Networking.IsHost )
			return false;

		if ( Pawn is not BasePawn pawn || !pawn.IsValid() )
			return true;

		return pawn.TryDropOwner();
	}

	/// <summary>
	/// Called when a pawn we owned was confirmed to be removed.
	/// </summary>
	public virtual void OnLosePawn( BasePawn pawn )
	{
	}

	/// <summary>
	/// Called when a pawn we didn't own is confirmed to be owned.
	/// </summary>
	public virtual void OnGainPawn( BasePawn pawn )
	{
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

		if ( !pawn.TrySetOwner( this ) )
			return Result( AttemptStatus.Failure );

		return Result( AttemptStatus.Success );
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
		if ( !this.IsOwner() )
		{
			this.Warn( "Tried to simulate pawns we don't own!" );
			return;
		}

		if ( Pawn is not BasePawn pawn || !pawn.IsValid() )
			return;

		if ( pawn.CanSimulate() )
			pawn.FrameSimulate( in deltaTime );
	}
}
