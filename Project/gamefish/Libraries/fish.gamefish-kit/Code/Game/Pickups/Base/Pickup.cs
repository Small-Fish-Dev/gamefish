using System;
using System.Text.Json.Serialization;
namespace GameFish;

/// <summary>
/// Players can use it for some effect.
/// </summary>
[Icon( "interests" )]
[EditorHandle( Icon = "🌭" )] // glizzy
public abstract partial class Pickup : DynamicEntity, Component.ITriggerListener
{
	protected const int PICKUP_ORDER = DEFAULT_ORDER - 1000;

	protected const int EFFECT_ORDER = PICKUP_ORDER + 10;
	protected const int TRANSFORM_ORDER = PICKUP_ORDER + 20;

	/// <summary>
	/// Should touching pick this up?
	/// </summary>
	[Property]
	[Title( "Auto-Pickup" )]
	[Sync( SyncFlags.FromHost )]
	[Feature( PICKUP ), Order( PICKUP_ORDER )]
	public bool AutoPickup { get; set; } = true;

	/// <summary>
	/// Self-destruct after activation?
	/// </summary>
	[Property]
	[Feature( PICKUP ), Order( PICKUP_ORDER )]
	public bool IsConsumable { get; set; } = true;

	/// <summary>
	/// Force <c>z</c>, rotation and scale?
	/// </summary>
	[Property]
	[Feature( PICKUP ), Order( TRANSFORM_ORDER )]
	[ToggleGroup( nameof( TransformEnabled ), Label = TRANSFORM )]
	public bool TransformEnabled { get; set; } = true;

	/// <summary>
	/// Go to this <c>z</c> upon start.
	/// </summary>
	[Property]
	[Title( "Vertical Level" )]
	[Range( 0f, 64f, clamped: false )]
	[Feature( PICKUP ), Order( TRANSFORM_ORDER )]
	[ToggleGroup( nameof( TransformEnabled ) )]
	public float VerticalLevel { get; set; } = 32f;

	/// <summary>
	/// Set this rotation upon start.
	/// </summary>
	[Property]
	[Title( "Angles" )]
	[Range( 0f, 64f, clamped: false )]
	[Feature( PICKUP ), Order( TRANSFORM_ORDER )]
	[ToggleGroup( nameof( TransformEnabled ) )]
	protected Angles PickupAngles { get; set; } = Angles.Zero;

	/// <summary>
	/// The scale to force upon start.
	/// </summary>
	[Property]
	[Title( "Scale" )]
	[Feature( PICKUP ), Order( TRANSFORM_ORDER )]
	[ToggleGroup( nameof( TransformEnabled ) )]
	protected Vector3 PickupScale { get; set; } = 1f;

	/// <summary>
	/// Play this sound when a player activates this.
	/// </summary>
	[Property]
	[Title( "On Pickup" )]
	[Feature( PICKUP ), Group( SOUNDS )]
	public SoundEvent PickupSound { get; set; }

	/// <summary>
	/// The thing what which lets us pick this up.
	/// </summary>
	[Title( "Usable" )]
	[Property, RequireComponent]
	[Feature( PICKUP ), Group( USE )]
	protected PickupUsable UsableModule { get; set; }

	protected override void OnEnabled()
	{
		base.OnEnabled();

		Tags?.Add( TAG_ITEM );
		Tags?.Add( TAG_PICKUP );
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( TransformEnabled )
		{
			var tWorld = WorldTransform;

			tWorld.Position.z = VerticalLevel;
			tWorld.Rotation = PickupAngles;
			tWorld.Scale = WorldScale = PickupScale;

			WorldTransform = tWorld;
		}
	}

	public void OnTriggerEnter( GameObject other )
	{
		if ( !Networking.IsHost )
			return;

		if ( !AutoPickup )
			return;

		if ( Pawn.TryGet<Player>( other, out var pl ) )
			TryPickup( pl );
	}

	public bool TryPickup( Player pl )
	{
		if ( !Networking.IsHost )
			return false;

		if ( !Scene.IsValid() || !GameObject.IsValid() )
			return false;

		if ( !pl.IsValid() || !CanPickup( pl ) )
			return false;

		try
		{
			OnPickup( pl );

			PlayPickupEffect( pl );

			if ( IsConsumable )
				GameObject.Destroy();
		}
		catch ( Exception e )
		{
			this.Warn( $"{nameof( TryPickup )} exception: {e}" );
		}

		return true;
	}

	public virtual bool CanPickup( Player pl )
		=> pl.IsValid() && pl.IsAlive;

	protected abstract void OnPickup( Player pl );

	protected virtual void PlayPickupEffect( Player pl )
	{
		if ( pl.IsValid() )
			pl.HostBroadcastSound( PickupSound );
	}
}
