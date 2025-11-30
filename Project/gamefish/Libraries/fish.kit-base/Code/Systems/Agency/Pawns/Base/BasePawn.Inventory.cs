using System.Text.Json.Serialization;

namespace GameFish;

partial class BasePawn
{
	[Property]
	[Feature( PAWN ), Group( EQUIPMENT )]
	public virtual PawnEquipment Equipment
		=> !_equipment.IsValid() || _equipment.Parent != this
			? _equipment = GetModule<PawnEquipment>()
			: _equipment;

	protected PawnEquipment _equipment;

	[Property, JsonIgnore]
	[Feature( PAWN ), Group( EQUIPMENT )]
	[ShowIf( nameof( InGame ), true )]
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
