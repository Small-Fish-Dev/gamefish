namespace GameFish;

public abstract partial class ActorSkinnedModel : ActorModel, ISkinned, IRagdoll
{
	[Property, Feature( BaseActor.FEATURE_ACTOR ), Group( GROUP_MODEL )]
	public SkinnedModelRenderer SkinRenderer { get; set; }

	[Property, Feature( BaseActor.FEATURE_ACTOR ), Group( GROUP_MODEL )]
	public ModelPhysics Ragdoll { get; set; }
}
