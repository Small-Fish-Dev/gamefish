using System.Text.Json.Serialization;

namespace GameFish;

partial class Pawn
{
	[Property]
	[Feature( PAWN ), Group( EQUIPMENT )]
	public virtual PawnEquipment Equipment
	{
		get => _equipment.GetCached( this );
		set => _equipment = value;
	}

	protected PawnEquipment _equipment;

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
