namespace GameFish;

public partial class ActorCitizenModel : ActorSkinnedModel
{
	public override Model GetModel() => SkinRenderer?.Model;

	public override void SetModel( Model mdl )
	{
		if ( SkinRenderer.IsValid() )
			SkinRenderer.Model = mdl;
	}
}
