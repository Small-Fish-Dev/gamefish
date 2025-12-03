using System;
using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// A pawn module that manages and optionally spawns equipment.
/// </summary>
[Icon( "backpack" )]
public partial class PawnEquipment : Module
{
	public const string SLOTTING = "Slotting";
	public const string INVENTORY = "Inventory";

	public Pawn Pawn => Parent as Pawn;

	public override bool IsParent( ModuleEntity comp )
		=> comp.IsValid() && comp is Pawn;

	/// <summary> If true: only try to pick up weapons in their intended slot. </summary>
	[Title( "Strict" )]
	[Property, Feature( EQUIP ), Group( SLOTTING )]
	public virtual bool StrictSlots { get; set; }

	/// <summary> If true: prevent picking up multiple instances of a weapon. </summary>
	[Title( "Unique" )]
	[Property, Feature( EQUIP ), Group( SLOTTING )]
	public virtual bool UniqueEquips { get; set; } = true;

	/// <summary> How many weapons can fit in each individual slot? </summary>
	[Title( "Size" )]
	[Range( 1, 4, clamped: false ), Step( 1f )]
	[Property, Feature( EQUIP ), Group( SLOTTING )]
	public virtual int SlotCapacity { get; set; } = 1;

	/// <summary> How many equipment slots are available overall? </summary>
	[Title( "Available" )]
	[Range( 0, 10, clamped: false ), Step( 1f )]
	[Property, Feature( EQUIP ), Group( SLOTTING )]
	public virtual int SlotCount { get; set; } = 10;

	/// <summary> The part of each slot select action's name that comes before its number. </summary>
	[Title( "Input Prefix" )]
	[Range( 0, 10, clamped: false ), Step( 1f )]
	[Property, Feature( EQUIP ), Group( SLOTTING )]
	public virtual string InputPrefix { get; set; } = "Slot";

	/// <summary>
	/// Print debug logs to console?
	/// </summary>
	[Property]
	[Title( "Logging" )]
	[Order( DEBUG_ORDER )]
	[Feature( EQUIP ), Group( DEBUG )]
	public virtual bool DebugLogging { get; set; } = false;

	/// <summary>
	/// Automatically give the loadout when this module first starts? <br />
	/// If not then you'll need to call <see cref="GiveLoadout"/> yourself.
	/// </summary>
	[Property, Feature( EQUIP ), Group( INVENTORY )]
	public virtual bool AutoGiveLoadout { get; set; } = true;

	/// <summary> The weapons to spawn. </summary>
	[WideMode]
	[Property, Feature( EQUIP ), Group( INVENTORY )]
	public virtual List<EquipLoadoutEntry> Loadout { get; set; } = [];

	/// <summary>
	/// The main active equipment.
	/// </summary>
	[Title( "Active Equip" )]
	[InlineEditor, ReadOnly, JsonIgnore]
	[Property, Feature( EQUIP ), Group( INVENTORY )]
	protected Equipment InspectorActiveEquip => ActiveEquip;

	/// <summary>
	/// All currently equipped items according to the host.
	/// </summary>
	[Title( "Equipped" )]
	[InlineEditor, ReadOnly, JsonIgnore]
	[Property, Feature( EQUIP ), Group( INVENTORY )]
	protected NetList<Equipment> InspectorEquipped => Equipped;

	/// <summary>
	/// The main active equipment.
	/// </summary>
	[Sync]
	public Equipment ActiveEquip
	{
		get => _activeEquip.IsValid() && _activeEquip.Active
			? _activeEquip : null;

		protected set => _activeEquip = value;
	}

	protected Equipment _activeEquip;

	/// <summary>
	/// All currently equipped items according to the host.
	/// </summary>
	[Sync( SyncFlags.FromHost )]
	public NetList<Equipment> Equipped { get; set; } = [];

	protected override void OnRegistrationFailure( ModuleEntity parent )
	{
		base.OnRegistrationFailure( parent );

		if ( DebugLogging )
			this.Log( $"Failed to register to parent:[{parent}]" );
	}

	protected override void OnStart()
	{
		base.OnStart();

		if ( !Networking.IsHost )
			return;

		if ( AutoGiveLoadout && this.InGame() )
		{
			if ( DebugLogging )
				this.Log( $"Auto-giving loadout for pawn:[{Pawn}]" );

			GiveLoadout();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy )
			return;

		if ( !Pawn.IsValid() || !Pawn.IsAlive )
			return;

		SelectEquipUpdate();
	}

	protected virtual void SelectEquipUpdate()
	{
		if ( Equipped is null || Equipped.Count == 0 )
			return;

		// Scrolling
		var yScroll = Input.MouseWheel.y.Sign();

		if ( yScroll != 0f )
		{
			var slots = Equipped
				.Where( e => e.IsValid() )
				.Select( e => e.Slot )
				.Distinct()
				.Order()
				.ToList();

			var currentSlot = ActiveEquip.IsValid() ? ActiveEquip.Slot : 0;
			var slotIndex = slots.IndexOf( currentSlot );

			if ( slotIndex == -1 || !slots.Contains( currentSlot ) )
			{
				TryDeploy( Equipped.FirstOrDefault() ?? ActiveEquip );
				return;
			}

			var nextSlotIndex = (slotIndex + yScroll).UnsignedMod( slots.Count );
			var nextSlot = slots.ElementAtOrDefault( nextSlotIndex );
			var selectEquip = Equipped.FirstOrDefault( e => e.IsValid() && e.Slot == nextSlot );

			if ( selectEquip.IsValid() )
			{
				TryDeploy( selectEquip );
				return;
			}
		}

		// Slot Pressing
		foreach ( var equip in Equipped )
		{
			if ( !equip.IsValid() )
				continue;

			if ( Input.Pressed( InputPrefix + equip.Slot ) )
				if ( TryDeploy( equip ) )
					return;
		}
	}

