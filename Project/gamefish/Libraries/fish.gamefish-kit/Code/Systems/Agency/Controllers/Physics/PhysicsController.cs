namespace GameFish;

/// <summary>
/// A very basic controller meant for use with a <see cref="Rigidbody"/>.
/// </summary>
public abstract class PhysicsController : BaseController
{
	protected override void Move( in float deltaTime )
	{
		PreMove( in deltaTime );
		PostMove( in deltaTime );
	}

	protected override void PreMove( in float deltaTime ) { }

	protected override void PostMove( in float deltaTime )
	{
	}
}
