namespace GameFish;

/// <summary>
/// The most basic form of something that can separately exist.
/// </summary>
[Icon( "data_object" )]
public abstract partial class Entity : Class
{
	protected const int DEBUG_ORDER = DEFAULT_ORDER - 100;
	protected const int ENTITY_ORDER = DEFAULT_ORDER + 100;
	protected const int NETWORK_ORDER = ENTITY_ORDER + 1;

	/// <summary>
	/// Is this currently loaded in a valid editor scene? <br />
	/// You can use this with <see cref="HideIfAttribute"/> or <see cref="ShowIfAttribute"/>.
	/// </summary>
	public bool InEditor => this.InEditor();

	/// <summary>
	/// Is this currently loaded in a valid play mode scene? <br />
	/// You can use this with <see cref="HideIfAttribute"/> or <see cref="ShowIfAttribute"/>.
	/// </summary>
	public bool InGame => this.InGame();

	/// <summary>
	/// A consistent way of getting an entity-derived class from an object.
	/// </summary>
	/// <returns> If the entity was found. </returns>
	public static bool TryGet<TEntity>( GameObject obj, out TEntity ent, FindMode findMode = FindMode.EnabledInSelf | FindMode.InAncestors )
		where TEntity : Entity
	{
		if ( !obj.IsValid() )
		{
			ent = null;
			return false;
		}

		return obj.Components.TryGet( out ent, findMode );
	}
}
