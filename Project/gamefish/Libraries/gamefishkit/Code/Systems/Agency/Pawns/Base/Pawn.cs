namespace GameFish;

/// <summary>
/// Something an <see cref="Agent"/> can control.
/// </summary>
[Icon( "person" )]
[EditorHandle( Icon = "🐴" )]
public abstract partial class Pawn : MovingEntity
{
	protected const int PAWN_ORDER = DEFAULT_ORDER - 5000;

	protected new const int DEBUG_ORDER = PAWN_ORDER - 50;

	public override string ToString()
	{
		var str = $"{GetType().ToSimpleString( includeNamespace: false )}";

		if ( Owner.IsValid() && !Owner.DisplayName.IsBlank() )
			str = $"{str}:\"{Owner.DisplayName}\"";

		return str;
	}

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
			var old = _owner;
			_owner = value;

			OnSetOwner( old, value );
		}
	}

	protected Agent _owner;

	public bool TryAssignOwner( Agent newAgent )
	{
		if ( !Networking.IsHost || !GameObject.IsValid() )
			return false;

		if ( !newAgent.IsValid() || !AllowOwnership( newAgent ) )
			return false;

		var cn = newAgent.Connection;

		if ( cn is null )
		{
			this.Warn( $"Failed to assign new owner:[{newAgent}] with null connection!" );
			return false;
		}

		// If the owner is the same then no need.
		if ( Owner.IsValid() && Owner == newAgent )
			if ( Network?.Owner == Owner.Network?.Owner )
				return true;

		if ( !TrySetNetworkOwner( cn, allowProxy: true ) )
		{
			this.Warn( $"Failed to assign owner:[{newAgent}] to Connection:[{cn}]" );
			return false;
		}

		Owner = newAgent;

		return true;
	}

	public bool TryDropOwner( Agent oldOwner )
	{
		if ( !Networking.IsHost )
			return false;

		// If we don't have that owner then consider it a success.
		if ( Owner != oldOwner )
			return true;

		if ( Owner is not null )
			Owner = null;

		return true;
	}

	/// <summary>
	/// Called when the <see cref="Owner"/> property has been set to a new value.
	/// </summary>
	protected virtual void OnSetOwner( Agent oldAgent, Agent newAgent )
	{
		// Ignore duplicate assignment.
		if ( oldAgent == newAgent )
			return;

		if ( DebugLogging )
		{
			if ( oldAgent.IsValid() )
			{
				if ( newAgent.IsValid() )
					this.Log( $"owner changed: [{oldAgent}] -> [{newAgent}]" );
				else
					this.Log( $"lost owner: [{oldAgent}]" );
			}
			else if ( newAgent.IsValid() )
			{
				this.Log( $"gained owner:[{newAgent}]" );
			}
		}

		if ( oldAgent.IsValid() )
			OnDropped( oldAgent: oldAgent );

		if ( newAgent.IsValid() )
			OnTaken( newAgent: newAgent, oldAgent: oldAgent );
	}

	/// <summary>
	/// Called when our new <see cref="Owner"/> has been fully confirmed.
	/// </summary>
	protected virtual void OnTaken( Agent newAgent, Agent oldAgent = null )
	{
	}

	/// <summary>
	/// Called whenever an <see cref="Owner"/> stops owning this.
	/// </summary>
	protected virtual void OnDropped( Agent oldAgent )
	{
		if ( Networking.IsHost )
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
