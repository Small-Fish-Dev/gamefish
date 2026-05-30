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

	protected const int PICKUP_EFFECT_ORDER = PICKUP_ORDER + 10;
	protected const int PICKUP_TRANSFORM_ORDER = PICKUP_ORDER + 20;
	protected const int PICKUP_SOUNDS_ORDER = PICKUP_ORDER + 30;

	protected const int PICKUP_USE_ORDER = PICKUP_ORDER + 50;

	/// <summary>
	/// If enabled: touching this will pick it up.
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
	/// If enabled: force rotation and scale.
	/// </summary>
	[Property]
	[Feature( PICKUP )]
	[Order( PICKUP_TRANSFORM_ORDER )]
	[ToggleGroup( nameof( TransformEnabled ), Label = TRANSFORM )]
	public bool TransformEnabled { get; set; } = true;

	/// <summary>
	/// The rotation to set upon start.
	/// </summary>
	[Property]
	[Title( "Rotation" )]
	[Feature( PICKUP )]
	[Order( PICKUP_TRANSFORM_ORDER )]
	[ToggleGroup( nameof( TransformEnabled ) )]
	protected Angles PickupRotation { get; set; } = Rotation.Identity;

	/// <summary>
	/// The scale to set upon start.
	/// </summary>
	[Property]
	[Title( "Scale" )]
	[Feature( PICKUP )]
	[Order( PICKUP_TRANSFORM_ORDER )]
	[ToggleGroup( nameof( TransformEnabled ) )]
	protected Vector3 PickupScale { get; set; } = 1f;

	/// <summary>
	/// Play this sound when a player activates this.
	/// </summary>
	[Property]
	[Title( "On Pickup" )]
	[Order( PICKUP_SOUNDS_ORDER )]
	[Feature( PICKUP ), Group( SOUNDS )]
	public SoundEvent PickupSound { get; set; }

	/// <summary>
	/// The thing what which lets us press a button to pick this up.
	/// </summary>
	[Property]
	[Title( "Module" )]
	[Order( PICKUP_USE_ORDER + 1 )]
	[Feature( PICKUP ), Group( USE )]
	protected virtual PickupUsable UsableModule { get; set; }

	public virtual bool HasUsableModule => UsableModule.IsValid();

	/// <summary>
	/// Assigns a new or existing module that lets you press a button to pick this up.
	/// </summary>
	[Button( "Add" )]
	[Order( PICKUP_USE_ORDER + 1 )]
	[Feature( PICKUP ), Group( USE )]
	[HideIf( nameof( HasUsableModule ), true )]
	protected void AddUsableButton()
		=> EnsureUsableModule();

	protected virtual void EnsureUsableModule()
	{
		if ( UsableModule.IsValid() )
			return;

		UsableModule = Components?.GetOrCreate<PickupUsable>();
		UsableModule?.Enabled = true;
	}

	protected override void OnEnabled()
	{
		base.OnEnabled();

		Tags?.Add( TAG_ITEM );
		Tags?.Add( TAG_PICKUP );
	}

	protected override void OnStart()
	{
		base.OnStart();

		OnTransformStart();
	}

	protected virtual void OnTransformStart()
	{
		if ( !TransformEnabled )
			return;

		var tWorld = WorldTransform;

		tWorld.Rotation = PickupRotation;
		tWorld.Scale = WorldScale = PickupScale;

		WorldTransform = tWorld;
	}

	public virtual void OnTriggerEnter( GameObject other )
	{
		if ( !Networking.IsHost )
			return;

		if ( !GameObject.IsValid() || GameObject.IsDestroyed )
			return;

		if ( !AutoPickup )
			return;

		if ( TryGet<Player>( other, out var pl ) )
			TryPickup( pl );
	}

	public bool TryPickup( Player pl )
	{
		if ( !Networking.IsHost )
			return false;

		if ( !Scene.InGame() )
			return false;

		if ( !GameObject.IsValid() || GameObject.IsDestroyed )
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

			if ( GameObject.IsValid() )
				return false;
		}

		return true;
	}

	public virtual bool CanPickup( Player pl )
		=> pl.IsValid() && pl.IsAlive;

	protected virtual void PlayPickupEffect( Player pl )
	{
		if ( pl.IsValid() && PickupSound.IsValid() )
			pl.HostBroadcastSound( PickupSound );
	}

	protected abstract void OnPickup( Player pl );
}
