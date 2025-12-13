using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// A module for components. Registers itself. <br />
/// These are entities that can automatically network the object they're on.
/// </summary>
[Icon( "extension" )]
public abstract partial class Module : ModuleEntity, Component.ExecuteInEditor
{
	public const string MODULE = "🧩 Module";
	public const int MODULE_ORDER = NETWORK_ORDER - 1;

	/// <summary>
	/// The singular parent entity this is registered to.
	/// </summary>
	[Title( "Parent" )]
	[Property, ReadOnly, JsonIgnore]
	[Feature( ENTITY ), Group( MODULE ), Order( MODULE_ORDER )]
	protected ModuleEntity InspectorParent => Parent;

	/// <summary>
	/// The singular parent entity this is registered to.
	/// </summary>
	public ModuleEntity Parent { get; protected set; }

	/// <returns> The first targeted parent entity in our hierarchy(or null, not cached). </returns>
	protected virtual ModuleEntity FindParent()
		=> Components?.GetAll<ModuleEntity>( FindMode.EverythingInSelfAndAncestors )
			.FirstOrDefault( p => p.IsValid() && IsParent( p ) );

	/// <returns> If this module is meant to target that component. </returns>
	public abstract bool IsParent( ModuleEntity comp );

	protected override void OnParentChanged( GameObject oldParent, GameObject newParent )
	{
		base.OnParentChanged( oldParent, newParent );

		// this.Log( $"Parent changed, old:[{oldParent}] new:[{newParent}]" );

		if ( !TryRegisterModule() )
			RemoveModule();
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		if ( !GameObject.IsValid() )
			return;

		if ( !Parent.IsValid() && Network.Active )
			TryRegisterModule();
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( !Parent.IsValid() )
			TryRegisterModule();
	}

	protected override void OnDisabled()
	{
		base.OnDisabled();

		RemoveModule();
	}

	/// <returns> If the module was newly(or already) registered. </returns>
	public bool TryRegisterModule()
	{
		if ( !GameObject.IsValid() )
			return false;

		var newParent = FindParent();

		if ( !newParent.IsValid() )
		{
			OnRegistrationFailure( null );
			return false;
		}

		// Parent is defined on successful register.
		if ( Parent.IsValid() && Parent == newParent )
			return true;

		// Let 'em know we failed.
		if ( !newParent.TryRegisterModule( this ) )
		{
			OnRegistrationFailure( newParent );
			return false;
		}

		return true;
	}

	/// <summary>
	/// Removes this module from the parent and clears the parent.
	/// </summary>
	public void RemoveModule()
	{
		if ( Parent.IsValid() )
			Parent.RemoveModule( this );

		// Clear the cached value.
		Parent = null;
	}

	/// <summary>
	/// Called when we tried to register on a parent but failed.
	/// </summary>
	/// <param name="parent"> The target parent(or null if if none found). </param>
	protected virtual void OnRegistrationFailure( ModuleEntity parent )
	{
		// Ensure no network owner upon failure.
		if ( Network?.Owner is not null )
			TrySetNetworkOwner( null, allowProxy: false );
	}

	/// <summary>
	/// Called when this has been successfully registered onto a parent component.
	/// </summary>
	public virtual void OnRegistered( ModuleEntity parent )
	{
		if ( !parent.IsValid() )
			return;

		Parent = parent;

		var cn = parent?.Network?.Owner;

		if ( cn is not null )
			TrySetNetworkOwner( cn );
	}

	/// <summary>
	/// Called when this has been removed from a parent component.
	/// </summary>
	public virtual void OnRemoved( ModuleEntity parent )
	{
		if ( Parent == parent )
			Parent = null;
	}
}
