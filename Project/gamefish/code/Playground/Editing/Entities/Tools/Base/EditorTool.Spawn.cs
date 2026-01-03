namespace Playground;

partial class EditorTool
{
	public virtual bool TryCreateIsland( in Transform tWorld, out EditorIsland island )
	{
		island = null;

		if ( !Scene.InGame() || !Editor.IsValid() )
			return false;

		if ( !Editor.IslandPrefab.TrySpawn( tWorld, out var obj ) )
		{
			this.Warn( $"Missing island prefab on {typeof( Editor )}!" );
			return false;
		}

		obj.WorldTransform = tWorld.WithScale( Vector3.One );
		obj.Transform.ClearInterpolation();

		island = obj.Components.Get<EditorIsland>();

		if ( !island.IsValid() )
		{
			this.Warn( $"Missing island prefab on prefab:{Editor.IslandPrefab}!" );
			obj.DestroyImmediate();
			return false;
		}

		return true;
	}

	public bool TrySpawnObject( PrefabFile prefab, in Transform tWorld, GameObject objParent, out EditorObject e )
		=> TrySpawnObject( prefab, tWorld, Editor.FindIsland( objParent ), out e );

	public bool TrySpawnObject( PrefabFile prefab, in Transform tWorld, EditorIsland parent, out EditorObject e )
	{
		var cfg = ObjectSpawnConfig.FromWorld( prefab, parent, tWorld );
		return TrySpawnObject( in cfg, out e );
	}

	public bool TrySpawnObject( PrefabFile prefab, in Transform tWorld, out EditorObject e, bool withIsland = false )
	{
		var cfg = new ObjectSpawnConfig( prefab, tWorld );

		if ( withIsland )
			cfg = cfg.WithIsland( isEnabled: true, island: null );

		return TrySpawnObject( cfg, out e );
	}

	public virtual bool TrySpawnObject( in ObjectSpawnConfig cfg, out EditorObject e )
	{
		e = null;

		// Permission check.
		if ( !IsClientAllowed( Client.Local ) )
			return false;

		// Clone the prefab.
		if ( !cfg.TryGetWorldTransform( out var tWorld ) )
			return false;

		if ( !cfg.Prefab.TrySpawn( tWorld, out var eObj ) )
			return false;

		if ( !eObj.Components.TryGet( out e, FindMode.EnabledInSelf ) )
		{
			this.Warn( $"No {typeof( EditorObject )} on prefab:[{cfg.Prefab}]!" );

			eObj.DestroyImmediate();
			return false;
		}

		e.SetupNetworking( force: true );

		// 🏝
		EditorIsland island = null;

		if ( cfg.InGroup )
		{
			island = cfg.Island;

			// Create a new island if configured to.
			if ( !island.IsValid() && !TryCreateIsland( tWorld, out island ) )
			{
				this.Warn( $"Couldn't create island! Preventing object:[{eObj}]" );
				eObj.DestroyImmediate();
				return false;
			}

			eObj.SetParent( island.GameObject, keepWorldPosition: true );

			if ( cfg.IsTransformLocal )
				eObj.LocalTransform = cfg.Transform;
		}

		eObj.Transform.ClearInterpolation();

		OnObjectSpawned( e, island );

		// if ( parent.IsValid() )
		// parent.RpcBroadcastRefreshPhysics();

		return true;
	}

	protected virtual void OnObjectSpawned( EditorObject e, EditorIsland parent )
	{
	}
}
