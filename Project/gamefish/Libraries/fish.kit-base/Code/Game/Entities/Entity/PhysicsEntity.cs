namespace GameFish;

/// <summary>
/// An entity that can have a <see cref="Sandbox.Rigidbody"/>.
/// </summary>
public partial class PhysicsEntity : ModuleEntity, IPhysics
{
	public const string PHYSICS = "🍎 Physics";
	public const int PHYSICS_ORDER = -9001;

	protected Rigidbody _rb;
	public Rigidbody Rigidbody => _rb.IsValid() ? _rb
		: Components?.Get<Rigidbody>( FindMode.EverythingInSelfAndDescendants );

	public PhysicsBody PhysicsBody => Rigidbody?.PhysicsBody;
	public Vector3 MassCenter => PhysicsBody?.MassCenter ?? GetPosition();

	[Property]
	[Feature( ENTITY ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
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
