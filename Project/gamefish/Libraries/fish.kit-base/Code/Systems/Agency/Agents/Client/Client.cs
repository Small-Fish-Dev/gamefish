using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// An <see cref="Agent"/> with <see cref="GameFish.Identity"/> that supports a connection.
/// </summary>
[Icon( "account_box" )]
[EditorHandle( Icon = "account_box" )]
public partial class Client : Agent
{
	public static IEnumerable<Client> All => Server.ValidClients;
	public static IEnumerable<Client> Players => Server.PlayerClients;

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

	[Sync( SyncFlags.FromHost )]
	public bool IsBot { get; set; }

	/// <summary>
	/// Is this meant to be a player?
	/// </summary>
	public override bool IsPlayer => !IsBot;

	[Sync( SyncFlags.FromHost )]
	public override Identity Identity
	{
		get => _id;

		protected set
		{
			if ( _id == value )
				return;

			var old = _id;
			_id = value;

			if ( value.IsValid() )
				OnSetIdentity( in old, ref _id );
		}
	}

	protected Identity _id;

	public override Connection Connection => _id.Connection;

	/// <summary> Is the client's connection defined and active? </summary>
	public override bool Connected => Connection is Connection cn && (!Networking.IsActive || cn.IsActive);

	/// <summary>
	/// The connection's display name.
	/// </summary>
	public override string DisplayName => Connection?.DisplayName ?? base.DisplayName;

	public override bool CompareConnection( Connection cn )
		=> _id.CompareConnection( cn );

	protected override void OnEnabled()
	{
		// Keep client objects between scenes.
		if ( GameObject.IsValid() )
			GameObject.Flags |= GameObjectFlags.DontDestroyOnLoad;

		// Then network spawn.
		base.OnEnabled();
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( this.IsOwner() && IsPlayer )
			Local = this;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		// Don't bother in the editor or a destroyed scene.
		if ( !this.InGame() )
			return;

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

		if ( this.IsOwner() )
			UpdateCamera();
	}

	/// <summary>
	/// Sets the camera's transform according to its pawn.
	/// </summary>
	public virtual void UpdateCamera()
	{
		if ( Scene?.Camera is not CameraComponent cam || !cam.IsValid() )
			return;

		if ( Pawn is not BasePawn pawn || !pawn.IsValid() )
			return;

		if ( !pawn.CanSimulate() )
			return;

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

		TrySetNetworkOwner( Connection );

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