	public virtual void GiveLoadout()
	{
		if ( !Networking.IsHost || Loadout is null )
			return;

		foreach ( var entry in Loadout )
		{
			// this.Log( entry );
			int? slot = entry.Slot?.AsInt();

			if ( slot.HasValue && slot.Value <= 0 )
				slot = null;

			if ( !TryEquip( entry.Prefab, out _, slot ) )
				this.Warn( $"Failed to equip prefab:[{entry.Prefab}] in slot:[{slot}]" );
		}
	}

	public virtual Equipment SelectFirstEquip()
	{
		if ( Equipped is null )
			return null;

		return Equipped.OrderBy( e => e?.Slot ?? 0 )
			.FirstOrDefault( e => e.IsValid() );
	}

	public virtual bool TryHolster( Equipment to = null )
	{
		if ( to.IsValid() && !to.CanDeploy() )
			return false;

		if ( !ActiveEquip.IsValid() )
			return true;

		if ( !ActiveEquip.TryHolster( to ) )
			return false;

		ActiveEquip = null;
		return false;
	}

	public virtual bool TryDeploy( Equipment equip )
	{
		if ( IsProxy )
			return false;

		if ( !equip.IsValid() || !equip.CanDeploy( from: ActiveEquip ) )
			return false;

		if ( ActiveEquip.IsValid() )
		{
			// Prevent switching to the same equip.
			if ( ActiveEquip == equip )
				return true;

			// Holster previous equipment first.
			if ( !TryHolster( to: equip ) )
				return false;
		}

		// Let equipment decide/manage deployement.
		if ( equip.TryDeploy( from: ActiveEquip ) )
		{
			ActiveEquip = equip;

			if ( DebugLogging )
				this.Log( $"Deployed equip:[{equip}]" );

			return true;
		}

		// Couldn't deploy for some reason.
		if ( DebugLogging )
			this.Log( $"Failed to deploy from:[{ActiveEquip}] to:[{equip}]" );

		return false;
	}

	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostDeploy( Equipment equip )
	{
		if ( GameObject.IsValid() )
			TryDeploy( equip: equip );
	}

	public virtual bool TryRemove( Equipment e )
	{
		if ( !e.IsValid() || !e.GameObject.IsValid() )
		{
			// Check if it was destroyed but still referenced.
			if ( e is not null && Equipped is not null )
				if ( Equipped.Contains( e ) )
					RefreshList();

			return true;
		}

		e.GameObject.Destroy();

		RefreshList();

		return true;
	}

	public virtual bool TryDrop( Equipment e )
	{
		throw new NotImplementedException();
	}

	public virtual bool TryEquip( string classId, out Equipment e, int? slot = null )
	{
		if ( !TryGetEntityPrefab( classId, out var ePrefab ) )
		{
			e = null;
			return false;
		}

		return TryEquip( ePrefab, out e, slot );
	}

	public virtual bool TryEquip( PrefabFile p, out Equipment e, int? slot = null )
	{
		e = null;

		if ( !Networking.IsHost )
			return false;

		if ( !p.TrySpawn( WorldPosition, out var go ) )
		{
			this.Warn( $"Tried to equip invalid/missing equipment prefab:[{p}]" );
			return false;
		}

		e = go.Components.Get<Equipment>( FindMode.EverythingInSelf );

		if ( TryEquip( e, slot ) )
			return true;

		go.Destroy();
		return false;
	}

	public virtual bool TryEquip( Equipment e, int? slot = null )
	{
		if ( !Networking.IsHost )
			return false;

		var pawn = Pawn;

		if ( !pawn.IsValid() )
		{
			this.Warn( $"Tried to equip:[{e}] onto an invalid parent:[{pawn}]" );
			return false;
		}

		if ( !e.IsValid() )
		{
			this.Warn( $"Tried to equip invalid/missing equipment on parent:[{pawn}]" );
			return false;
		}

		// Allow restricting one weapon at a time.
		if ( UniqueEquips && Any( e.ClassId ) )
			return false;

		// The "None" EquipSlot is zero. Nullify for simplicity.
		if ( slot.HasValue && slot.Value <= 0 )
			slot = null;

		// Allow restricting to a specific slot.
		if ( StrictSlots )
		{
			var defaultSlot = e.DefaultSlot.AsInt();
			slot ??= defaultSlot;

			if ( slot != defaultSlot )
				return false;
		}
		else
		{
			slot ??= FirstFreeSlot();

			if ( !slot.HasValue )
				return false;
		}

		var inSlot = GetAllInSlot( slot );

		if ( inSlot.Count() > SlotCapacity )
		{
			if ( DebugLogging )
				this.Warn( $"Tried to equip:[{e}] in full slot:[{slot}] on parent:[{pawn}]" );

			return false;
		}

		// Equip might have something to say about this.
		if ( !e.AllowEquip( pawn ) )
			return false;

		// Assign the slot so it can be swapped between.
		e.Slot = slot.Value;

		// Network spawn it before making it a child!
		e.TrySetNetworkOwner( pawn.Network?.Owner );

		// Setting this should parent it and shit.
		e.SetOwner( pawn );

		// So that clients know what's up.
		RefreshList();

		return true;
	}
}
