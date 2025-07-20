namespace GameFish;

/// <summary>
/// An equipment prefab and a slot(optional).
/// </summary>
public partial struct EquipLoadoutEntry
{
	[KeyProperty] public PrefabFile Prefab { get; set; }
	[KeyProperty] public EquipSlot? Slot { get; set; }

	public EquipLoadoutEntry() { }
	public EquipLoadoutEntry( PrefabFile prefab ) { Prefab = prefab; }
	public EquipLoadoutEntry( PrefabFile prefab, EquipSlot? slot ) { Prefab = prefab; Slot = slot; }
}
