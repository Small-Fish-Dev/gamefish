namespace GameFish;

/// <summary>
/// Something an <see cref="Agent"/> can control.
/// </summary>
[Icon( "person" )]
[EditorHandle( Icon = "🐴" )]
public abstract partial class Pawn : DestructibleEntity
{
	protected const int PAWN_ORDER = DEFAULT_ORDER - 444;

	// public override string ToString()
	// => $"{GetType().ToSimpleString( includeNamespace: false )}|Agent:{Agent?.ToString() ?? "none"}";

	/// <summary>
	/// A position between our aim and feet.
	/// </summary>
	public override Vector3 Center => WorldPosition.LerpTo( EyePosition, 0.5f );

	public virtual bool IsPlayer => Owner?.IsPlayer is true;

	[Sync( SyncFlags.FromHost )]
	public Agent Owner
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

	public virtual bool TryAssignOwner( Agent newAgent )
	{
		if ( !Networking.IsHost || !this.IsValid() )
			return false;

		if ( !newAgent.IsValid() )
			return TryDropOwner();

		if ( Owner == newAgent )
			return true;

		Owner = newAgent;

		if ( Owner != newAgent )
			return false;

		newAgent.OnGainPawn( this );

		return true;
	}

	public virtual bool TryDropOwner()
	{
		if ( !Networking.IsHost )
			return false;

		var owner = Owner;

		if ( !this.IsValid() || !owner.IsValid() )
			return true;

		Owner = null;

		if ( Owner.IsValid() )
			return false;

		owner.OnLosePawn( this );

		return true;
	}

	/// <summary>
	/// Called when the <see cref="Owner"/> property has been set to a new value.
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
			if ( newAgent.TryAssignPawn( this ) )
			{
				OnTaken( oldAgent, newAgent );
			}
			else
			{
				this.Warn( $"failed to add Pawn:[{this}] to Agent:[{newAgent}]" );
				Owner = null;
			}
		}
		else
		{
			SetWishVelocity( Vector3.Zero );

			// Always drop ownership if we don't belong to an agent.
			TrySetNetworkOwner( null );
		}
	}

	/// <summary>
	/// Called when our new <see cref="Owner"/> has been fully confirmed.
	/// </summary>
	protected virtual void OnTaken( Agent old, Agent agent )
	{
	}

	/// <summary>
	/// Called whenever an <see cref="Owner"/> stops owning this.
	/// </summary>
	protected virtual void OnDropped( Agent old )
	{
		GameObject?.Destroy();
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
