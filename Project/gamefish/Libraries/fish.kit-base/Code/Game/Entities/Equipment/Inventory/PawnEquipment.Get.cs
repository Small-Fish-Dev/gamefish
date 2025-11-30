using System;

namespace GameFish;

partial class PawnEquipment
{
	public void RefreshList()
	{
		Equipped ??= [];

		if ( !Networking.IsHost || Equipped is null )
			return;

		if ( Pawn is not BasePawn pawn || !pawn.IsValid() )
			return;

		// Add any missing equips.
		var toAdd = new List<Equipment>();

		foreach ( var e in pawn.Components.GetAll<Equipment>( FindMode.EverythingInSelfAndDescendants ) )
			if ( e.IsValid() && !Equipped.Contains( e ) )
				toAdd.Add( e );

		toAdd.ForEach( Equipped.Add );

		// Remove invalid equips.
		var toRemove = new List<Equipment>();

		foreach ( var e in Equipped )
			if ( !e.IsValid() )
				toRemove.Add( e );

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
			.ToDictionary( s => s, s => GetAllInSlot( s ).Count() );

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
	public Equipment Get( string id )
	{
		if ( Equipped is null || id is null )
			return null;

		return Equipped.FirstOrDefault( e => e.IsValid() && e.ClassId == id );
	}

	/// <summary>
	/// Gets all valid held equipment.
	/// </summary>
	/// <returns> All valid equipment(or empty, never null). </returns>
	public IEnumerable<Equipment> GetAll()
		=> Equipped?.Where( e => e.IsValid() ) ?? [];

	/// <summary>
	/// Gets all held equipment with an ID(if any).
	/// </summary>
	/// <returns> All equipment(or empty, never null) with that ID. </returns>
	public IEnumerable<Equipment> GetAll( string id )
	{
		if ( Equipped is null || id is null )
			return [];

		return Equipped.Where( e => e.IsValid() && e.ClassId == id );
	}

	public bool Any<T>( T e ) where T : Equipment
		=> e is not null && Any<T>();

	public bool Any<T>()
		=> Equipped?.Any( e => e.IsValid() && e is T ) is true;

	/// <summary>
	/// Gets the first instance(if any) of an equip of a specific type.
	/// </summary>
	/// <returns> The first found <typeparamref name="T"/>(or null). </returns>
	public T Get<T>() where T : Equipment
	{
		if ( Equipped is null )
			return null;

		return Equipped.FirstOrDefault( e => e.IsValid() && e is T ) as T;
	}

	/// <summary>
	/// Gets all held equipment of the provided type.
	/// </summary>
	/// <returns> All equipment(or empty, never null) with that type. </returns>
	public IEnumerable<T> GetAll<T>() where T : Equipment
	{
		if ( Equipped is null )
			return [];

		return Equipped
			.Where( e => e.IsValid() && e is T )
			.Select( e => e as T );
	}

	/// <summary>
	/// Tries to get the first instance of an equip of a specific type.
	/// </summary>
	/// <returns> If a <typeparamref name="T"/> was found. </returns>
	public bool TryGet<T>( out T equip ) where T : Equipment
		=> (equip = Get<T>()).IsValid();

	/// <returns> If there is any equipment in a specific slot. </returns>
	public bool AnyInSlot( int? slot )
	{
		if ( slot is null || Equipped is null )
			return false;

		return Equipped.Any( e => e.IsValid() && e.Slot == slot );
	}

	/// <summary>
	/// Gets all equipment assigned to a specific slot.
	/// </summary>
	/// <returns> All equipment in that slot(or empty, never null). </returns>
	public IEnumerable<Equipment> GetAllInSlot( int? slot )
	{
		if ( slot is null || Equipped is null )
			return [];

		return Equipped.Where( e => e.IsValid() && e.Slot == slot );
	}

	/// <summary>
	/// Gets the first equipment of a specific slot.
	/// </summary>
	/// <returns> The first found equipment in that slot(or null). </returns>
	public Equipment GetFirstInSlot( int? slot )
	{
		if ( slot is null || Equipped is null )
			return null;

		return Equipped.FirstOrDefault( e => e.IsValid() && e.Slot == slot );
	}
}
