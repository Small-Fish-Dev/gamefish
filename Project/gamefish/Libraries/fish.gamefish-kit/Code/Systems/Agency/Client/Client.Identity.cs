using System.Text.Json.Serialization;

namespace GameFish;

partial class Client
{
	[Property]
	[Title( "Identity" )]
	[ReadOnly, JsonIgnore]
	[WideMode( HasLabel = false )]
	[InlineEditor( Label = false )]
	[Feature( CLIENT ), Group( ID )]
	[ShowIf( nameof( InGame ), true )]
	protected Identity InspectorIdentity => Identity;

	[Sync( SyncFlags.FromHost )]
	public virtual Identity Identity
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

	/// <summary>
	/// Is this owned by a player?
	/// </summary>
	[Property]
	[Title( "Is Player" )]
	[ReadOnly, JsonIgnore]
	[Feature( CLIENT ), Order( CLIENT_ORDER )]
	protected virtual bool InspectorIsPlayer => IsPlayer;

	[Sync( SyncFlags.FromHost )]
	public bool IsBot { get; set; }

	/// <summary>
	/// Is this meant to be a player?
	/// </summary>
	public virtual bool IsPlayer => !IsBot;

	/// <summary>
	/// If bot: always true. ('cause they in the matrix or some shit) <br />
	/// If real person™: if the connection exists and is active.
	/// </summary>
	[Property]
	[Feature( CLIENT )]
	[Title( "Connected" )]
	[ReadOnly, JsonIgnore]
	protected virtual bool InspectorConnected => Connected;

	/// <summary>
	/// Is the connection defined and active?
	/// </summary>
	public virtual bool Connected => !Networking.IsActive || (Connection?.IsActive is true);

	/// <summary>
	/// The connection assigned to this client.
	/// </summary>
	public virtual Connection Connection => _id.Connection;

	/// <summary>
	/// The connection's display name.
	/// </summary>
	public virtual string DisplayName => Connection?.DisplayName;
}
