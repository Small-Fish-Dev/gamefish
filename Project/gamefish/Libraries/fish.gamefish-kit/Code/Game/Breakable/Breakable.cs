namespace GameFish;

/// <summary>
/// An entity with health and physics meant to be broken.
/// <br /> <br />
/// <b> WARNING: </b> Don't place this on top of other dynamic entities
/// such as projectiles, pawns etc. as it is an entity itself.
/// <br /> <br />
/// <b> TIP: </b> If you're trying to add these features to an existing
/// entity you should look into <see cref="DamageModule"/>s.
/// </summary>
[Icon( "broken_image" )]
[EditorHandle( Icon = "⚱" )]
public partial class Breakable : DynamicEntity
{
	protected const int BREAKABLE_ORDER = HEALTH_ORDER - 200;

	/// <summary>
	/// Should the object be destroyed when this is broken?
	/// </summary>
	[Property]
	[Feature( BREAKABLE ), Group( EFFECTS ), Order( BREAKABLE_ORDER )]
	public bool DestroyObject { get; set; } = true;

	/// <summary>
	/// Should effects be centered on the object?
	/// </summary>
	[Property]
	[Title( "Centering" )]
	[Feature( BREAKABLE ), Group( EFFECTS ), Order( BREAKABLE_ORDER )]
	public bool CenterEffects { get; set; } = true;

	/// <summary>
	/// Should the effect prefab inherit this object's scale?
	/// </summary>
	[Property]
	[Feature( BREAKABLE ), Group( EFFECTS ), Order( BREAKABLE_ORDER )]
	public bool InheritScale { get; set; } = true;

	/// <summary>
	/// Applies this transform to the break prefab/sound.
	/// </summary>
	[Property]
	[Title( "Offset" )]
	[Feature( BREAKABLE ), Group( EFFECTS ), Order( BREAKABLE_ORDER )]
	public Transform EffectsOffset { get; set; } = global::Transform.Zero;

	/// <summary>
	/// The prefab to spawn upon breaking.
	/// <br /> <br />
	/// <b> NOTE: </b> Could be a particle effect or some matryoshka bullshit.
	/// You probably want to make sure it cleans itself up.
	/// </summary>
	[Property]
	[Title( "Prefab" )]
	[Feature( BREAKABLE ), Group( EFFECTS ), Order( BREAKABLE_ORDER )]
	public PrefabFile BreakPrefab { get; set; }

	/// <summary>
	/// The sound to play(if any) upon breaking.
	/// </summary>
	[Property]
	[Title( "Break Sound" )]
	[Feature( BREAKABLE ), Group( SOUND ), Order( BREAKABLE_ORDER )]
	public SoundEvent BreakSound { get; set; }

	/// <summary>
	/// The sound to play(if any) upon taking damage.
	/// </summary>
	[Property]
	[Title( "Damaged Sound" )]
	[Feature( BREAKABLE ), Group( SOUND ), Order( BREAKABLE_ORDER )]
	public SoundEvent DamagedSound { get; set; }

	protected override void OnDamaged( in DamageData data )
	{
		base.OnDamaged( data );

		if ( IsProxy || data.Damage <= 0f )
			return;

		if ( !DamagedSound.IsValid() )
			return;

		if ( data.HitPosition is Vector3 hitPos )
			BroadcastSound( DamagedSound, hitPos );
		else
			BroadcastSound( DamagedSound, Center );
	}

	public override void OnDeath()
	{
		if ( GameObject.IsValid() )
			OnBreak();

		base.OnDeath();

		if ( DestroyObject )
			GameObject?.Destroy();
	}

	protected virtual void OnBreak()
	{
		if ( IsProxy )
			return;

		var tWorld = new Transform( WorldPosition, WorldRotation );

		if ( CenterEffects )
			tWorld.Position = GameObject.GetBounds().Center;

		if ( InheritScale )
			tWorld.Scale = WorldScale;

		if ( EffectsOffset != default )
			tWorld = tWorld.ToWorld( EffectsOffset );

		// To play a sound.
		RpcBreakSound( BreakSound, tWorld.Position );

		// Particles or something weird.
		if ( BreakPrefab.TrySpawn( tWorld, out var objEffect ) )
		{
			objEffect.NetworkSetup(
				cn: Connection.Host,
				orphanMode: NetworkOrphaned.Destroy,
				ownerTransfer: OwnerTransfer.Fixed,
				netMode: NetworkMode.Object,
				ignoreProxy: false
			);
		}
	}


	[Rpc.Broadcast( NetFlags.OwnerOnly | NetFlags.SendImmediate | NetFlags.Reliable )]
	protected static void RpcBreakSound( SoundEvent snd, Vector3 pos )
	{
		Sound.Play( snd, pos );
	}
}
