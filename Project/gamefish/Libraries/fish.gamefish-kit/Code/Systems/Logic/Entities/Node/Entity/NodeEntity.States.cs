using System.Text.Json.Serialization;

namespace GameFish.Nodes;

partial class NodeEntity
{
	/// <summary>
	/// The default state to select on start.
	/// <br /> <br />
	/// <b> NOTE: </b> If not defined then all states will be disabled initially.
	/// In that case you'll have to manually select one or every linked state will stay disabled.
	/// </summary>
	[Sync]
	[Property]
	[Title( "Default" )]
	[Order( NODE_STATES_ORDER )]
	[Feature( NODE ), Group( STATES )]
	[ShowIf( nameof( InspectorShowDefaultState ), true )]
	public NodeEntity DefaultState { get; set; }

	/// <summary>
	/// The actively selected state.
	/// </summary>
	[Sync]
	[Property]
	[JsonIgnore]
	[Title( "Active" )]
	[Order( NODE_STATES_ORDER )]
	[Feature( NODE ), Group( STATES )]
	[ShowIf( nameof( InspectorShowCurrentState ), true )]
	protected NodeEntity InspectorState
	{
		get => StateNode;
		set => StateNode = TrySelectState( value ) ? value : null;
	}

	protected bool InspectorShowDefaultState => StatesEnabled && InEditor;
	protected bool InspectorShowCurrentState => StatesEnabled && InGame;

	/// <summary>
	/// The actively selected state node(if any).
	/// </summary>
	[Sync]
	public NodeEntity StateNode { get; protected set; }

	protected virtual void SetupDefaultStates()
	{
		if ( !StatesEnabled )
			return;

		if ( DefaultState.IsValid() && DefaultState.Active )
			TrySelectState( DefaultState );
	}

	/// <summary>
	/// Ensure that our linked states are enabled/disabled when they should be.
	/// </summary>
	protected virtual void UpdateLinkedStates()
	{
		if ( !TryGetLinkedStates( out var states ) )
			return;

		foreach ( var state in states )
			UpdateLinkedState( state );
	}

	/// <summary>
	/// Ensure that our linked state is enabled/disabled when it should be.
	/// </summary>
	protected virtual void UpdateLinkedState( NodeEntity state )
	{
		if ( !state.IsValid() )
			return;

		// TODO: Network signals if we don't own them.
		if ( state.IsProxy )
			return;

		state.Toggle( IsOn && state == StateNode );
	}

	public virtual bool TrySelectState( NodeEntity node )
	{
		if ( !StatesEnabled )
			return false;

		// Allow clearing the state this way.
		if ( !node.IsValid() || !node.Active )
		{
			if ( StateNode.IsValid() )
			{
				StateNode = null;
				UpdateLinkedStates();
			}

			return true;
		}

		// TODO: Send a signal to enable it.
		if ( node.IsProxy )
			return false;

		if ( !IsState( node ) )
			return false;

		StateNode = node;

		UpdateLinkedState( StateNode );

		if ( DebugLogNode )
			this.Log( $"Selected state:[{node}]" );

		UpdateLinkedStates();

		return true;
	}

	/// <returns> The nodes this links to as state modules(if any). </returns>
	public virtual bool TryGetLinkedStates( out IEnumerable<NodeEntity> states )
	{
		if ( !StatesEnabled || Links is null )
		{
			states = null;
			return false;
		}

		states = GetLinkedStates();

		return states?.Any() is true;
	}

	protected virtual IEnumerable<NodeEntity> GetLinkedStates()
	{
		if ( GameObject.IsDestroyed() )
			yield break;

		if ( Links is null )
			yield break;

		foreach ( var (node, p) in Links )
		{
			if ( !node.IsValid() || !node.Active )
				continue;

			if ( p is LinkProtocol.State )
				yield return node;
		}
	}
}
