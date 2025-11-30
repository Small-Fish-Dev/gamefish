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
public partial class Essential : Singleton<Essential>
{
	protected const int BOOT_ORDER = DEBUG_ORDER - 1000;

	protected const int GAME_ORDER = BOOT_ORDER + 10;
	protected const int SCENES_ORDER = BOOT_ORDER + 20;

	/// <summary>
	/// The prefab with a <see cref="Session"/> component on it.
	/// If defined here then the prefab will be created initially
	/// if one does not already exist.
	/// <br /> <br />
	/// <b> NOTE: </b> You want one of these to persist data between loading of scenes.
	/// </summary>
	[Title( "Session Prefab" )]
	[Property, Order( BOOT_ORDER )]
	[Feature( BOOT ), Group( SESSION )]
	public virtual PrefabFile SessionPrefab { get; set; }

	/// <summary>
	/// The prefab with a <see cref="GameManager"/> component on it.
	/// If defined here then it will be spawned automatically.
	/// </summary>
	[Title( "Manager Prefab" )]
	[Property, Order( GAME_ORDER )]
	[Feature( BOOT ), Group( GAME )]
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

		if ( !Networking.IsHost || !this.InGame() )
			return;

		EnsureSession();

		EnsureGameManager();
	}

	/// <summary>
	/// Creates the <see cref="Session"/> prefab if one doesn't exist.
	/// </summary>
	[Order( BOOT_ORDER + 1 )]
	[Button( "Create Session" )]
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
	/// Creates the <see cref="Session"/> prefab if one doesn't exist.
	/// </summary>
	[Order( GAME_ORDER + 1 )]
	[Button( "Create Manager" )]
	[Feature( BOOT ), Group( GAME )]
	[ShowIf( nameof( InGame ), true )]
	public virtual void EnsureGameManager()
	{
		if ( !this.InGame() || !Networking.IsHost )
			return;

		if ( GameManager.TryGetInstance( out _ ) )
			return;

		// Scene settings might override the prefab.
		var prefab = GameManagerPrefab;

		var s = SceneSettings.Instance;

		if ( s.IsValid() )
		{
			// Might also block spawning the game manager.
			if ( !s.SpawnGameManager )
				return;

			if ( s.GameManagerPrefabOverride.IsValid() )
				prefab = s.GameManagerPrefabOverride;
		}

		if ( prefab.IsValid() )
			GameManager.TryCreate( prefab, out var gm );
	}
}
