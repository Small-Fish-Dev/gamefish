namespace GameFish;

/// <summary>
/// An entity supporting modules that you can access from anywhere.
/// </summary>
/// <typeparam name="TComp"> The component you want to be a singleton. </typeparam>
public abstract class Singleton<TComp> : ModuleEntity where TComp : ModuleEntity
{
	protected override bool? IsNetworkedOverride => null;

	protected override NetworkMode NetworkingModeDefault => NetworkMode.Object;
	protected override OwnerTransfer NetworkTransferModeDefault => OwnerTransfer.Fixed;
	protected override NetworkOrphaned NetworkOrphanedModeDefault => NetworkOrphaned.Host;

	public override Connection DefaultNetworkOwner => Connection.Host;

	[SkipHotload]
	private static TComp _instance;

	/// <summary>
	/// The first active non-editor instance of <typeparamref name="TComp"/>.
	/// </summary>
	public static TComp Instance
	{
		get => _instance.GetSingleton();
		protected set => _instance = value;
	}

	/// <summary>
	/// The first active instance of <typeparamref name="TComp"/>. <br />
	/// This works even in in the editor.
	/// </summary>
	public static TComp EditorInstance
	{
		get => _instance.GetSingleton( allowEditor: true );
		protected set => _instance = value;
	}

	/// <summary>
	/// Tries to find an instance of <typeparamref name="TComp"/>
	/// without any respect to classes inheriting it.
	/// </summary>
	/// <remarks>
	/// If you are inheriting a singleton then use <see cref="TryGetInstance{T}"/>
	/// to have it be returned as that class.
	/// </remarks>
	/// <param name="inst"> The <typeparamref name="TComp"/>(or null). </param>
	/// <returns> If the instance of <typeparamref name="TComp"/> could be found. </returns>
	public static bool TryGetInstance( out TComp inst )
		=> (inst = Instance).IsValid();

	/// <summary>
	/// Tries to cast the singleton's instance to a type that inherits it.
	/// Useful in cases where you are inheriting a base singleton component.
	/// </summary>
	/// <remarks>
	/// This version lets you use what's on your custom type directly.
	/// </remarks>
	/// <param name="inst"> The <typeparamref name="T"/>(or null). </param>
	/// <returns> If the instance of <typeparamref name="T"/> could be found. </returns>
	public static bool TryGetInstance<T>( out T inst ) where T : TComp
		=> (inst = Instance as T).IsValid();

	/// <summary>
	/// Tries to cast the singleton's instance to a type that inherits it.
	/// Useful in cases where you are inheriting a base singleton component.
	/// </summary>
	/// <returns> The valid instance of this singleton casted to this type(or null). </returns>
	public static T GetInstance<T>() where T : TComp
		=> Instance is T inst && inst.IsValid() ? inst : null;
}
