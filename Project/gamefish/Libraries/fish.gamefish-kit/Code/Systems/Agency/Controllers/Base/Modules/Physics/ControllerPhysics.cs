using System;

namespace GameFish;

/// <summary>
/// A module that gives a <see cref="PawnController"/> special physics within its world.
/// <br /> <br />
/// <b> EXPLANATION: </b> Decides how to move and react to collision.
/// <br /> <br />
/// <b> NOTE: </b> Allows easily overriding common character controller features.
/// </summary>
[Icon( "compare_arrows" )]
public abstract partial class ControllerPhysics : ControllerModule
{
	protected const int PHYSICS_ORDER = DEFAULT_ORDER - 1000;

	public Vector3 Up => Controller?.Up ?? WorldRotation.Up;
	public Vector3 Down => -Up;

	[Sync]
	public Vector3 Velocity
	{
		get
		{
			// Always keep the cached value up to date.
			_vel = GetVelocity();
			return _vel;
		}
		set
		{
			// Cache the intended value.
			_vel = value;

			SetVelocity( in value );
			OnSetVelocity( in value );
		}
	}

	protected Vector3 _vel;

	protected virtual Vector3 GetVelocity()
	{
		if ( Rigidbody.IsValid() )
			return Rigidbody.Velocity;

		return _vel;
	}

	protected virtual void SetVelocity( in Vector3 vel )
	{
		if ( Rigidbody.IsValid() )
			Rigidbody.Velocity = vel;
	}

	protected virtual void OnSetVelocity( in Vector3 vel )
	{
		Controller?.OnSetVelocity( vel );
	}

	public virtual void Simulate( in float deltaTime )
	{
		MoveDelta( Velocity * deltaTime );
	}

	protected override void OnStart()
	{
		base.OnStart();

		SetupPhysics();
	}

	/// <summary>
	/// Initalize physics components/settings at start.
	/// </summary>
	protected virtual void SetupPhysics()
		=> UpdatePhysics();

	/// <summary>
	/// Updates physics settings according to parameters.
	/// </summary>
	protected virtual void UpdatePhysics()
	{
	}
}
