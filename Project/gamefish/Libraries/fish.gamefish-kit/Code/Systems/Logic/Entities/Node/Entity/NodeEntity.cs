using System;

namespace GameFish.Nodes;

/// <summary>
/// A node entity that can run its own logic when enabled and signal to or toggle other nodes within a network of any size.
/// <br /> <br />
/// <b> LOGIC: </b> The "<b>Toggle</b>" command can enable/disable an interactivity with this node.
/// <br /> <br />
/// <b> LOGIC: </b> Use the "<b>Activate</b>" command with no value to signal nearby nodes.
/// <br /> <br />
/// <b> LOGIC: </b> Pass in a string with the "<b>Activate</b>" command to run one of this node's functions.
/// </summary>
[Icon( "polyline" )]
[Title( "Logic Node" )]
[EditorHandle( Icon = "polyline" )]
public partial class NodeEntity : LogicEntity
{
	protected const int NODE_ORDER = LOGIC_ORDER - 1000;
	public const string NODE_DESCRIPTION = "A dynamic point of a logical network.";

	protected const int NODE_DEBUG_ORDER = NODE_ORDER - 50;

	protected const int NODE_LINKS_ORDER = NODE_ORDER + 10;
	protected const int NODE_STATES_ORDER = NODE_ORDER + 20;
	protected const int NODE_TIMING_ORDER = NODE_ORDER + 30;

	protected const int METHODS_ORDER = NODE_ORDER + 100;
	protected const string METHODS_DESCRIPTION = "Inspector-defined methods that can be triggered through logic functions or node signals.";

	protected const int METHODS_CUSTOM_ORDER = METHODS_ORDER + 10;
	protected const int METHODS_LOGIC_ORDER = METHODS_ORDER + 20;

	public const string NAME_DEFAULT = "Node";

	public override string ToString()
	{
		var typeName = GetType().ToSimpleString( includeNamespace: false );

		if ( NetworkName.IsBlank() )
			return $"{typeName}";

		return $"{typeName}|\"{NetworkName}\"";
	}

	/// <summary>
	/// If enabled: the node is allowed to function.
	/// </summary>
	[Sync]
	[Property]
	[EnumButtonGroup]
	[Feature( NODE )]
	[Title( "Enabled" )]
	[Order( NODE_ORDER )]
	[WideMode( HasLabel = true )]
	[Change( Name = nameof( OnNodeStateSet ) )]
	public virtual ToggleState NodeEnabled { get; set; } = ToggleState.Enabled;

	/// <summary>
	/// The additional capabilities of this node.
	/// </summary>
	[Property]
	[Title( "Features" )]
	[Order( NODE_ORDER )]
	[EnumButtonGroup, BitFlags]
	[WideMode( HasLabel = true )]
	[Feature( NODE, Description = NODE_DESCRIPTION )]
	public virtual NodeFeatures Features { get; protected set; }

	/// <summary>
	/// The rate at which this node will run logic continuously(if enabled).
	/// </summary>
	[Property]
	[Title( "Tick Rate" )]
	[Order( NODE_TIMING_ORDER )]
	[Feature( NODE ), Group( TIMING )]
	[ShowIf( nameof( TickingEnabled ), true )]
	[Range( 0f, 5f, clamped: false ), Step( 0.1f )]
	public virtual float TickRate { get; protected set; } = 0.1f;

	/// <summary>
	/// Runs logic continuously as fast as you set the tick rate.
	/// </summary>
	[Property]
	[Title( "On Tick" )]
	[WideMode( HasLabel = true )]
	[InlineEditor( Label = true )]
	[Order( NODE_TIMING_ORDER + 5 )]
	[Feature( NODE ), Group( TIMING )]
	[ShowIf( nameof( TickingEnabled ), true )]
	protected virtual List<LogicAction> OnTickLogic { get; set; } = [new( LogicAction.ActionType.Node )];

	public virtual bool HasNodeFlag( in NodeFeatures flag ) => Features.HasFlag( flag );

	public bool TickingEnabled => HasNodeFlag( NodeFeatures.Tick );
	public bool StatesEnabled => HasNodeFlag( NodeFeatures.States );
	// public bool RelayEnabled => HasNodeFlag( NodeFeatures.Relay );
	// public bool RoutingEnabled => HasNodeFlag( NodeFeatures.Router );

	protected override void OnStart()
	{
		base.OnStart();

		OnNodeStart();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		OnNodeUpdate();

		RenderNode( isGizmoPass: false );
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		RenderNode( isGizmoPass: true );
	}

	protected virtual void OnNodeUpdate()
	{
		Think();
	}

	/// <returns> If this is an enabled, actively capable node. </returns>
	public virtual bool IsNodeEnabled()
	{
		if ( GameObject.IsDestroyed() )
			return false;

		if ( !Active )
			return false;

		return IsOn;
	}
}
