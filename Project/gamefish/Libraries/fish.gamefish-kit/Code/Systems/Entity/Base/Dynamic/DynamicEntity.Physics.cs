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
			var rb = Rigidbody;

			if ( rb.IsValid() && rb.MotionEnabled )
				rb.Velocity = value;
		}
	}

	public virtual bool CanImpulse( in Vector3 vel, in Vector3? point = null )
	{
		if ( vel == default || !ITransform.IsValid( vel ) )
			return false;

		var rb = Rigidbody;

		if ( rb.IsValid() )
		{
			if ( !rb.MotionEnabled )
				return false;

			if ( rb.PhysicsBody?.BodyType is PhysicsBodyType.Static )
				return false;
		}

		return true;
	}

	public virtual bool TryImpulse( in Vector3 vel, in Vector3? point = null )
	{
		if ( !GameObject.IsValid() )
			return false;

		if ( !CanImpulse( in vel ) )
			return false;

		if ( IsProxy )
			RpcImpulse( vel, point );
		else
			ApplyImpulse( vel, point );

		return true;
	}

	[Rpc.Owner( NetFlags.Unreliable | NetFlags.SendImmediate )]
	protected void RpcImpulse( Vector3 vel, Vector3? point = null )
	{
		if ( !CanImpulse( in vel, in point ) )
			return;

		ApplyImpulse( vel );
	}

	/// <summary>
	/// Allows the owner to apply velocity in a way that can be altered.
	/// <br /> <br />
	/// <b> NOTE: </b> May be called by others with <see cref="RpcImpulse"/>.
	/// </summary>
	protected virtual void ApplyImpulse( Vector3 vel, Vector3? point = null )
	{
		if ( !ITransform.IsValid( in vel ) )
			return;

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
