using System;
using System.Runtime.CompilerServices;
using GameFish;

namespace Playground;

[Icon( "rocket_launch" )]
public partial class UprightDevice : AttachedDevice
{
	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public Offset Offset { get; set; }

	[Sync]
	[Property, InlineEditor]
	[Feature( EDITOR ), Group( PHYSICS ), Order( PHYSICS_ORDER )]
	public UprightSettings Settings { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		// Snap to the offset without lag if we're the owner.
		if ( Rigidbody.IsValid() && !Rigidbody.IsProxy )
			TryAttachTo( Rigidbody.GameObject, Offset );
	}

	public override void RenderHelpers()
	{
		base.RenderHelpers();

		RenderUprightHelper();
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		Apply( Time.Delta );
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if ( !this.InGame() || !GameObject.IsValid() )
			return;

		var comps = GameObject.Components.GetAll( FindMode.EverythingInSelf )
			.Where( comp => comp.IsValid() );

		if ( !comps.Any() )
			GameObject.Destroy();
	}

	protected virtual void RenderUprightHelper()
	{
		var tOrigin = GetPhysicsOrigin( Rigidbody );

		var c = Color.White.WithAlpha( 0.3f );

		this.DrawArrow(
			from: tOrigin.Position,
			to: tOrigin.Position + Vector3.Up * 64f,
			c: c, len: 7f, w: 5f,
			tWorld: global::Transform.Zero
		);
	}

	public virtual void Apply( in float deltaTime )
	{
		if ( !Rigidbody.IsValid() || Rigidbody.IsProxy )
			return;

		var tWorld = GetPhysicsOrigin( Rigidbody );

		// Damping
		var torque = Rigidbody.AngularVelocity.Horizontal( Vector3.Up );
		var damp = (deltaTime * Settings.Damping).Min( torque.Length );
		Rigidbody.AngularVelocity -= torque * damp;

		// Upright forces
		var rAngle = Rotation.FromToRotation( tWorld.Forward, Vector3.Up );
		var upright = new Vector3( rAngle.Roll(), rAngle.Pitch(), rAngle.Yaw() );
		var force = upright * Rigidbody.Mass * Settings.Force;

		Rigidbody.ApplyTorque( force );

		/*
		this.DrawArrow(
			from: WorldPosition,
			to: WorldPosition + Vector3.Up * 64f,
			c: Color.White, len: 7f, w: 2f, th: 4f,
			tWorld: global::Transform.Zero
		);
		*/
	}

	public Transform GetPhysicsOrigin( Rigidbody rb )
	{
		// Fallback transform.
		if ( !rb.IsValid() )
			return WorldTransform;

		// Relative transform.
		return rb.WorldTransform.WithOffset( Offset );
	}

	public virtual bool TryAttachTo( GameObject obj, in Offset offs )
	{
		if ( !obj.IsValid() )
			return false;

		WorldTransform = obj.WorldTransform.WithOffset( offs );

		GameObject.SetParent( obj, keepWorldPosition: true );

		Transform.ClearInterpolation();

		return true;
	}
}
