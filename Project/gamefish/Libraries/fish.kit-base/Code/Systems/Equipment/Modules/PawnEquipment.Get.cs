using System;

namespace GameFish;

partial class PawnEquipment
{
	protected void RefreshList()
	{
		if ( !Networking.IsHost || Equipped is null )
			return;

		// Add any missing equips.
		var toAdd = new List<BaseEquip>();

		foreach ( var e in Components.GetAll<BaseEquip>( FindMode.EverythingInSelfAndDescendants ) )
			if ( e.IsValid() && e.EquipState is not EquipState.Dropped )
				if ( !Equipped.Contains( e ) )
					toAdd.Add( e );

		toAdd.ForEach( Equipped.Add );

		// Remove invalid equips.
		var toRemove = new List<BaseEquip>();

		foreach ( var e in Equipped )
			if ( !e.IsValid() || e.EquipState is EquipState.Dropped )
				toRemove.Add( e );

		var parent = ModuleParent;

		toRemove.ForEach( e => { Equipped.Remove( e ); e.Destroy(); } );
	}

	public virtual int? FirstFreeSlot()
	{
		if ( Equipped is null )
			return SlotCount >= 1 ? 1 : null;

		var slots = Equipped
			.Where( e => e.IsValid() )
			.Select( e => e.Slot )
			.Distinct();

		var counts = slots
			.ToDictionary( s => s, s => GetInSlot( s ).Count() );

		for ( var i = 1; i <= SlotCount; i++ )
			if ( !counts.TryGetValue( i, out var count ) || count < SlotCapacity )
				return i;

		return null;
	}

	public bool Any( string id )
		=> Get( id ).IsValid();

	/// <summary>
	/// Gets a held equip using its ID(if it's there).
	/// </summary>
	/// <returns> The first found equip(or null). </returns>
	public BaseEquip Get( string id )
	{
		if ( Equipped is null || id is null )
			return null;

		return Equipped.FirstOrDefault( e => e.IsValid() && e.ID == id );
	}

	/// <summary>
	/// Gets all held equipment with an ID(if any).
	/// </summary>
	/// <returns> All equipment(or empty, never null). </returns>
	public IEnumerable<BaseEquip> GetAll( string id )
	{
		if ( Equipped is null || id is null )
			return [];

		return Equipped.Where( e => e.IsValid() && e.ID == id );
	}

	public bool Any<T>( T e ) where T : BaseEquip
		=> e is not null && Any( e.GetType() );

	public bool Any( Type eType )
	{
		if ( Equipped is null )
			return false;

		return Equipped.Any( e => e.IsValid() && e.GetType() == eType );
	}

	/// <summary>
	/// Gets an equip of a specific type(if it's there).
	/// </summary>
	/// <returns> The first found <typeparamref name="T"/>(or null). </returns>
	public T Get<T>() where T : BaseEquip
	{
		if ( Equipped is null )
			return null;

		return Equipped.FirstOrDefault( e => e.IsValid() && e.GetType() == typeof( T ) ) as T;
	}

	/// <summary>
	/// Gets all equipment stuck in a slot.
	/// </summary>
	/// <returns> All equipment in that slot(or empty, never null). </returns>
	public bool AnyInSlot( int? slot )
	{
		if ( slot is null || Equipped is null )
			return false;

		return Equipped.Any( e => e.IsValid() && e.Slot == slot );
	}

	/// <summary>
	/// Gets all equipment stuck in a slot.
	/// </summary>
	/// <returns> All equipment in that slot(or empty, never null). </returns>
	public IEnumerable<BaseEquip> GetInSlot( int? slot )
	{
		if ( slot is null || Equipped is null )
			return [];

		return Equipped.Where( e => e.IsValid() && e.Slot == slot );
	}
}
