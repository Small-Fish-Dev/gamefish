namespace Playground;

/// <summary>
/// Something that can be controlled and wired.
/// </summary>
public partial class Device : EditorEntity, IWired
{
	protected const int PHYSICS_ORDER = EDITOR_ORDER + 10;

	public const int WIRE_LIMIT = 16;

	/// <summary>
	/// Wires connecting this to other entities.
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	public NetDictionary<Entity, Vector3> Wires { get; protected set; }

	/// <summary>
	/// How many valid connections have been made?
	/// </summary>
	public int WireCount => Wires?.Count( kv => kv.Key.IsValid() ) ?? 0;

	/// <summary>
	/// Is this not under the wire count it allows?
	/// </summary>
	public virtual bool TooManyWires => WireCount >= WIRE_LIMIT;

	/// <returns> If these two are compatible. </returns>
	public virtual bool CanWire( Entity to )
	{
		if ( !to.IsValid() )
			return false;

		return to is IWired;
	}

	/// <summary>
	/// Allows the host to wire this up.
	/// </summary>
	public virtual bool TryWire( Entity to, in Vector3 localPos, Client cl = null )
	{
		if ( !Networking.IsHost )
		{
			this.Warn( $"Tried wiring to:[{to}] as a non-host!" );
			return false;
		}

		if ( TooManyWires )
			return false;

		if ( !CanWire( to ) )
			return false;

		Wires ??= [];

		if ( Wires is not null )
		{
			Wires[to] = localPos;
			return true;
		}

		return false;
	}

	[Rpc.Host( NetFlags.Reliable | NetFlags.SendImmediate )]
	public virtual void RpcRequestWire( Entity to, Vector3 localPos )
	{
		if ( !to.IsValid() )
			return;

		if ( !Server.TryFindClient( Rpc.Caller, out var cl ) )
			return;

		TryWire( to, localPos, cl );
	}
}
