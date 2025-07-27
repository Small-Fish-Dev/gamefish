namespace GameFish;

public abstract partial class ActorSkinnedModel : ActorModel, ISkinned, IRagdoll
{
	[Property, Feature( MODEL )]
	public SkinnedModelRenderer SkinRenderer { get; set; }

	[Property, Feature( MODEL )]
	public ModelPhysics Ragdoll { get; set; }

	public override Model GetModel()
		=> SkinRenderer?.Model;

	public override void SetModel( Model mdl )
	{
		if ( SkinRenderer.IsValid() )
			SkinRenderer.Model = mdl;
	}
}
