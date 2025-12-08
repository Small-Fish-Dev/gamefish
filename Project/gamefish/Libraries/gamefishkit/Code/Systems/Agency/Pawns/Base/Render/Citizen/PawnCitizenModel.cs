namespace GameFish;

public partial class PawnCitizenModel : PawnSkinnedModel
{
	public override void SetOpacity( in float a )
	{
		if ( !SkinRenderer.IsValid() )
			return;

		SkinRenderer.Tint = SkinRenderer.Tint.WithAlpha( a );

		foreach ( var cosmetic in SkinRenderer.Components.GetAll<ModelRenderer>( FindMode.InDescendants ) )
			cosmetic.Tint = cosmetic.Tint.WithAlpha( a );
	}
}
