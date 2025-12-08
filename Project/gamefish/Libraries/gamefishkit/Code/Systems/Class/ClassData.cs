namespace GameFish;

public partial struct ClassData : IValid
{
	public const string ID_ERROR = "ERROR";
	public const string NAME_DEFAULT = "???";

	/// <summary>
	/// If this class
	/// </summary>
	public readonly bool IsValid => ID != ID_ERROR && !ID.IsBlank()
		&& Prefab.IsValid() && Class.IsValid();

	public string ID { get; set; } = ID_ERROR;

	/// <summary>
	/// The prefab representing this class.
	/// </summary>
	public PrefabFile Prefab { get; set; }

	/// <summary>
	/// The <see cref="GameFish.Class"/> derived component.
	/// </summary>
	public Class Class { get; set; }

	/// <summary>
	/// The display name for this class.
	/// </summary>
	public readonly string Name => Class?.ClassName.IsBlank() is not true
		? Class.ClassName ?? NAME_DEFAULT
		: NAME_DEFAULT;

	public ClassData() { }

	public ClassData( PrefabFile prefab, Class c )
	{
		ID = c.IsValid() && !c.ClassId.IsBlank() ? c.ClassId : ID_ERROR;

		Prefab = prefab;
		Class = c;
	}

	public ClassData( string id, PrefabFile prefab, Class c )
	{
		ID = id;

		Prefab = prefab;
		Class = c;
	}
}
