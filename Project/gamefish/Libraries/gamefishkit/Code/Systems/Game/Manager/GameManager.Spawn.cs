namespace GameFish;

partial class GameManager
{
	protected override void OnStart()
	{
		base.OnStart();

		// Auto-select state.
		if ( AutoStateSelection )
			SelectState();
	}

	protected override void OnEnabled()
	{
		if ( !Networking.IsHost )
		{
			base.OnEnabled();
			return;
		}

		// Prevent multiple instances.
		if ( TryGetInstance( out var gm ) && gm != this )
		{
			this.Warn( "Tried to create a duplicate! Self-destructing." );
			DestroyGameObject();
			return;
		}

		if ( !GameObject.IsValid() )
			return;

		base.OnEnabled();
	}

	/// <summary>
	/// Spawn a game manager if one does not already exist.
	/// </summary>
	/// <param name="prefab"> The prefab to use. </param>
	/// <param name="gm"> The manager component(or null). </param>
	/// <returns> If a new(not existing) manager could be spawned. </returns>
	public static bool TryCreate( PrefabFile prefab, out GameManager gm )
	{
		// Don't replace the manager on accident.
		if ( TryGetInstance( out gm ) )
			return false;

		if ( !Networking.IsHost )
			return false;

		// Scene-specific settings may block spawning the game manager.
		if ( SceneSettings.TryGetInstance( out var s ) && !s.SpawnGameManager )
		{
			Print.WarnFrom( typeof( GameManager ), "Settings manager blocked spawning." );
			return false;
		}

		if ( !prefab.IsValid() )
		{
			Print.WarnFrom( typeof( GameManager ), $"Missing/invalid prefab:[{prefab}]" );
			return false;
		}

		if ( !prefab.TrySpawn( out var gmObj ) || !gmObj.Components.TryGet( out gm ) )
		{
			Print.WarnFrom( typeof( GameManager ), $"Couldn't find type:[{typeof( GameManager )}] on manager prefab:[{prefab}]. Destroying." );
			gmObj.Destroy();
			return false;
		}

		// Print.InfoFrom( typeof( GameManager ), $"New game manager:[{gm}]" );

		return true;
	}
}
