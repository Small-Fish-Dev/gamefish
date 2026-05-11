using System.Text.Json.Serialization;

namespace GameFish;

partial class DynamicEntity : IPhysics
{
	public Rigidbody Rigidbody => GameObject.GetCached( ref _rb, FindMode.EverythingInSelf | FindMode.InAncestors );

	protected Rigidbody _rb;

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
	public void RpcImpulse( Vector3 vel )
	{
		if ( !ITransform.IsValid( in vel ) )
			return;

		ApplyImpulse( vel );
	}

	/// <summary>
	/// Allows the owner to apply velocity in a way that can be altered.
	/// <br /> <br />
	/// <b> NOTE: </b> May be called by others with <see cref="RpcImpulse"/>.
	/// </summary>
	public virtual void ApplyImpulse( Vector3 vel )
	{
		Velocity += vel;
	}

	public override bool TryTeleport( in Transform tWorld )
	{
		if ( IsProxy )
			return false;

		WorldPosition = tWorld.Position;
		WorldRotation = tWorld.Rotation;

		return true;
	}
}
