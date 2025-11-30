namespace GameFish;

public class WeightedSpriteList : WeightedResourceList<PrefabFile>
{
	[ResourceType( "sprite" )]
	[KeyProperty, WideMode( HasLabel = false )]
	[Range( WEIGHT_MIN, WEIGHT_MAX, clamped: true ), Step( WEIGHT_STEP )]
	public override Dictionary<string, float> Resources { get; set; }
}
