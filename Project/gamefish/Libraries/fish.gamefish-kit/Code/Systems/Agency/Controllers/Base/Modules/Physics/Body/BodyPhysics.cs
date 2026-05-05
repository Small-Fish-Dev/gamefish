namespace GameFish;

/// <summary>
/// Provides a <see cref="PawnController"/> Rigidbody-based movement.
/// </summary>
public class BodyPhysics : ControllerPhysics
{
	public override ITagSet TraceTags => Tags;

	protected override void SetVelocity( in Vector3 vel )
	{
		if ( Rigidbody.IsValid() )
			Rigidbody.Velocity = vel;
	}

	public override SceneTrace Trace( in float skin = 0f )
	{
		if ( !Scene.IsValid() )
			return default;

		var tr = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject )
			.Body( Rigidbody, Origin.Position )
			.WithCollisionRules( TraceTags )
			.Rotated( WorldRotation );

		return tr;
	}
}
