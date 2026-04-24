using System.Text.Json.Serialization;

namespace GameFish;

partial class Equipment : ISkinned
{
	[Property]
	[Feature( EQUIP ), Group( MODELS )]
	public Model ViewModel { get; set; }

	[Property, JsonIgnore]
	[Feature( EQUIP ), Group( MODELS )]
	public Model WorldModel { get => WorldRenderer?.Model; set { if ( WorldRenderer.IsValid() ) WorldRenderer.Model = value; } }

	[Property]
	[Feature( EQUIP ), Group( MODELS )]
	public virtual SkinnedModelRenderer WorldRenderer { get; set; }
	SkinnedModelRenderer ISkinned.SkinRenderer { get => WorldRenderer; set => WorldRenderer = value; }
}
