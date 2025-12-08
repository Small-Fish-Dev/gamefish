namespace GameFish;

/// <summary>
/// Lets you access the Rigidbody and affect its forces.
/// </summary>
public interface IPhysics : IVelocity, ITransform
{
	public Rigidbody Rigidbody { get; }
	public PhysicsBody PhysicsBody => Rigidbody?.PhysicsBody;
	public Vector3 MassCenter => PhysicsBody?.MassCenter ?? Center;

	Vector3 IVelocity.Velocity
	{
		get => Rigidbody?.Velocity ?? default;
		set
		{
			if ( Rigidbody.IsValid() )
				Rigidbody.Velocity = value;
		}
	}
}
