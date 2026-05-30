namespace GameFish;

partial class Entity : ITransform
{
	public virtual Vector3 Center => WorldPosition;

	/// <summary>
	/// Allows for custom teleportation behavior.
	/// </summary>
	/// <remarks> Example: telling a pawn to set their eye rotation instead. </remarks>
	/// <returns> If the teleportation was successful. </returns>
	public virtual bool TryTeleport( in Transform tWorld )
		=> false;

	/// <summary>
	/// Allows the host to teleport this.
	/// </summary>
	/// <remarks> Supports custom behavior such as setting a pawn's eye rotation. </remarks>
	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostTeleport( Transform tWorld )
		=> TryTeleport( in tWorld );
}
