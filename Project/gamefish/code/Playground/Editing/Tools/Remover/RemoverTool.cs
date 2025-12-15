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

	public override void FrameSimulate( in float deltaTime )
	{
		if ( Input.Pressed( "Attack1" ) )
			TryRemoveTarget();
	}

	protected virtual bool TryRemoveTarget()
	{
		if ( !IsClientAllowed( Client.Local ) )
			return false;

		if ( !TryTrace( out var tr ) || !tr.Hit )
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

		// Can't remove spectators using a tool.
		if ( pawn is Spectator )
			return false;

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

		// Put players into spectator mode.
		if ( Pawn.TryGet( obj, out var pawn ) && pawn.Owner is Client clTarget )
		{
			if ( pawn is Spectator )
				return;

			if ( clTarget.IsValid() && clTarget.Connected )
			{
				// Tell the state to force them to spectate.
				if ( GameState.TryGetCurrent( out var state ) )
					state.TryAssignSpectator( clTarget, out _, force: true, oldCleanup: true );

				// If that failed then prevent destruction.
				return;
			}
		}

		if ( obj.IsValid() )
			obj.Destroy();
	}
}
