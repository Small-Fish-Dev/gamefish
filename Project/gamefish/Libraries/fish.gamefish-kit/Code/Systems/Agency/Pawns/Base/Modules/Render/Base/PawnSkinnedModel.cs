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

	public override void SetAnim<T>( in string key, in T value )
	{
		if(value is Vector3 vector)
		{
			SkinRenderer.Set(key, vector );
		}
		else if ( value is Rotation rot )
		{
			SkinRenderer.Set( key, rot );
		}
		else if( value is float f )
		{
			SkinRenderer.Set( key, f );
		}
		else if( value is bool b )
		{
			SkinRenderer.Set( key, b );
		}
		else if( value is int i )
		{
			SkinRenderer.Set( key, i );
		}
	}
}
