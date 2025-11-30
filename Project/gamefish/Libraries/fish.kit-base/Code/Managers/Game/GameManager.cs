using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Handles your core gameplay logic.
/// <br /> <br />
/// <b> NOTE: </b> You are encouraged to inherit and override this component.
/// </summary>
[Title( "Game Manager" )]
[Icon( "videogame_asset" )]
public abstract partial class GameManager : Singleton<GameManager>, ISceneLoadingEvents
{
	protected const int GAME_ORDER = DEFAULT_ORDER - 1000;

	protected const int NAME_ORDER = GAME_ORDER - 10;
	protected const int NAV_MESH_ORDER = GAME_ORDER + 5;

	protected new const int DEBUG_ORDER = GAME_ORDER - 100;

	/// <summary>
	/// What's your game called?
	/// </summary>
	[Property, WideMode]
	[Order( NAME_ORDER )]
	[Feature( GAME, Description = "Some important stuff." )]
	protected virtual string Name { get; set; }

	/// <summary>
	/// Is this loaded in a valid play mode scene?
	/// </summary>
	[Title( "In Game" )]
	[Property, ReadOnly, JsonIgnore]
	[Feature( GAME ), Group( DEBUG ), Order( DEBUG_ORDER - 1 )]
	protected bool InspectorInGame => InGame;

	/// <summary>
	/// Are we in a menu scene?
	/// <br /> <br />
	/// <b> TIP: </b> Add <see cref="SceneSettings"/> to the scene.
	/// </summary>
	[Title( "In Main Menu" )]
	[Property, ReadOnly, JsonIgnore]
	// [ShowIf( nameof( InGame ), true )]
	[Feature( GAME ), Group( DEBUG ), Order( DEBUG_ORDER - 1 )]
	protected bool InspectorInMainMenu => InMainMenu;

	/// <summary>
	/// Are we in a menu scene?
	/// <br /> <br />
	/// <b> TIP: </b> Add <see cref="SceneSettings"/> to the scene.
	/// </summary>
	public static bool InMainMenu => SceneSettings.InMainMenu;

	/// <summary>
	/// Does the current scene have an enabled nav mesh?
	/// </summary>
	[Title( "Is Enabled" )]
	[Property, ReadOnly, JsonIgnore]
	[Feature( GAME ), Group( NAV_MESH ), Order( NAV_MESH_ORDER )]
	public bool IsNavMeshEnabled => Scene?.NavMesh?.IsEnabled is true;

	/// <summary>
	/// Override if the nav mesh is enabled when the scene is loaded?
	/// </summary>
	[Property]
	[Title( "Override" )]
	[Feature( GAME ), Group( NAV_MESH ), Order( NAV_MESH_ORDER )]
	protected virtual bool? NavMeshOverride { get; set; } = null;

	void ISceneLoadingEvents.AfterLoad( Scene scene )
	{
		if ( scene.IsValid() )
			OnSceneLoad( scene );
	}

	public virtual void OnSceneLoad( Scene scene )
	{
		if ( NavMeshOverride.HasValue )
			OverrideNavMesh( isEnabled: true );
	}

	public virtual void OverrideNavMesh( bool isEnabled, Scene scene = null )
	{
		scene ??= Scene;

		if ( scene?.NavMesh is not null )
			scene.NavMesh.IsEnabled = NavMeshOverride.Value;
	}
}
