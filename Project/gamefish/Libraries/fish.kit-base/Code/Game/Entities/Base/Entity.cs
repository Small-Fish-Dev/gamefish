namespace GameFish;

/// <summary>
/// The most basic form of something that can separately exist.
/// </summary>
[Icon( "data_object" )]
public abstract partial class Entity : BaseClass, ITransform
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
	/// Allows for custom teleportation behavior.
	/// </summary>
	/// <remarks> Example: telling a pawn to set their eye rotation instead. </remarks>
	/// <returns> If the teleportation was successful. </returns>
	public virtual bool TryTeleport( in Transform tWorld )
		=> false;
}
