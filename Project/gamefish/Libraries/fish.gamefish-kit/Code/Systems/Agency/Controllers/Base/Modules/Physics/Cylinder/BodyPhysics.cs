namespace GameFish;

/// <summary>
/// Provides a <see cref="PawnController"/> Rigidbody-based movement.
/// </summary>
public class BodyPhysics : ControllerPhysics
{
	public override SceneTrace Trace()
	{
		if ( !Scene.IsValid() )
			return default;

		var tr = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject )
			.Body( Rigidbody, WorldPosition )
			.WithCollisionRules( Tags )
			.Rotated( WorldRotation );

		return tr;
	}
}
