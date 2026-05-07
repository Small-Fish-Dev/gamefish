namespace GameFish;

/// <summary>
/// Provides a <see cref="PawnController"/> physical movement capabilities.
/// </summary>
public class BodyPhysics : ControllerPhysics, IScenePhysicsEvents
{
	public override ITagSet TraceTags => Tags;

	public override SceneTrace Trace( in float skin = 0f )
	{
		if ( !Scene.IsValid() )
			return default;

		var rb = Rigidbody;

		if ( !rb.IsValid() )
			return default;

		var tr = Scene.Trace
			.IgnoreGameObjectHierarchy( GameObject )
			.WithCollisionRules( TraceTags )
			.Body( rb, Origin.Position );

		return tr;
	}

	protected override Vector3 GetVelocity() => _vel;
	protected override void SetVelocity( in Vector3 vel ) => _vel = vel;

	/// <summary>
	/// Take the velocity from the <see cref="Rigidbody"/> and apply it to our custom physics.
	/// </summary>
	protected void ApplyBodyVelocity()
	{
		var rb = Rigidbody;

		if ( !rb.IsValid() )
			return;

		_vel += rb.Velocity;
		rb.Velocity = default;
	}

	void IScenePhysicsEvents.PrePhysicsStep()
		=> ApplyBodyVelocity();

	void IScenePhysicsEvents.PostPhysicsStep()
		=> ApplyBodyVelocity();

	protected override void SetupPhysics()
	{
		base.SetupPhysics();

		var rb = Rigidbody;

		if ( rb.IsValid() )
		{
			// Gravity is manually applied.
			rb.Gravity = false;

			// Fuck this garbage default.
			rb.EnableImpactDamage = false;

			// Prevent rotating from the physics engine.
			rb.Locking = rb.Locking with
			{
				Pitch = true,
				Yaw = true,
				Roll = true
			};
		}
	}
}
