namespace GameFish;

/// <summary>
/// Something an <see cref="global::GameFish.Agent"/> can control.
/// </summary>
[Icon( "person" )]
[EditorHandle( Icon = "🐴" )]
public abstract partial class BasePawn : DestructibleEntity
{
	// public override string ToString()
	// => $"{GetType().ToSimpleString( includeNamespace: false )}|Agent:{Agent?.ToString() ?? "none"}";

	/// <summary>
	/// Is this controlled by a player agent?
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( DEBUG )]
	public virtual bool IsPlayer => Agent?.IsPlayer ?? false;

	/// <summary>
	/// The agent controlling this pawn. Could be a player or an NPC.
	/// </summary>
	[Property, Feature( PAWN )]
	[Sync( SyncFlags.FromHost )]
	public Agent Agent
	{
		get => _owner;
		protected set
		{
			if ( _owner == value )
				return;

			if ( Networking.IsHost )
			{
				if ( value is not null && !AllowOwnership( value ) )
					return;
			}

			var old = _owner;

			_owner = value;

			OnSetOwner( old, value );
		}
	}

	protected Agent _owner;

	public virtual bool TrySetOwner( Agent newAgent )
	{
		if ( !Networking.IsHost || !this.IsValid() )
			return false;

		if ( !newAgent.IsValid() )
			return TryDropOwner();

		if ( Agent == newAgent )
			return true;

		Agent = newAgent;

		if ( Agent != newAgent )
			return false;

		newAgent.OnGainPawn( this );

		return true;
	}

	public virtual bool TryDropOwner()
	{
		if ( !Networking.IsHost )
			return false;

		var owner = Agent;

		if ( !this.IsValid() || !owner.IsValid() )
			return true;

		Agent = null;

		if ( Agent.IsValid() )
			return false;

		owner.OnLosePawn( this );

		return true;
	}

	/// <summary>
	/// Called when the <see cref="Agent"/> property has been set to a new value.
	/// </summary>
	protected virtual void OnSetOwner( Agent oldAgent, Agent newAgent )
	{
		if ( !Networking.IsHost || !this.IsValid() )
			return;

		// Debug logging.
		if ( oldAgent.IsValid() )
		{
			this.Log( newAgent.IsValid()
				? $"owner changed: [{oldAgent}] -> [{newAgent}]"
				: $"lost owner: [{oldAgent}]" );
		}
		else if ( newAgent.IsValid() )
		{
			this.Log( $"gained owner:[{newAgent}]" );
		}

		// Old agent might've been destroyed, but not null.
		if ( oldAgent is not null )
		{
			// If valid: tell previous agent to drop this pawn.
			if ( oldAgent.IsValid() )
			{
				if ( oldAgent.Pawn == this && oldAgent.TryDropPawn() )
					OnDropped( oldAgent );
			}
			else
			{
				OnDropped( oldAgent );
			}
		}

		// New agent must be valid.
		if ( newAgent.IsValid() )
		{
			// Tell the new agent to register this pawn.
			if ( newAgent.TrySetPawn( this ) )
			{
				OnTaken( oldAgent, newAgent );
			}
			else
			{
				this.Warn( $"failed to add Pawn:[{this}] to Agent:[{newAgent}]" );
				Agent = null;
			}
		}
		else
		{
			WishVelocity = default;

			// Always drop ownership if we don't belong to an agent.
			UpdateNetworking( null );
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
	/// Can a valid agent take ownership of this pawn?
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
