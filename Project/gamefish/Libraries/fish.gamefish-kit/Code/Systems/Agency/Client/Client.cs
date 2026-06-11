using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Something capable of control over other objects. <br />
/// It may be a real or fake(bot) player.
/// </summary>
[Icon( "account_box" )]
[EditorHandle( Icon = "account_box" )]
public partial class Client : ModuleEntity, ISimulate
{
	protected const int CLIENT_ORDER = DEFAULT_ORDER - 1000;
	protected const int CLIENT_DEBUG_ORDER = CLIENT_ORDER + 500;

	protected const int TEAM_ORDER = CLIENT_ORDER - 100;

	public static IEnumerable<Client> All => Server.ValidClients;
	public static IEnumerable<Client> Players => Server.PlayerClients;

	public static IEnumerable<TClient> GetAll<TClient>() where TClient : Client
		=> Server.GetAllClients<TClient>();

	/// <summary>
	/// A valid <see cref="Client"/> only ever belonging to the local connection(or null).
	/// Automatically finds and caches the local client if not yet defined.
	/// </summary>
	public static Client Local
	{
		get
		{
			// Must have explicit ownership of the cached instance.
			if ( _local.IsOwner() )
				return _local;

			// Auto-cache the first instance with our connection.
			return _local = Server.FindClient( Connection.Local );
		}

		protected set => _local = value;
	}

	private static Client _local;

	/// <summary>
	/// Is this the local user's client?
	/// </summary>
	public bool IsLocal => Local == this;

	public override string ToString()
	{
		var str = $"{GetType().ToSimpleString( includeNamespace: false )}";

		if ( !DisplayName.IsBlank() )
			str = $"{str}:\"{DisplayName}\"";

		return str;
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		// Keep client objects between scenes.
		if ( GameObject.IsValid() )
			GameObject.Flags |= GameObjectFlags.DontDestroyOnLoad;
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( !Local.IsValid() )
			if ( this.IsOwner() && IsPlayer )
				Local = this;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		OnClientUpdate( Time.Delta );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		OnClientFixedUpdate( Time.Delta );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		// Don't bother in the editor or a destroyed scene.
		if ( !InGame )
			return;

		if ( Networking.IsHost )
			TryDropPawn( Pawn );

		// Cleanup the pawn upon leaving.
		// TODO: Let pawns prevent this.
		if ( Pawn.IsValid() )
			Pawn.DestroyGameObject();

		// Destroy the entire client object with the component.
		if ( GameObject.IsValid() )
		{
			// this.Log( $"was destroyed. cleaning up object:[{GameObject}]" );
			GameObject.Destroy();
		}
	}

	protected override void OnPreRender()
	{
		base.OnPreRender();

		if ( !IsLocal )
			return;

		UpdateCamera();
	}

	protected virtual void OnClientUpdate( in float deltaTime )
	{
		if ( CanSimulate() )
			FrameSimulate( in deltaTime );

		UpdateVoice();
	}

	protected virtual void OnClientFixedUpdate( in float deltaTime )
	{
		if ( !CanSimulate() )
			return;

		FixedSimulate( in deltaTime );
	}

	public virtual bool CanSimulate()
		=> InGame && this.IsOwner();

	public virtual void FrameSimulate( in float deltaTime )
	{
		UpdateInput( in deltaTime );

		SimulatePawn( Pawn, in deltaTime, isFixedUpdate: false );
	}

	public virtual void FixedSimulate( in float deltaTime )
	{
		SimulatePawn( Pawn, in deltaTime, isFixedUpdate: true );
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
	/// If our <see cref="Identity"/> has the specified connection.
	/// </summary>
	public virtual bool CompareConnection( Connection cn )
		=> _id.CompareConnection( cn );

	/// <summary>
	/// Sets the camera's transform according to its pawn.
	/// </summary>
	public virtual void UpdateCamera()
	{
		if ( !Scene.IsValid() || !Scene.Camera.IsValid() )
			return;

		if ( !Pawn.IsValid( out var pawn ) )
			return;

		if ( !pawn.CanSimulate() )
			return;

		var cam = Scene.Camera;
		var tView = cam.WorldTransform;

		if ( pawn.TryApplyView( cam, ref tView ) )
			cam.WorldTransform = tView;
	}

	/// <summary>
	/// Create a networkable ID for this client using a connection.
	/// </summary>
	public void AssignConnection( Connection cn, out Identity id )
	{
		id = new Identity( this, cn );
		Identity = id;

		TryNetwork( Connection );

		this.Log( $"assigned Connection:[{cn}]" );
	}

	/// <summary>
	/// Allows you to modify and/or respond to new identity assignment.
	/// </summary>
	protected virtual void OnSetIdentity( in Identity old, ref Identity id )
	{
	}

	/// <summary>
	/// Updates the name of this client's identity.
	/// </summary>
	[Rpc.Host( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public virtual void SetName( string name )
	{
		Identity = Identity with { Name = name };
	}
}
