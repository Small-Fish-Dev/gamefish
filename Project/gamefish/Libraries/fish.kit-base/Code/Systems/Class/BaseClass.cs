namespace GameFish;

/// <summary>
/// A component that when placed at the root of a prefab can identify it globally.
/// This lets you easily spawn it or read the prefab's components from anywhere.
/// <br />
/// <br /> ⚠
/// <br /> Be careful not to have duplicate class IDs!
/// <br /> ⚠
/// </summary>
[Icon( "local_offer" )]
[Group( Library.NAME )]
public abstract partial class BaseClass : Component
{
	protected const int DEFAULT_ORDER = -1000;

	/// <summary>
	/// Enables uniquely identifying this entity. <br />
	/// Makes it possible to spawn this prefab in
	/// commands etc. by specifying its string ID.
	/// </summary>
	[Title( "Is Class" )]
	[Property, Order( DEFAULT_ORDER )]
	[ShowIf( nameof( IsPrefabRoot ), true )]
	[Feature( ENTITY ), Group( ID )]
	public virtual bool IsClass { get; set; }

	/// <summary>
	/// Uniquely identifies this entity. <br />
	/// Makes it possible to spawn this prefab by specifying this.
	/// </summary>
	[Title( "Class ID" )]
	[Property, Order( DEFAULT_ORDER )]
	[ShowIf( nameof( ShowClassSettings ), true )]
	[Feature( ENTITY ), Group( ID )]
	public string ClassId
	{
		get
		{
			if ( IsPrefabRoot && string.IsNullOrWhiteSpace( _classId ) )
				return _classId = GameObject.Name;

			return _classId;
		}
		set => _classId = value;
	}

	protected string _classId;

	/// <summary>
	/// What to display as this entity prefab's name.
	/// </summary>
	[Title( "Class Name" )]
	[Property, Order( DEFAULT_ORDER )]
	[ShowIf( nameof( ShowClassSettings ), true )]
	[Feature( ENTITY ), Group( ID )]
	public string ClassName { get; set; }

	/// <summary>
	/// Sets the class ID to the prefab's file name.
	/// </summary>
	[Title( "Reset" )]
	[Button, Order( DEFAULT_ORDER )]
	[ShowIf( nameof( ShowClassSettings ), true )]
	[Feature( ENTITY ), Group( ID )]
	protected virtual void ResetClassID()
		=> ClassId = GameObject.Name;

	protected bool ShowClassSettings => IsClass && IsPrefabRoot;

	public bool IsPrefabRoot => GameObject is PrefabScene;
}
