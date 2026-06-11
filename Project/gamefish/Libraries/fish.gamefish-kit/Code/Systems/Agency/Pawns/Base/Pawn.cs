namespace GameFish;

/// <summary>
/// Something an <see cref="Client"/> can control.
/// </summary>
[Icon( "person" )]
[EditorHandle( Icon = "🐴" )]
public abstract partial class Pawn : DynamicEntity
{
	protected const int PAWN_ORDER = DEFAULT_ORDER - 5000;

	protected new const int DEBUG_ORDER = PAWN_ORDER - 50;

	/// <summary>
	/// Could be an animated model or a sprite.
	/// Used to fade model(s) in/out from distance.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( BODY )]
	public virtual PawnBody Body
	{
		get => this?.GetCached( ref _body );
		set => _body = value;
	}

	protected PawnBody _body;

	public override string ToString()
	{
		var str = $"{GetType().ToSimpleString( includeNamespace: false )}";

		if ( Owner.IsValid() && !Owner.DisplayName.IsBlank() )
			str = $"{str}:\"{Owner.DisplayName}\"";

		return str;
	}

	/// <summary>
	/// A position between our feet and aim.
	/// </summary>
	public override Vector3 Center => Controller?.Center ?? WorldPosition.LerpTo( EyePosition, 0.5f );

	public Vector3 Bottom => Controller?.Bottom ?? WorldPosition;
	public Vector3 Top => Controller?.Bottom ?? EyePosition;

	/// <summary>
	/// Is this actively owned by a valid player client?
	/// </summary>
	public virtual bool IsPlayer => Owner.IsValid() && Owner.IsPlayer;

	[Sync( SyncFlags.FromHost )]
	public Client Owner
	{
		get => _owner;

		protected set
		{
			var old = _owner;
			_owner = value;

			OnSetOwner( old, value );
		}
	}

	protected Client _owner;

	public bool TryAssignOwner( Client newAgent )
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

		if ( !TryNetwork( cn, allowProxy: true ) )
		{
			this.Warn( $"Failed to assign owner:[{newAgent}] to Connection:[{cn}]" );
			return false;
		}

		Owner = newAgent;

		return true;
	}

	public bool TryDropOwner( Client oldOwner )
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
	protected virtual void OnSetOwner( Client oldAgent, Client newAgent )
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
	protected virtual void OnTaken( Client newAgent, Client oldAgent = null )
	{
	}

	/// <summary>
	/// Called whenever an <see cref="Owner"/> stops owning this.
	/// </summary>
	protected virtual void OnDropped( Client oldAgent )
	{
		if ( Networking.IsHost )
			GameObject?.Destroy();
	}

	/// <summary>
	/// Can a valid agent take ownership of this pawn?
	/// </summary>
	/// <returns> If ownership would be allowed. </returns>
	public virtual bool AllowOwnership( Client agent )
	{
		if ( !agent.IsValid() )
			return false;

		// If it's a client then check if they're connected.
		if ( agent is Client cl )
		{
			if ( !cl.IsValid() || !cl.Connected )
				return false;

			return true;
		}

		// No filtering by default for NPCs.
		return true;
	}
}
