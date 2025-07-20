namespace GameFish;

/// <summary>
/// A <see cref="BaseEntity"/> that can have a Rigidbody on/in it.
/// </summary>
public partial class PhysicsEntity : BaseEntity, IPhysics
{
    protected Rigidbody _rb;
    public Rigidbody Rigidbody => _rb = _rb.IsValid() ? _rb
        : Components?.Get<Rigidbody>( FindMode.EverythingInSelfAndDescendants );
}
