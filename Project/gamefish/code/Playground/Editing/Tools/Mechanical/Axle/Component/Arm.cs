using System;
using System.Numerics;
using System.Text.Json.Serialization;
using Sandbox.Physics;

namespace Playground;

[Icon( "rocket_launch" )]
public partial class Arm : Entity
{
	protected const int EDITOR_ORDER = DEFAULT_ORDER - 1000;
	protected const int PHYSICS_ORDER = EDITOR_ORDER + 10;

	[Property]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Sandbox.BallJoint Ball { get; set; }

	[Sync]
	public ToolAttachPoint ParentPoint { get; set; }

	[Sync]
	public ToolAttachPoint TargetPoint { get; set; }

	/// <summary>
	/// The key you press to activate the spiner you're placing.
	/// </summary>
	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public ArmSettings Settings { get; set; }

	[Sync]
	[Property, ReadOnly]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Angles SteerAngles { get; set; } = Angles.Zero;

	[Sync]
	[Property]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Angles TargetAngles
	{
		get => _targetAngles;
		set => _targetAngles = value.Normal;
	}

	protected Angles _targetAngles;

	protected override void OnStart()
	{
		base.OnStart();

		// Snap to the offset without lag if we're the owner.
		if ( GameObject.Parent.IsValid() && !GameObject.Parent.IsProxy )
			TryAttachTo( a: ParentPoint, b: TargetPoint );
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		DrawPhysicsGizmo();

		if ( IsProxy )
			return;

		UpdateSteering( Time.Delta );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( SteerAngles.IsNearlyZero() )
			return;

		Apply( Time.Delta );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( !this.InGame() || !GameObject.IsValid() )
			return;

		// Auto-cleanup empty objects that only had a spiner.
		var comps = GameObject.Components.GetAll( FindMode.EverythingInSelf )
			.Where( comp => comp.IsValid() );

		if ( !comps.Any() )
			GameObject.Destroy();
	}

	protected virtual void UpdateSteering( in float deltaTime )
	{
		var steer = Angles.Zero;

		// Pitch
		var pitchUp = !Settings.KeyPitchUp.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyPitchUp );

		var pitchDown = !Settings.KeyPitchDown.IsBlank()
			&& Input.Keyboard.Down( Settings.KeyPitchDown );

		if ( pitchUp )
			steer.pitch += 1;

		if ( pitchDown )
			steer.pitch -= 1;

		SteerAngles = steer;
	}

	protected virtual void DrawPhysicsGizmo()
	{
		var c = Color.Green.WithAlpha( 0.3f );

		var tObj = WorldTransform.WithScale( 1f );
		var dir = tObj.Forward;

		this.DrawArrow(
			from: tObj.Position,
			to: tObj.Position + (dir * 6f),
			c: c, len: 0.1f, w: 5f, th: 4f,
			tWorld: global::Transform.Zero
		);
	}

	public virtual void Apply( in float deltaTime )
	{
		if ( !Ball.IsValid() )
			return;

		if ( !ParentPoint.Object.IsValid() )
			return;

		var dirYaw = SteerAngles.yaw.Clamp( -1f, 1f );
		var dirPitch = SteerAngles.pitch.Clamp( -1f, 1f );
		var dirRoll = SteerAngles.roll.Clamp( -1f, 1f );

		var addYaw = dirYaw * Settings.Speed * deltaTime;
		var addPitch = dirPitch * Settings.Speed * deltaTime;
		var addRoll = dirRoll * Settings.Speed * deltaTime;

		var angTarget = TargetAngles;

		const float angleLimit = -89.5f;

		angTarget.yaw = (angTarget.yaw + addYaw).Clamp( -angleLimit, angleLimit );
		angTarget.pitch = (angTarget.pitch + addPitch).Clamp( -angleLimit, angleLimit );
		angTarget.roll = (angTarget.roll + addRoll).Clamp( -angleLimit, angleLimit );

		TargetAngles = angTarget;

		Ball.TargetRotation = TargetAngles;
	}

	public virtual bool TryAttachTo( in ToolAttachPoint a, in ToolAttachPoint b )
	{
		// Must have a Ball(the entire point).
		if ( !Ball.IsValid() )
			return false;

		var objParent = a.Object;
		var objTarget = b.Object;

		if ( !objParent.IsValid() || !objTarget.IsValid() )
			return false;

		// Disgraceful..
		if ( objParent == objTarget )
			return false;

		var aPhys = Physics.FindBody( objParent );

		if ( !aPhys.IsValid() || a.Offset is not Offset parentOffset )
			return false;

		var bPhys = Physics.FindBody( objTarget );

		if ( !aPhys.IsValid() || b.Offset is not Offset targetOffset )
			return false;

		var tParentPoint = objParent.WorldTransform.WithOffset( parentOffset );
		var tTargetPoint = objTarget.WorldTransform.WithOffset( targetOffset );

		var aPhysLocal = aPhys.Transform.ToLocal( tParentPoint );
		var bPhysLocal = bPhys.Transform.ToLocal( tParentPoint );

		// var aPhysPoint = new PhysicsPoint( aPhys, aPhysLocal.Position, aPhysLocal.Rotation );
		// var bPhysPoint = new PhysicsPoint( bPhys, bPhysLocal.Position, bPhysLocal.Rotation );

		if ( !ITransform.IsValid( aPhysLocal ) || !ITransform.IsValid( bPhysLocal ) )
			return false;

		LocalTransform = parentOffset;

		GameObject.SetParent( objParent, keepWorldPosition: true );
		Transform.ClearInterpolation();

		// Let the engine's component handle it.
		Ball.Attachment = Joint.AttachmentMode.LocalFrames;

		Ball.LocalFrame1 = aPhysLocal;
		Ball.LocalFrame2 = bPhysLocal;

		// Set the other body.
		Ball.Body = objTarget;

		return true;
	}
}
