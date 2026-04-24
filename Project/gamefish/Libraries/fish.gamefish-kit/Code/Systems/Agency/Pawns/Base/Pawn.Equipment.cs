using System.Text.Json.Serialization;

namespace GameFish;

partial class Pawn
{
	[Property]
	[Feature( PAWN ), Group( EQUIPMENT )]
	public virtual EquipInventory Equipment
	{
		get => this.GetCached( ref _equip );
		set => _equip = value;
	}

	protected EquipInventory _equip;

	[Property, JsonIgnore]
	[ShowIf( nameof( InGame ), true )]
	[Feature( PAWN ), Group( EQUIPMENT )]
	public virtual Equipment ActiveEquip
	{
		get => Equipment?.ActiveEquip;
		set
		{
			if ( Equipment is var inv && inv.IsValid() )
				inv.TryDeploy( ActiveEquip );
		}
	}
}
