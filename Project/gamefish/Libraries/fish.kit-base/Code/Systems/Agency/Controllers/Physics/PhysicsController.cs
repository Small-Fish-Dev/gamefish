namespace GameFish;

public partial class PhysicsController : BaseController
{
	public override void Move( in float deltaTime )
	{
		PreMove( in deltaTime );
		PostMove( in deltaTime );
	}

	protected override void PreMove( in float deltaTime ) { }

	protected override void PostMove( in float deltaTime )
	{

	}
}
