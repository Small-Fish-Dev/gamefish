using System.Text.Json.Serialization;

namespace Playground;

/// <summary>
/// A physical object a player can create.
/// Tracks who and what it belongs to so that stuff can be managed.
/// </summary>
public partial class EditorObject : PhysicsObject
{
	protected const int EDITOR_ORDER = DEFAULT_ORDER - 1000;

	[Title( "Owner" )]
	[Property, JsonIgnore, ReadOnly]
	[ShowIf( nameof( InGame ), true )]
	[Feature( EDITOR ), Group( ID ), Order( EDITOR_ORDER - 100 )]
	protected SteamId? InspectorOwner
	{
		get => Owner;
		set => Owner = value;
	}

	/// <summary>
	/// The client this entity belongs to.
	/// Probably the one that spawn/requested it.
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	public SteamId? Owner { get; protected set; }

	/// <summary>
	/// Should this use hacks to fix physics bugs once added to an island?
	/// This unfortunately breaks joints for now, so don't if you can help it.
	/// </summary>
	public virtual bool RefreshPhysicsUponJoin => true;

	/// <summary>
	/// Should the fact this exists as a member prevent the object
	/// from vanishing when it looks for reasons not to kill itself?
	/// </summary>
	public virtual bool IsWorthwhile => true;

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( Island.IsValid() )
			Island.OnObjectDestroyed( this );

		AutoCleanup();
	}

	protected override void OnStart()
	{
		base.OnStart();

		RefreshIsland();
	}

	protected override void OnParentChanged( GameObject oldParent, GameObject newParent )
	{
		base.OnParentChanged( oldParent, newParent );

		RefreshIsland( newParent );
	}

	/// <summary>
	/// Destroys this object if this entity was removed
	/// from an object with no other remaining entities.
	/// </summary>
	protected virtual void AutoCleanup()
	{
		if ( !Scene.InGame() || !GameObject.IsValid() )
			return;

		// Auto-cleanup islands that don't have any other objects.
		const FindMode findMode = FindMode.EnabledInSelfAndDescendants;

		var reasonsToLive = GameObject.Components.GetAll<EditorObject>( findMode )
			.Any( e => e.IsValid() && e.IsWorthwhile );

		if ( !reasonsToLive )
			GameObject.Destroy();
	}

	public virtual void RenderHelpers()
	{
	}
}
