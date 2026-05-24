namespace GameFish;

/// <summary>
/// Spawns a prefab at this position.
/// <br /> <br />
/// <b> NOTE: </b> You can also specify an object in the scene.
/// <br /> <br />
/// <b> TIP: </b> Put the object you want to clone underneath this
/// spawner and disable it. The copy will be enabled upon cloning.
/// </summary>
[Icon( "add_circle" )]
[EditorHandle( Icon = "🔵" )]
public class PrefabSpawner : ModuleEntity, IActivate
{
	protected const int SPAWNER_ORDER = DEFAULT_ORDER - 1000;

	protected const int SPAWNER_ROTATION_ORDER = SPAWNER_ORDER + 10;
	protected const int SPAWNER_SCALING_ORDER = SPAWNER_ORDER + 20;

	protected const int SPAWNER_NETWORKING_ORDER = SPAWNER_ORDER + 50;

	/// <summary>
	/// If enabled: spawn on start.
	/// </summary>
	[Property]
	[Feature( SPAWNER )]
	[Title( "Auto-Spawn" )]
	[Order( SPAWNER_ORDER )]
	public bool AutoSpawning { get; set; } = true;

	/// <summary>
	/// The prefab to spawn(or object to clone).
	/// <br /> <br />
	/// <b> TIP: </b> Put the object you want to clone underneath this
	/// spawner and disable it. The copy will be enabled upon cloning.
	/// </summary>
	[Property]
	[Feature( SPAWNER )]
	[Order( SPAWNER_ORDER )]
	public GameObject Prefab { get; set; }

	/// <summary>
	/// If defined: sets the parent when spawning. Defaults to the scene.
	/// </summary>
	[Property]
	[Title( "Parent" )]
	[Feature( SPAWNER )]
	[Order( SPAWNER_ORDER )]
	public GameObject PrefabParent { get; set; }

	/// <summary>
	/// If enabled: override rotation, otherwise inherit it from this object.
	/// </summary>
	[Property]
	[Feature( SPAWNER )]
	[Order( SPAWNER_ROTATION_ORDER )]
	[ToggleGroup( nameof( RotationEnabled ), Label = ROTATION )]
	public bool RotationEnabled { get; set; } = false;

	[Property]
	[Feature( SPAWNER )]
	[Title( "Rotation" )]
	[Order( SPAWNER_ROTATION_ORDER )]
	[ToggleGroup( nameof( RotationEnabled ) )]
	public Rotation PrefabRotation { get; set; } = Rotation.Identity;

	/// <summary>
	/// If enabled: override scale, otherwise inherit it from this object.
	/// </summary>
	[Property]
	[Feature( SPAWNER )]
	[Order( SPAWNER_SCALING_ORDER )]
	[ToggleGroup( nameof( ScalingEnabled ), Label = SCALING )]
	public bool ScalingEnabled { get; set; } = false;

	[Property]
	[Feature( SPAWNER )]
	[Title( "Scaling" )]
	[Order( SPAWNER_SCALING_ORDER )]
	[ToggleGroup( nameof( ScalingEnabled ) )]
	public Vector3 PrefabScaling { get; set; } = Vector3.One;

	/// <summary>
	/// If enabled: immediately network the spawned object.
	/// </summary>
	[Property]
	[Feature( SPAWNER )]
	[Order( SPAWNER_NETWORKING_ORDER )]
	[ToggleGroup( nameof( PrefabNetworkingEnabled ), Label = NETWORKING )]
	public bool PrefabNetworkingEnabled { get; set; } = true;

	[Property]
	[Feature( SPAWNER )]
	[Title( "Network Mode" )]
	[Order( SPAWNER_NETWORKING_ORDER )]
	[ToggleGroup( nameof( PrefabNetworkingEnabled ) )]
	public NetworkMode PrefabNetworkMode { get; set; } = NetworkMode.Object;

	[Property]
	[Feature( SPAWNER )]
	[Title( "Owner Transfer" )]
	[Order( SPAWNER_NETWORKING_ORDER )]
	[ToggleGroup( nameof( PrefabNetworkingEnabled ) )]
	public OwnerTransfer PrefabOwnerTransfer { get; set; } = OwnerTransfer.Fixed;

	[Property]
	[Feature( SPAWNER )]
	[Title( "Orphan Mode" )]
	[Order( SPAWNER_NETWORKING_ORDER )]
	[ToggleGroup( nameof( PrefabNetworkingEnabled ) )]
	public NetworkOrphaned PrefabOrphanMode { get; set; } = NetworkOrphaned.ClearOwner;

	protected override void OnStart()
	{
		base.OnStart();

		if ( AutoSpawning )
			AutoSpawn();
	}

	protected virtual void AutoSpawn()
	{
		if ( !Networking.IsHost )
			return;

		TrySpawn( out _ );
	}

	public virtual bool TrySpawn( out GameObject obj )
	{
		obj = null;

		if ( !Prefab.IsValid() )
			return false;

		var tSpawner = WorldTransform;
		var tWorld = Prefab.WorldTransform;

		// Position
		tWorld.Position = tSpawner.Position;

		// Rotation
		if ( RotationEnabled && ITransform.IsValid( PrefabRotation ) )
			tWorld.Rotation = PrefabRotation;
		else
			tWorld.Rotation = WorldRotation;

		// Scale
		if ( ScalingEnabled && ITransform.IsValid( PrefabScaling ) )
			tWorld.Scale = PrefabScaling;
		else
			tWorld.Scale = tSpawner.Scale;

		var parent = PrefabParent.AsValid();

		obj = Prefab.Clone( transform: tWorld, parent: parent, startEnabled: true );

		OnSpawned( obj );

		return obj.IsValid();
	}

	protected virtual void OnSpawned( GameObject obj )
	{
		if ( !obj.IsValid() )
			return;

		if ( PrefabNetworkingEnabled )
		{
			obj.NetworkSetup(
				cn: Connection.Local,
				netMode: PrefabNetworkMode,
				orphanMode: PrefabOrphanMode,
				ownerTransfer: PrefabOwnerTransfer
			);
		}
	}

	public virtual bool TryActivate( object source = null, object value = null )
	{
		if ( !GameObject.IsValid() || !Active )
			return false;

		RpcActivate();
		return true;
	}

	[Rpc.Owner]
	protected void RpcActivate()
		=> OnActivate();

	protected virtual void OnActivate()
		=> TrySpawn( out _ );
}
