using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// 🔫 Shoots bullets.
/// </summary>
[Icon( "clear_all" )]
[Title( "Bullet Shooter" )]
public partial class BulletEquipFunction : EquipFunction
{
	protected const int BULLET_ORDER = MODULE_ORDER - 1000;

	protected const int BULLET_DEBUG_ORDER = BULLET_ORDER - 10;

	protected const int BULLET_SOUNDS_ORDER = BULLET_ORDER + 10;
	protected const int BULLET_SPREAD_ORDER = BULLET_ORDER + 20;
	protected const int BULLET_VIEW_ORDER = BULLET_ORDER + 30;
	protected const int BULLET_PREFABS_ORDER = BULLET_ORDER + 40;
	protected const int BULLET_EFFECTS_ORDER = BULLET_ORDER + 50;
	protected const int BULLET_TRACING_ORDER = BULLET_ORDER + 60;
	protected const int BULLET_HEALTH_ORDER = BULLET_ORDER + 70;

	[Property]
	[Title( "Render Traces" )]
	[Order( BULLET_DEBUG_ORDER )]
	[Feature( BULLET ), Group( DEBUG )]
	public bool DebugRenderTraces { get; set; } = false;

	/// <summary>
	/// The current angle(in degrees) of total, active bullet spread.
	/// </summary>
	[Property]
	[Title( "Current" )]
	[ReadOnly, JsonIgnore]
	[Order( BULLET_DEBUG_ORDER )]
	[ShowIf( nameof( InGame ), true )]
	[Feature( BULLET ), Group( DEBUG )]
	protected virtual Vector3 InspectorSpread => GetSpread();

	[Property]
	[Title( "Fire" )]
	[Order( BULLET_SOUNDS_ORDER )]
	[Feature( BULLET ), Group( SOUNDS )]
	public virtual SoundEvent FireSound { get; set; }

	[Property]
	[Title( "Empty" )]
	[Order( BULLET_SOUNDS_ORDER )]
	[Feature( BULLET ), Group( SOUNDS )]
	public virtual SoundEvent EmptySound { get; set; }

	/*
	[Property]
	[Title( "Tracer" )]
	[Order( BULLET_PREFABS_ORDER )]
	[Feature( BULLET ), Group( PREFABS )]
	public virtual GameObject TracerPrefab { get; set; }
	*/

	/// <summary>
	/// The maximum distance the bullet trace will travel.
	/// </summary>
	[Property]
	[Title( "Distance" )]
	[Order( BULLET_TRACING_ORDER )]
	[Feature( BULLET ), Group( TRACING )]
	public virtual float TraceDistance { get; set; } = 4096f;

	[Property]
	[Title( "Tracer Width" )]
	[Order( BULLET_EFFECTS_ORDER )]
	[Feature( BULLET ), Group( EFFECTS )]
	public virtual float TracerWidth { get; set; } = 0.5f;

	[Property]
	[Title( "Tracer Width" )]
	[Order( BULLET_EFFECTS_ORDER )]
	[Feature( BULLET ), Group( EFFECTS )]
	public virtual float TracerDuration { get; set; } = 0.25f;

	[Property]
	[Title( "Tracer Width" )]
	[Order( BULLET_EFFECTS_ORDER )]
	[Feature( BULLET ), Group( EFFECTS )]
	public virtual Color TracerColor { get; set; } = Color.White.WithAlpha( 0.12f );

	/// <summary>
	/// The base damage of the weapon.
	/// </summary>
	[Property]
	[Title( "Damage" )]
	[InlineEditor, WideMode]
	[Order( BULLET_HEALTH_ORDER )]
	[Feature( BULLET ), Group( HEALTH )]
	public DamageSettings DamageSettings { get; protected set; } = new()
	{
		BaseDamage = 25f,

		Impulse = 1f,
		ScaleForces = true,
		EnableForces = true,

		EnableHitboxes = true,
		HitboxMultipliers = { ["head"] = 2f },
	};

	/// <summary>
	/// If it's a cone or box or whatever.
	/// </summary>
	[Property]
	[Title( "Shape" )]
	[Order( BULLET_SPREAD_ORDER )]
	[Feature( BULLET ), Group( SPREAD )]
	public virtual SpreadShape SpreadShape { get; set; }

