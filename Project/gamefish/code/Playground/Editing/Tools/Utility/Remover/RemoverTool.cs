namespace Playground;

public partial class RemoverTool : EditorTool
{
	protected override void OnPrimary( in SceneTraceResult tr )
	{
		base.OnPrimary( in tr );

		if ( !IsClientAllowed( Client.Local ) )
			return;

		if ( !TryGetTargetComponent( in tr, out var target ) )
			return;

		if ( !CanRemove( Client.Local, target?.GameObject ) )
			return;

		RpcRemoveObject( target?.GameObject );
	}

	public override bool TryGetTargetComponent( in SceneTraceResult tr, out Component target )
	{
		target = null;

		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return false;

		if ( HoldingShift && Editor.TryFindIsland( tr.GameObject, out var island ) )
		{
			target = island;
			return true;
		}

		if ( tr.Collider.IsValid() && tr.Collider is not MapCollider )
		{
			target = tr.Collider;
			return true;
		}

		return base.TryGetTargetComponent( tr, out target );
	}

	protected override bool TryGetTargetEntity( in SceneTraceResult tr, out EditorObject e )
	{
		e = null;
		return false;
	}

	public virtual bool CanRemove( Client cl, GameObject obj )
	{
		if ( !obj.IsValid() )
			return false;

		// Can only remove editor objects.
		if ( !obj.Components.TryGet<EditorObject>( out _, FindMode.EverythingInSelfAndAncestors ) )
			return false;

		// Can never remove maps.
		if ( obj.Components.TryGet<MapInstance>( out _, FindMode.EverythingInSelfAndAncestors | FindMode.InDescendants ) )
			return false;

		// Can't remove connected player-owned pawns.
		if ( Pawn.TryGet<Pawn>( obj, out var pawn ) )
			if ( !CanRemovePawn( cl, pawn ) )
				return false;

		return true;
	}

	protected virtual bool CanRemovePawn( Client cl, Pawn pawn )
	{
		// I suppose the job is already done?
		if ( !pawn.IsValid() )
			return true;

		// Can't ever remove spectators using a tool.
		if ( pawn is Spectator )
			return false;

		// Can't ever remove player-owned pawns.
		if ( pawn.IsPlayer )
			return false;

		return true;
	}

	[Rpc.Host( NetFlags.Reliable )]
	protected void RpcRemoveObject( GameObject obj )
	{
		if ( !TryUse( Rpc.Caller, out var cl ) )
			return;

		if ( !obj.IsValid() )
			return;

		const FindMode findMode = FindMode.EnabledInSelf | FindMode.InAncestors;

		if ( !obj.Components.TryGet<EditorObject>( out var e, findMode ) )
			return;

		obj = e.GameObject;

		if ( !CanRemove( cl, obj ) )
		{
			this.Warn( $"Client:[{cl}] failed to remove obj:[{obj}]" );
			return;
		}

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
