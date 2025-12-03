using System.Text.Json.Serialization;

namespace GameFish;

partial class DynamicEntity : IPhysics
{
	protected Rigidbody _rb;
	public Rigidbody Rigidbody => _rb.IsValid() ? _rb
		: Components?.Get<Rigidbody>( FindMode.EverythingInSelf | FindMode.InAncestors );

	public PhysicsBody PhysicsBody => Rigidbody?.PhysicsBody;
	public Vector3 MassCenter => PhysicsBody?.MassCenter ?? WorldPosition;

	/// <summary>
	/// By default this is the velocity of the Rigidbody(if any, otherwise zero).
	/// It could however also be the velocity of some other component.
	/// </summary>
	[Title( "Velocity" )]
	[Property, JsonIgnore]
	[Feature( ENTITY ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	protected Vector3 InspectorVelocity
	{
		get => Velocity;
		set => Velocity = value;
	}

	/// <summary>
	/// By default this is the velocity of the Rigidbody(if any, otherwise zero).
	/// It could however also be the velocity of some other component.
	/// </summary>
	public virtual Vector3 Velocity
	{
		get => Rigidbody?.Velocity ?? Vector3.Zero;
		set
		{
			if ( Rigidbody.IsValid() )
				Rigidbody.Velocity = value;
		}
	}

	[Rpc.Owner( NetFlags.Unreliable | NetFlags.SendImmediate )]
	public virtual void SendImpulse( Vector3 vel )
	{
		if ( !ITransform.IsValid( in vel ) )
			return;

		Velocity += vel;
	}

	public override bool TryTeleport( in Transform tWorld )
	{
		WorldPosition = tWorld.Position;
		WorldRotation = tWorld.Rotation;

		return !IsProxy;
	}
}
