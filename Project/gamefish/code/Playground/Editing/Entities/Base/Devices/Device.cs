namespace Playground;

/// <summary>
/// Something that can be controlled and wired.
/// </summary>
public abstract partial class Device : EditorEntity, IWired
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
	/// Does this have too many active connections for a new one?
	/// </summary>
	public virtual bool TooManyWires => WireCount >= WIRE_LIMIT;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		SimulateWires( Time.Delta, isFixedUpdate: false );
	}

	protected virtual void SimulateWires( in float deltaTime, in bool isFixedUpdate )
	{
		if ( Wires is null || Wires.Count == 0 )
			return;

		foreach ( var (ent, _) in Wires )
			if ( ent.IsValid() && ent is IWired wire )
				SimulateWire( wire, in deltaTime, in isFixedUpdate );
	}

	/// <summary>
	/// Update a particular connection to something else.
	/// </summary>
	protected virtual void SimulateWire( IWired wire, in float deltaTime, in bool isFixedUpdate )
		=> wire?.WireSimulate( this, in deltaTime, in isFixedUpdate );

	/// <summary>
	/// Show active connections.
	/// </summary>
	public virtual void DrawWireGizmos()
	{
		if ( Wires is null || Wires.Count == 0 )
			return;

		var center = Center;
		var c = Color.Black.WithAlpha( 2f );

		foreach ( var (ent, localPos) in Wires )
		{
			if ( !ent.IsValid() )
				continue;

			var plug = ent.WorldTransform.PointToWorld( localPos );

			this.DrawArrow(
				from: center, to: plug,
				c: c, len: 7f, w: 2f, th: 4f,
				tWorld: global::Transform.Zero
			);
		}
	}

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

		if ( Wires is null )
			return false;

		Wires[to] = localPos;

		return true;
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

	public virtual void WireSimulate( Device device, in float deltaTime, in bool isFixedUpdate )
	{
	}
}
