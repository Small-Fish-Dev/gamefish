namespace GameFish;

public abstract partial class PawnSkinnedModel : PawnBody, ISkinned, IRagdoll
{
	[Property, Feature( MODEL )]
	public SkinnedModelRenderer SkinRenderer { get; set; }

	[Property, Feature( MODEL )]
	public ModelPhysics Ragdoll { get; set; }

	protected override Model GetModel()
		=> SkinRenderer?.Model;

	protected override void SetModel( Model mdl )
	{
		if ( SkinRenderer.IsValid() )
			SkinRenderer.Model = mdl;
	}
}
