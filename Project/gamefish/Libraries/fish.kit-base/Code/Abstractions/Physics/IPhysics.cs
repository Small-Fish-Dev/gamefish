namespace GameFish;

/// <summary>
/// Lets you access the Rigidbody and affect its forces.
/// </summary>
public interface IPhysics : IVelocity, ITransform
{
	public Rigidbody Rigidbody { get; }
	public PhysicsBody PhysicsBody => Rigidbody?.PhysicsBody;
	public Vector3 MassCenter => PhysicsBody?.MassCenter ?? GetPosition();

	Vector3 IVelocity.Velocity
	{
		get => Rigidbody?.Velocity ?? default;
		set
		{
			if ( Rigidbody.IsValid() )
				Rigidbody.Velocity = value;
		}
	}

	Vector3 IVelocity.GetVelocity() => Velocity;
	void IVelocity.SetVelocity( in Vector3 vel ) => Velocity = vel;
}
