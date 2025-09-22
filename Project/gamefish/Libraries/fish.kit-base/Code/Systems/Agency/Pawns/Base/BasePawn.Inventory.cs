namespace GameFish;

partial class BasePawn
{
	[Property]
	[Feature( EQUIP )]
	public PawnEquipment Inventory
		=> GetModule<PawnEquipment>();

	[Property]
	[Feature( EQUIP )]
	public BaseEquip ActiveEquip
	{
		get => Inventory?.ActiveEquip;
		set
		{
			if ( Inventory is var inv && inv.IsValid() )
				inv.ActiveEquip = value;
		}
	}
}