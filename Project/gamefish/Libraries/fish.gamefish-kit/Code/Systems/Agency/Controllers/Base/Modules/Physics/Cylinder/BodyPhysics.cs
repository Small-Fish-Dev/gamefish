namespace GameFish;

/// <summary>
/// Provides a <see cref="PawnController"/> Rigidbody-based movement.
/// </summary>
public class BodyPhysics : ControllerPhysics
{
	protected override void SetVelocity( in Vector3 vel )
	{
		if ( Rigidbody.IsValid() )
			Rigidbody.Velocity = vel;
	}

	public override SceneTrace Trace()
	{
		if ( !Scene.IsValid() )
			return default;

		var tr = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject )
			.Body( Rigidbody, Origin.Position )
			.WithCollisionRules( Tags )
			.Rotated( WorldRotation );

		return tr;
	}
}
