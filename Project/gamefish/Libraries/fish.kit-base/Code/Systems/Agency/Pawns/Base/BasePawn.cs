namespace GameFish;

/// <summary>
/// Something an <see cref="global::GameFish.Agent"/> can control.
/// </summary>
[Icon( "person" )]
[EditorHandle( Icon = "person" )]
public abstract partial class BasePawn : PhysicsEntity
{
	public const string FEATURE_PAWN = "🐴 Pawn";

	// public override string ToString()
	// => $"{GetType().ToSimpleString( includeNamespace: false )}|Agent:{Agent?.ToString() ?? "none"}";

	/// <summary>
	/// The agent controlling this pawn. Could be a player or NPC.
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	[Property, Feature( FEATURE_PAWN )]
	public Agent Agent
	{
		get => _owner;
		set
		{
			if ( _owner == value )
				return;

			if ( value != null && !AllowOwnership( value ) )
				return;

			var old = _owner;

			_owner = value;

			OnSetOwner( old, value );
		}
	}

	protected Agent _owner;

	/// <summary>
	/// The thing with the model that does the stuff.
	/// </summary>
	[Property]
	[Feature( FEATURE_PAWN ), Group( BaseActor.ACTOR )]
	public BaseActor Actor
	{
		get => _actor.IsValid() ? _actor
			: _actor = Components?.Get<BaseActor>( FindMode.EverythingInSelfAndDescendants );

		set => _actor = value;
	}

	protected BaseActor _actor;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		Tags?.Add( TAG_PAWN );

		UpdateNetworking();
	}

	public void UpdateNetworking()
	{
		if ( !Networking.IsHost )
			return;

		GameObject?.NetworkSetup(
			cn: Agent?.Connection,
			orphanMode: NetworkOrphaned.ClearOwner,
			ownerTransfer: OwnerTransfer.Fixed,
			netMode: NetworkMode.Object,
			ignoreProxy: true
		);
	}

	/// <summary>
	/// Called when the <see cref="Agent"/> property has been set to a new value.
	/// </summary>
	protected virtual void OnSetOwner( Agent old, Agent agent )
	{
		if ( !Networking.IsHost || !this.IsValid() )
			return;

		// Debug logging.
		if ( old.IsValid() )
		{
			this.Log( agent.IsValid()
				? $"owner changed: [{old}] -> [{agent}]"
				: $"lost owner: [{old}]" );
		}
		else if ( agent.IsValid() )
		{
			this.Log( $"gained owner:[{agent}]" );
		}

		// Old agent might've been destroyed, but not null.
		if ( old is not null )
		{
			// If valid: tell previous agent to drop this pawn.
			if ( old.IsValid() )
				old.RemovePawn( this );

			// Let the pawn know they're dropped.
			OnDropped( old );
		}

		// New agent must be valid.
		if ( agent.IsValid() )
		{
			// Tell the new agent to register this pawn.
			if ( agent.AddPawn( this ) )
			{
				OnTaken( old, agent );
			}
			else
			{
				this.Warn( $"failed to add Pawn:[{this}] to Agent:[{agent}]" );
				Agent = null;
			}
		}
		else
		{
			// Always drop ownership if we don't belong to an agent.
			Network.DropOwnership();
		}
	}

	/// <summary>
	/// Called when our new <see cref="Agent"/> has been fully confirmed.
	/// </summary>
	protected virtual void OnTaken( Agent old, Agent agent )
	{
	}

	/// <summary>
	/// Called whenever an <see cref="Agent"/> stops owning this.
	/// </summary>
	protected virtual void OnDropped( Agent old )
	{
	}

	/// <summary>
	/// Can an agent take ownership of this pawn?
	/// </summary>
	/// <returns> If ownership would be allowed. </returns>
	public virtual bool AllowOwnership( Agent agent )
	{
		if ( !agent.IsValid() )
			return false;

		// If it's a client then check for connection.
		if ( agent is Client cl )
		{
			if ( !cl.IsValid() || !cl.Connected )
				return false;

			if ( Network.Owner is null || Network.Owner == cl.Connection )
				return true;

			return true;
		}

		// No filtering by default for NPCs.
		return true;
	}
}
