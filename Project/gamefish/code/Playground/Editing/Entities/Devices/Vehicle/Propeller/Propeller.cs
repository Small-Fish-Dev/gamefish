using System.Text.Json.Serialization;
using GameFish;

namespace Playground;

[Icon( "wind_power" )]
public partial class Propeller : Device
{
	[Property, JsonIgnore, ReadOnly]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Rigidbody Rigidbody => _rb.GetCached( GameObject, FindMode.EnabledInSelf | FindMode.InAncestors );
	protected Rigidbody _rb;

	[Property, JsonIgnore, ReadOnly]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Rigidbody TargetRigidbody => _rbTarget.GetCached( ParentPoint.Object, FindMode.EnabledInSelf | FindMode.InAncestors );
	protected Rigidbody _rbTarget;

	[Property]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Sandbox.WheelJoint Joint { get; set; }

	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public PropellerSettings Settings { get; set; }

	[Sync]
	public DeviceAttachPoint ParentPoint { get; set; }

	/// <summary>
	/// The multiplier of speed to apply (from <c>-1</c> to <c>1</c>).
	/// </summary>
	[Sync]
	[Property]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public float Throttle
	{
		get => _throttle.Clamp( -1f, 1f );
		set => _throttle = value.Clamp( -1f, 1f );
	}

	protected float _throttle;

	/// <summary>
	/// The body's current torque.
	/// </summary>
	[Sync]
	[Property, JsonIgnore]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Vector3 AngularVelocity
	{
		get => Rigidbody?.AngularVelocity ?? default;
		set
		{
			if ( Rigidbody.IsValid() )
				Rigidbody.AngularVelocity = value;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();

		// Snap to the offset without lag if we're the owner.
		if ( GameObject.Parent.IsValid() && !GameObject.Parent.IsProxy )
			TryAttachTo( point: ParentPoint );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		UpdateInput( Time.Delta );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		Simulate( Time.Delta );
	}

	protected virtual void UpdateInput( in float deltaTime )
	{
		if ( IsProxy )
			return;

		var drive = 0f;

		var bAccel = !Settings.KeyForward.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyForward );

		var bReverse = !Settings.KeyReverse.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyReverse );

		if ( bAccel )
			drive += 1;

		if ( bReverse )
			drive -= 1;

		Throttle = drive;
	}

	public void Simulate( in float deltaTime )
	{
		if ( !Joint.IsValid() || !Rigidbody.IsValid() )
			return;

		var tBody = Rigidbody.WorldTransform;
		var torque = Rigidbody.AngularVelocity * tBody.Up;

		var angVel = Rigidbody.AngularVelocity;
		angVel -= torque;

		// Friction
		torque = torque.WithFriction( Settings.Friction, deltaTime );

		// Acceleration
		var accel = Throttle * Settings.Speed;
		torque += accel * deltaTime;

		// Limit
		var limit = Settings.Limit.Positive();
		torque = torque.ClampLength( 0, limit );

		// Apply
		angVel += torque;
		Rigidbody.AngularVelocity = angVel;

		// Lift
		var lift = tBody.Up * torque;
		lift *= Settings.TorqueLift;

		if ( TargetRigidbody.IsValid() )
			TargetRigidbody.Velocity += lift;

		// var vLocalCenter = TargetRigidbody.MassCenter;
		// var vWorldCenter = TargetRigidbody.WorldTransform.PointToWorld( vLocalCenter );

		// TargetRigidbody.ApplyForceAt( vWorldCenter, lift );
	}

	public bool TryAttachTo( in DeviceAttachPoint point )
	{
		// Must have a Device(the entire point).
		if ( !Joint.IsValid() )
			return false;

		var objTarget = point.Object;

		if ( !objTarget.IsValid() || point.Offset is not Offset offset )
			return false;

		// Disgraceful..
		if ( objTarget == GameObject )
			return false;

		var aPhys = Physics.FindBody( GameObject );

		if ( !aPhys.IsValid() )
			return false;

		var bPhys = Physics.FindBody( objTarget );

		if ( !bPhys.IsValid() )
			return false;

		var tParent = objTarget.WorldTransform.WithOffset( offset );
		tParent.Rotation *= Rotation.FromPitch( 90f );

		var aPhysLocal = Joint.LocalTransform;
		var bPhysLocal = bPhys.Transform.ToLocal( tParent );

		if ( !ITransform.IsValid( aPhysLocal ) || !ITransform.IsValid( bPhysLocal ) )
			return false;

		var tWheel = tParent
			.ToWorld( aPhysLocal )
			.WithScale( WorldScale );

		WorldTransform = tWheel;

		Transform.ClearInterpolation();

		// Let the engine's component handle it.
		Joint.Attachment = Sandbox.Joint.AttachmentMode.LocalFrames;

		Joint.LocalFrame1 = aPhysLocal;
		Joint.LocalFrame2 = bPhysLocal;

		// Set the other body.
		Joint.Body = objTarget;

		return true;
	}

	protected void DrawDeviceGizmos()
	{
		var objParent = ParentPoint.Object;

		if ( !objParent.IsValid() )
			return;

		if ( ParentPoint.Offset is not Offset parentOffset )
			return;

		var c = Color.White.WithAlpha( 0.3f );

		var tParentPoint = objParent.WorldTransform.WithOffset( parentOffset );

		this.DrawArrow(
			from: WorldPosition,
			to: tParentPoint.Position,
			c: c, len: 7f, w: 2f, th: 4f,
			tWorld: global::Transform.Zero
		);
	}
}