	/// <summary>
	/// The base angle(in degrees) of bullet cone spread.
	/// </summary>
	[Property]
	[Title( "Base" )]
	[Order( BULLET_SPREAD_ORDER )]
	[Range( 0f, 90f, clamped: false )]
	[Feature( BULLET ), Group( SPREAD )]
	public virtual float SpreadCone { get; set; } = 1f;

	/// <summary>
	/// The default recoil to add when firing.
	/// </summary>
	[Property]
	[Title( "Recoil (base)" )]
	[Order( BULLET_VIEW_ORDER )]
	[Feature( BULLET ), Group( VIEW )]
	public virtual Rotation RecoilBase { get; set; } = Rotation.FromPitch( -20f );

	/// <summary>
	/// The current angle(in degrees) of active bullet spread.
	/// </summary>
	protected virtual Vector2 GetSpread()
		=> Equip?.GetCurrentSpread( SpreadCone, this ) ?? SpreadCone;

	/// <returns> The start position of the bullet. </returns>
	public virtual Vector3 GetBulletOrigin()
		=> AimPosition;

	/// <returns> The direction of the bullet as a rotation. </returns>
	public virtual Rotation GetBulletDirection( in Vector2? spread = null )
	{
		var bulletSpread = spread ?? GetSpread();

		return bulletSpread != default
			? AimRotation * GetSpreadConeRotation( bulletSpread.Length )
			: AimRotation;
	}

	public SceneTrace GetBulletTrace( in Transform t )
		=> GetBulletTrace( t.Position, t.Rotation.Forward );

	public virtual SceneTrace GetBulletTrace( in Vector3 origin, in Vector3 dir )
	{
		if ( !Pawn.IsValid() )
			return default;

		return Pawn.GetEyeTrace( origin, origin + (dir * TraceDistance) );
	}

	protected virtual DamageData GetDamage( in SceneTraceResult tr )
		=> DamageData.FromBullet( DamageSettings, in tr, Equip );

	protected override void PlayActivationEffect( in Transform tOrigin )
	{
		if ( !FireSound.IsValid() )
			return;

		BroadcastSound( FireSound );
	}

	public virtual Transform GetTracerOrigin()
	{
		if ( !IsProxy )
		{
			var r = Pawn?.View?.ViewRenderer;

			if ( r.IsValid() )
				return r.WorldTransform;
		}

		return AimTransform;
	}

	[Rpc.Broadcast( NetFlags.SendImmediate | NetFlags.Reliable | NetFlags.OwnerOnly )]
	protected void RpcTracerEffect( Vector3 endPos )
		=> PlayTracerEffect( in endPos );

	protected virtual void PlayTracerEffect( in Vector3 endPos )
	{
		var tFrom = GetTracerOrigin();

		/*
		if ( !TracerPrefab.IsValid() )
			return;

		var objTracer = TracerPrefab.Clone( tFrom );

		if ( !objTracer.IsValid() )
			return;

		if ( !objTracer.Components.TryGet<LineRenderer>( out var lr ) )
		{
			objTracer.DestroyImmediate();
			return;
		}
		*/

		var obj = Scene?.CreateObject( enabled: true );

		if ( !obj.IsValid() )
			return;

		obj.WorldTransform = tFrom;
		obj.Name = "Bullet Tracer";

		var lr = obj.Components.Create<LineRenderer>();

		if ( !lr.IsValid() )
			obj.DestroyImmediate();

		lr.UseVectorPoints = true;
		lr.VectorPoints = [tFrom.Position, endPos];

		lr.Opaque = false;

		lr.Width = TracerWidth;
		lr.Color = TracerColor;

		// HACK: Add LineTracer component later.
		lr.Invoke( TracerDuration, lr.DestroyGameObject );
	}

	protected override void Activate()
	{
		var tAim = AimTransform;

		PlayActivationEffect( in tAim );

		var tr = GetBulletTrace( in tAim ).Run();

		if ( DebugRenderTraces )
			DebugOverlay.Trace( tr, duration: 1f );

		RpcTracerEffect( tr.EndPosition );
		AddRecoil( GetRecoil() );

		// TODO: Tracer effects.
		if ( !tr.Hit || !tr.GameObject.IsValid() )
			return;

		tr.GameObject.TryDamage( GetDamage( in tr ) );
	}

	protected override void ActivateEmpty()
	{
		base.ActivateEmpty();

		if ( !EmptySound.IsValid() )
			return;

		BroadcastSound( EmptySound, AimPosition );
	}

	protected virtual Rotation GetRecoil()
		=> RecoilBase;
}
