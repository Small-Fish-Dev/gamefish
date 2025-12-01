using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Something capable of control over other objects. <br />
/// It may be a player(real/fake) or an NPC.
/// </summary>
[Icon( "psychology" )]
[EditorHandle( Icon = "psychology" )]
public abstract partial class Agent : ModuleEntity, ISimulate
{
	// mister anderson
	protected const int AGENT_ORDER = DEFAULT_ORDER - 1999;

	/// <summary>
	/// Is this owned by a player?
	/// </summary>
	[Title( "Is Player" )]
	[Property, ReadOnly, JsonIgnore]
	[Feature( AGENT ), Order( AGENT_ORDER )]
	protected virtual bool InspectorIsPlayer => IsPlayer;

	/// <summary>
	/// Is this meant to be owned by a player?
	/// </summary>
	public virtual bool IsPlayer { get; }

	/// <summary>
	/// What specific pawn(if any) is under this agent's control?
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	public Pawn Pawn { get; set; }

	[Title( "Pawn" )]
	[Property, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( AGENT ), Group( DEBUG )]
	protected Pawn InspectorPawn
	{
		get => Pawn;
		set => TryAssignPawn( value );
	}

	[Title( "Identity" )]
	[Feature( AGENT ), Group( ID )]
	[Property, ReadOnly, JsonIgnore]
	protected Identity InspectorIdentity => Identity;

	public abstract Identity Identity { get; protected set; }

	public virtual Connection Connection => Connection.Host;

	/// <summary>
	/// If NPC/Bot: always true. ('cause they in the matrix or some shit) <br />
	/// If <see cref="Client"/>: if the connection exists and is active.
	/// </summary>
	[Title( "Connected" )]
	[ReadOnly, JsonIgnore]
	[Property, Feature( AGENT )]
	protected bool InspectorConnected => Connected;

	/// <summary>
	/// If NPC/Bot: always true. ('cause they in the matrix or some shit) <br />
	/// If <see cref="Client"/>: if the connection exists and is active.
	/// </summary>
	public virtual bool Connected => true;

	/// <summary>
	/// If NPC/Bot: always false. <br />
	/// If <see cref="Client"/>: if our <see cref="Identity"/> has the specified connection.
	/// </summary>
	public virtual bool CompareConnection( Connection cn )
		=> false;

	public override string ToString()
	{
		if ( DisplayName.IsBlank() )
			return $"{base.ToString()}";

		return $"{base.ToString()}|Name:{DisplayName}";
	}

	/// <summary>
	/// A nice display name.
	/// </summary>
	public virtual string DisplayName => null;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( CanSimulate() )
			FrameSimulate( Time.Delta );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( CanSimulate() )
			FixedSimulate( Time.Delta );
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		TrySetNetworkOwner( Connection );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		TryDropPawn();
	}

	public virtual bool CanSimulate()
		=> InGame && this.IsOwner();

	public virtual void FrameSimulate( in float deltaTime )
	{
		if ( Pawn.IsValid() )
			Pawn.FrameSimulate( in deltaTime );
	}

	public virtual void FixedSimulate( in float deltaTime )
	{
		if ( Pawn.IsValid() )
			Pawn.FixedSimulate( in deltaTime );
	}

	/// <returns> A random default spawn point's transform(if any). </returns>
	public virtual Transform? FindSpawnPoint()
	{
		if ( GameManager.TryGetInstance( out var gm ) )
			return gm.FindSpawnPoint( this );

		var allSpawnPoints = Scene?.GetAll<SpawnPoint>();

		if ( allSpawnPoints is null || !allSpawnPoints.Any() )
			return null;

		return allSpawnPoints.PickRandom()?.WorldTransform;
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
	/// Called by the host to register a pawn assigned to this agent.
	/// </summary>
	public virtual bool TryAssignPawn( Pawn pawn )
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

			pawn.TrySetNetworkOwner( Connection );

			if ( pawn.Network.Owner != Connection )
			{
				this.Warn( $"Failed to assign ownership to Connection:[{Connection}]" );
				return false;
			}
		}

		var oldPawn = Pawn;

		if ( pawn.TryAssignOwner( this ) )
		{
			Pawn = pawn;

			if ( oldPawn.IsValid() )
				oldPawn.TryDropOwner();
		}

		this.Log( $"added pawn:[{pawn}]" );

		return true;
	}

	public bool TryDropPawn()
	{
		if ( !Networking.IsHost )
			return false;

		if ( Pawn is not Pawn pawn || !pawn.IsValid() )
			return true;

		return pawn.TryDropOwner();
	}

	/// <summary>
	/// Called when a pawn we owned was confirmed to be removed.
	/// </summary>
	public virtual void OnLosePawn( Pawn pawn )
	{
	}

	/// <summary>
	/// Called when a pawn we didn't own is confirmed to be owned.
	/// </summary>
	public virtual void OnGainPawn( Pawn pawn )
	{
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

		if ( !pawn.IsValid() || !pawn.AllowOwnership( this ) )
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
