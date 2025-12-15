namespace Playground;

public partial class RemoverTool : EditorTool
{
	/// <summary>
	/// Can pawns of any kind be removed?
	/// </summary>
	[Property]
	[Sync( SyncFlags.FromHost )]
	[Feature( EDITOR ), Group( SETTINGS ), Order( SETTINGS_ORDER )]
	public bool AllowPawns { get; set; } = true;

	public override void FixedSimulate( in float deltaTime )
	{
		if ( Input.Pressed( "Attack1" ) )
			TryRemoveTarget();
	}

	protected virtual bool TryRemoveTarget()
	{
		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( !Editor.TryTrace( Scene, out var tr ) )
			return false;

		if ( !CanRemove( Client.Local, tr.GameObject ) )
			return false;

		RpcTryRemoveObject( tr.GameObject );
		return true;
	}

	public virtual bool CanRemove( Client cl, GameObject obj )
	{
		if ( !obj.IsValid() )
			return false;

		// Can't ever remove the map itself with this tool.
		if ( obj.GetComponent<MapCollider>( includeDisabled: true ).IsValid() )
			return false;

		// Can't remove connected player-owned pawns.
		if ( Pawn.TryGet<Pawn>( obj, out var pawn ) )
			if ( !CanRemovePawn( cl, pawn ) )
				return false;

		return true;
	}

	protected virtual bool CanRemovePawn( Client cl, Pawn pawn )
	{
		if ( !AllowPawns )
			return false;

		if ( !pawn.IsValid() || !pawn.Owner.IsValid() )
			return true;

		// Only hosts can remove player pawns.
		// TODO: Admin system for organizing this.
		if ( pawn.Owner.IsPlayer && pawn.Owner.Connected )
			return cl?.Connection?.IsHost is true;

		return true;
	}

	[Rpc.Host( NetFlags.Reliable )]
	protected void RpcTryRemoveObject( GameObject obj )
	{
		if ( !TryUse( Rpc.Caller, out var cl ) )
			return;

		if ( !CanRemove( cl, obj ) )
			return;

		obj.Destroy();
	}
}
