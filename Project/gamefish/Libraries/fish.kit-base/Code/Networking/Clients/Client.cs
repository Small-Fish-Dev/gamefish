namespace GameFish;

/// <summary>
/// An <see cref="Agent"/> with an Identity that supports a connection.
/// </summary>
[Icon( "account_box" )]
[EditorHandle( Icon = "account_box" )]
public partial class Client : Agent
{
	public static Client Local => _local.GetSingleton( isOwned: true );
	protected static readonly Client _local;

	[Property, Feature( FEATURE_AGENT )]
	public override bool IsPlayer { get; } = true;

	[ReadOnly]
	[Sync( SyncFlags.FromHost )]
	[Property, Feature( FEATURE_AGENT )]
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

	/// <summary> Is the identity's connection defined and active? </summary>
	[ReadOnly]
	[Property, Feature( FEATURE_AGENT )]
	public override bool Connected => Connection is not null;// TEMP(lobby bug!): && Connection.IsActive;

	public override string Name => Connection?.DisplayName ?? base.Name;

	public override string ToString()
		=> base.ToString() + $"|Name:{Name}";

	public override bool CompareConnection( Connection cn )
		=> _id.CompareConnection( cn );

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( GameObject.IsValid() )
		{
			this.Log( $"was destroyed. cleaning up object:[{GameObject}]" );
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
	/// Sets camera transform according to the current view.
	/// </summary>
	protected virtual void UpdateCamera()
	{
		if ( Pawns is null || !Scene.IsValid() )
			return;

		var cam = Scene.Camera;

		if ( !cam.IsValid() )
			return;

		var tView = cam.WorldTransform;

		foreach ( var pawn in Pawns )
			if ( pawn.CanSimulate() )
				pawn.ApplyView( cam, ref tView );

		cam.WorldTransform = tView;
	}

	/// <summary>
	/// Create a networkable ID for this client using a connection.
	/// </summary>
	public void AssignConnection( Connection cn, out Identity id )
	{
		id = new Identity( this, cn );
		Identity = id;

		UpdateNetworking( Connection );

		this.Log( "assigned connection: " + cn );
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
