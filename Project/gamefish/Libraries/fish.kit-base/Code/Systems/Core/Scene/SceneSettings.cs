namespace GameFish;

/// <summary>
/// Important settings for this scene that you are placing it in.
/// <br /> <br />
/// <b> IMPORTANT: </b> Place it in your playable scenes directly to configure
/// them, not anywhere else such as as a prefab or your system scene.
/// </summary>
[Icon( "map" )]
public partial class SceneSettings : Singleton<SceneSettings>
{
	protected const int SETTINGS_ORDER = DEFAULT_ORDER - 777;

	protected const int UI_ORDER = SETTINGS_ORDER - 10;
	protected const int GAME_ORDER = SETTINGS_ORDER - 5;

	protected const int BOUNDS_ORDER = SETTINGS_ORDER + 10;
	protected const int DEPTH_ORDER = BOUNDS_ORDER + 5;

	protected override bool? IsNetworkedOverride => true;

	/// <summary>
	/// Prevents player spawn and game logic.
	/// Should probably open your menu, too.
	/// </summary>
	[Property]
	[Feature( SETTINGS ), Group( UI ), Order( UI_ORDER )]
	public bool IsMainMenu { get; set; } = false;

	/// <summary>
	/// Should the game manager be spawned?
	/// Maybe you don't want it to be if you're in a debug scene.
	/// </summary>
	[Property]
	[Title( "Spawn Manager" )]
	[Feature( SETTINGS ), Group( GAME ), Order( GAME_ORDER )]
	public bool SpawnGameManager { get; set; } = true;

	/// <summary>
	/// If defined then we'll try to spawn the
	/// game manager using this prefab instead.
	/// <br /> <br />
	/// <b> NOTE: </b> <see cref="Essential.GameManagerPrefab"/>
	/// is normally used as the prefab instead.
	/// </summary>
	[Property]
	[Title( "Manager Override" )]
	[Feature( SETTINGS ), Group( GAME ), Order( GAME_ORDER )]
	public PrefabFile GameManagerPrefabOverride { get; set; }

	/// <summary>
	/// If defined then <see cref="GameManager"/>
	/// is forced to auto-select this state instead.
	/// <br /> <br />
	/// <b> NOTE: </b> This is just for overriding
	/// based on the scene. You should set defaults
	/// in the game manager component itself.
	/// </summary>
	[Property]
	[Title( "State Override" )]
	[Feature( SETTINGS ), Group( GAME ), Order( GAME_ORDER )]
	public PrefabFile GameManagerStateOverride { get; set; }

	/// <summary>
	/// Does <see cref="SceneSettings"/> exist with <see cref="IsMainMenu"/> enabled?
	/// </summary>
	public static bool InMainMenu => Instance?.IsMainMenu is true;

	/// <summary>
	/// If enabled: out of bounds objects are teleported/respawned/destroyed.
	/// </summary>
	[Property]
	[Feature( BOUNDS ), Order( BOUNDS_ORDER )]
	[ToggleGroup( nameof( EnableBoundaries ), Label = $"{BOUNDARIES}" )]
	public virtual bool EnableBoundaries { get; set; } = true;

	/// <summary>
	/// If enabled: out of bounds objects are teleported/respawned.
	/// </summary>
	[Property]
	[Title( "Depth" )]
	[Feature( BOUNDS ), Order( BOUNDS_ORDER )]
	[ToggleGroup( nameof( EnableBoundaries ) )]
	public virtual bool EnableDepth { get; set; } = true;

	/// <summary>
	/// The depth(<c>z</c> level) where physics entities are considered out of bounds.
	/// </summary>
	[Property]
	[Title( "Level" )]
	[Range( -99999f, 0f, clamped: false )]
	[ShowIf( nameof( EnableDepth ), true )]
	[Feature( BOUNDS ), Group( DEPTH ), Order( DEPTH_ORDER )]
	public virtual float DepthLevel { get; set; } = -9001f;

	[Title( "On Fall" )]
	[ShowIf( nameof( EnableDepth ), true )]
	[Feature( BOUNDS ), Group( DEPTH ), Order( DEPTH_ORDER )]
	[Property, WideMode( HasLabel = false ), InlineEditor]
	public virtual BoundarySettings DepthSettings { get; set; }

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		if ( IsMainMenu )
			return;

		DrawTeleportationGizmos();
	}

	protected void DrawTeleportationGizmos()
	{
		if ( !EnableBoundaries )
			return;

		if ( EnableDepth )
			DrawDepthLevelGizmo( DepthLevel );
	}

	protected void DrawDepthLevelGizmo( in float z )
	{
		// Draw where the maximum depth is.
		using ( Gizmo.Scope() )
		{
			Gizmo.Draw.IgnoreDepth = false;
			Gizmo.Draw.LineThickness = 0.5f;
			Gizmo.Draw.Color = Color.Black.WithAlpha( 0.2f );

			// Plane offset is reversed for some reason..
			Gizmo.Transform = new( Vector3.Up * z, Rotation.Identity, 70f );
			Gizmo.Draw?.Plane( default, Vector3.Up );
		}
	}
}
