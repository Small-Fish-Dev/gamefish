namespace GameFish;

/// <summary>
/// Provides essential references and ensures that a
/// <see cref="Session"/> exists(if a prefab is set).
/// <br /> <br />
/// <b> USAGE: </b> Put this in the "System Scene"
/// which is specified in your project settings.
/// Inherit this class to add your own important things.
/// </summary>
[Icon( "power_settings_new" )]
public partial class Essential : Singleton<Essential>, ISceneLoadingEvents
{
	protected const int BOOT_ORDER = DEBUG_ORDER - 1000;

	protected const int PREFABS_ORDER = BOOT_ORDER - 1;
	protected const int SCENES_ORDER = BOOT_ORDER + 5;


	/// <summary>
	/// The prefab with a <see cref="Session"/> component on it.
	/// If defined here then the prefab will be created initially
	/// if one does not already exist.
	/// <br /> <br />
	/// <b> NOTE: </b> You want one of these to persist data between loading of scenes.
	/// </summary>
	[Property]
	[Title( "Session" )]
	[Feature( BOOT ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public virtual PrefabFile SessionPrefab { get; set; }

	/// <summary>
	/// The prefab with a <see cref="GameManager"/> component on it.
	/// If defined here then it will be spawned at each
	/// scene start if one does not already exist.
	/// <br /> <br />
	/// <b> NOTE: </b> Has state modules for both main menu
	/// and ingame scenes to handle logic for each of them.
	/// </summary>
	[Property]
	[Title( "Game Manager" )]
	[Feature( BOOT ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public virtual PrefabFile GameManagerPrefab { get; set; }

	/// <summary>
	/// The scene you want to go to for features like level selection.
	/// </summary>
	[Title( "Main Menu" )]
	[Property, Order( SCENES_ORDER )]
	[Feature( BOOT ), Group( SCENES )]
	public virtual SceneFile MainMenuScene { get; set; }

	/// <summary>
	/// The default/fallback scene for playing the game(if applicable).
	/// </summary>
	[Title( "Gameplay" )]
	[Property, Order( SCENES_ORDER )]
	[Feature( BOOT ), Group( SCENES )]
	public virtual SceneFile GameScene { get; set; }

	/// <summary>
	/// The scene to load for testing and/or debugging purposes.
	/// </summary>
	[Title( "Testing" )]
	[Property, Order( SCENES_ORDER )]
	[Feature( BOOT ), Group( SCENES )]
	public virtual SceneFile TestingScene { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		EnsureEssentials();
	}

	void ISceneLoadingEvents.AfterLoad( Scene scene )
	{
		if ( scene.IsValid() )
			OnSceneLoad( scene );
	}

	protected virtual void OnSceneLoad( Scene scene )
	{
		EnsureEssentials();
	}

	protected virtual void EnsureEssentials()
	{
		if ( !Networking.IsHost || !this.InGame() )
			return;

		EnsureSession();
		EnsureGameManager();
	}

	/// <summary>
	/// Spawns the <see cref="Session"/> prefab if an instance doesn't exist.
	/// </summary>
	[Order( PREFABS_ORDER )]
	[Button( "Ensure Session" )]
	[ShowIf( nameof( InGame ), true )]
	[Feature( BOOT ), Group( SESSION )]
	public virtual void EnsureSession()
	{
		if ( !this.InGame() || !Networking.IsHost )
			return;

		if ( Session.TryGetInstance( out _ ) )
			return;

		if ( SessionPrefab.IsValid() )
			Session.TryCreate( SessionPrefab, out _ );
	}

	/// <summary>
	/// Spawns the <see cref="GameManager"/> prefab if an instance doesn't exist.
	/// </summary>
	[Button( "Ensure Manager" )]
	[ShowIf( nameof( InGame ), true )]
	[Feature( BOOT ), Group( PREFABS ), Order( PREFABS_ORDER )]
	public virtual void EnsureGameManager()
	{
		if ( !this.InGame() || !Networking.IsHost )
			return;

		if ( GameManager.TryGetInstance( out _ ) )
			return;

		var prefab = GameManagerPrefab;

		// Scene settings might override the prefab.
		if ( SceneSettings.TryGetInstance( out var s ) )
			if ( s.GameManagerPrefabOverride.IsValid() )
				prefab = s.GameManagerPrefabOverride;

		if ( prefab.IsValid() )
			GameManager.TryCreate( prefab, out var _ );
	}
}
