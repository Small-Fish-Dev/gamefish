namespace GameFish;

/// <summary>
/// Lets you access the Rigidbody and affect its forces.
/// </summary>
public interface IPhysics : IVelocity, ITransform
{
    public Rigidbody Rigidbody { get; }
    public PhysicsBody PhysicsBody => Rigidbody?.PhysicsBody;
    public Vector3 MassCenter => PhysicsBody?.Position ?? GetPosition();

    Vector3 IVelocity.GetVelocity()
        => PhysicsBody?.Velocity ?? default;

    void IVelocity.SetVelocity( in Vector3 vel )
    {
        if ( PhysicsBody.IsValid() )
            PhysicsBody.Velocity = vel;
    }
}
