namespace GameFish;

/// <summary>
/// A <see cref="BaseEntity"/> that can have a Rigidbody on/in it.
/// </summary>
public partial class PhysicsEntity : BaseEntity, IPhysics
{
	public const string PHYSICS = "🍎 Physics";

	protected Rigidbody _rb;
	public Rigidbody Rigidbody => _rb.IsValid() ? _rb
		: Components?.Get<Rigidbody>( FindMode.EverythingInSelfAndDescendants );

	public PhysicsBody PhysicsBody => Rigidbody?.PhysicsBody;
	public Vector3 MassCenter => PhysicsBody?.MassCenter ?? GetPosition();

	[Property]
	[Feature( DEBUG ), Group( PHYSICS )]
	public virtual Vector3 Velocity
	{
		get => Rigidbody?.Velocity ?? default;
		set
		{
			if ( Rigidbody.IsValid() )
				Rigidbody.Velocity = value;
		}
	}

	public virtual Vector3 GetVelocity() => Velocity;
	public virtual void SetVelocity( in Vector3 vel ) => Velocity = vel;
}
