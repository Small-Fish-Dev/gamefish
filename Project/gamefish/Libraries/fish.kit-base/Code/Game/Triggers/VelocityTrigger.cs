using System;
using System.Linq.Expressions;
using System.Runtime;
using System.Threading;
using Microsoft.CSharp.RuntimeBinder;

namespace GameFish;

/// <summary>
/// A trigger that affects the velocity of objects. <br />
/// Can push, spin and/or slow physics objects down. <br />
/// Capable of creating, updating and previewing its collision.
/// <code> trigger_push </code> <code> trigger_catapult </code>
/// </summary>
[Icon( "air" )]
public partial class VelocityTrigger : FilterTrigger, Component.ExecuteInEditor
{
	public const string TITLE_METHOD = "Method";
	public const string TITLE_RELATION = "Relation";
	public const string TITLE_VELOCITY = "Velocity";
	public const string TITLE_NEGATION = "Negation";

	public const string FEATURE_FORCES = "💨 Forces";

	public const string GROUP_LINEAR = "➡ Momentum";
	public const string GROUP_ANGULAR = "♻ Rotation";
	public const string GROUP_DRAG = "🐌 Drag";

	public enum VelocityMethod
	{
		/// <summary>
		/// No force of this type is applied.
		/// </summary>
		None,

		/// <summary>
		/// Applies all force instantly upon entering.
		/// </summary>
		[Icon( "💥" )]
		Instantaneous,

		/// <summary>
		/// Adds momentum consistently over time.
		/// </summary>
		[Icon( "🌬" )]
		Continuous,
	}

	public enum VelocityRelation
	{
		/// <summary>
		/// Always pushes/spins in the exact direction you specify.
		/// </summary>
		Absolute,

		/// <summary>
		/// The trigger's orientation offsets the force's direction.
		/// </summary>
		Trigger,

		/// <summary>
		/// Applies force relative to where the object itself is facing.
		/// </summary>
		Object,
	}

	public enum VelocityNegation
	{
		/// <summary>
		/// Doesn't affect forces prior to applying any new force.
		/// </summary>
		None,

		/// <summary>
		/// Zeros all previous velocity regardless of direction.
		/// </summary>
		Total,

		/// <summary>
		/// Negates all force in the direction opposite of the force being added.
		/// </summary>
		Opposing,
	}

	[Property, Group( GROUP_COLLIDER )]
	public override ColliderType Collider
	{
		get => _colType;
		set { _colType = value; UpdateColliders(); }
	}
	private ColliderType _colType = ColliderType.Box;

	[ShowIf( nameof( UsingBox ), true )]
	[Property, Group( GROUP_COLLIDER )]
	public override BBox BoxSize
	{
		get => _boxSize;
		set { _boxSize = value; UpdateColliders(); }
	}
	private BBox _boxSize = BBox.FromPositionAndSize( Vector3.Zero, 256f );

	/// <summary>
	/// When/how linear velocity(momentum) should be added.
	/// </summary>
	[Title( TITLE_METHOD )]
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_LINEAR )]
	public virtual VelocityMethod LinearMethod { get; set; } = VelocityMethod.Continuous;

	/// <summary>
	/// What direction linear velocity(momentum) should be added towards.
	/// </summary>
	[Title( TITLE_RELATION )]
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_LINEAR )]
	public virtual VelocityRelation LinearRelation { get; set; } = VelocityRelation.Absolute;

	/// <summary>
	/// Allows you to (optionally) cancel out all/opposing momentum.
	/// </summary>
	[Title( TITLE_NEGATION )]
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_LINEAR )]
	public virtual VelocityNegation LinearImpulseNegation { get; set; } = VelocityNegation.Opposing;

	/// <summary>
	/// How much linear velocity(momentum) to add.
	/// </summary>
	[Title( TITLE_VELOCITY )]
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_LINEAR )]
	public virtual Vector3 LinearVelocity { get; set; } = Vector3.Forward * 1000f;

	/// <summary>
	/// When/how angular velocity(torque/rotation) should be added.
	/// </summary>
	[Title( TITLE_METHOD )]
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_ANGULAR )]
	public virtual VelocityMethod AngularMethod { get; set; } = VelocityMethod.Continuous;

	/// <summary>
	/// What orientation angular velocity(torque/rotation) should be added around.
	/// </summary>
	[Title( TITLE_RELATION )]
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_ANGULAR )]
	public virtual VelocityRelation AngularRelation { get; set; } = VelocityRelation.Absolute;

	/// <summary>
	/// How much angular velocity(torque/rotation) to add.
	/// </summary>
	[Title( TITLE_VELOCITY )]
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_ANGULAR )]
	public virtual Vector3 AngularVelocity { get; set; } = default;

	/// <summary>
	/// If true: objects within this move/spin slower.
	/// </summary>
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_DRAG )]
	public virtual bool Drag { get; set; } = false;

	/// <summary>
	/// Higher numbers slow down momentum more.
	/// </summary>
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_DRAG )]
	public virtual float LinearDrag { get; set; } = 5f;

	/// <summary>
	/// Higher numbers slow down turning/spinning more.
	/// </summary>
	[Property, Feature( FEATURE_FORCES ), Group( GROUP_DRAG )]
	public virtual float AngularDrag { get; set; } = 5f;

	public Rotation DefaultRotation { get; } = Rotation.Identity;

	public override Color GizmoColor { get; } = Color.Cyan.LerpTo( Color.Green, 0.35f ).Desaturate( 0.5f );

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( this.InGame() && !DebugGizmos )
			return;

		// Linear velocity helper arrow.
		if ( LinearMethod is VelocityMethod.None || LinearRelation is VelocityRelation.Object )
			return;

		Vector3 center = Collider switch
		{
			ColliderType.Manual => default,
			ColliderType.Box => BoxSize.Center,
			ColliderType.Sphere => Sphere?.Center ?? default,
			_ => default
		};

		var r = GetForceRotation( null, LinearRelation );
		var v = r * LinearVelocity.Normal * 64f;

		this.DrawArrow( center, center + v, GizmoColor, tWorld: new( WorldPosition ) );
	}

	protected override void OnFixedUpdate()
	{
		base.OnFixedUpdate();

		if ( !Scene.IsValid() || this.InEditor() )
			return;

		if ( Touching is null )
			return;

		// Continuous forces/drag.
		var fixedDelta = Scene.FixedDelta;

		foreach ( var obj in Touching )
			if ( TryGetRigidbody( obj, out var rb ) )
				SetVelocity( obj, GetLinearVelocity( rb, onEnter: false ), GetAngularVelocity( rb, instant: false ) );
	}

	protected override void OnTouchStart( GameObject obj )
	{
		base.OnTouchStart( obj );

		if ( TryGetRigidbody( obj, out var rb ) )
			SetVelocity( obj, GetLinearVelocity( rb, onEnter: true ), GetAngularVelocity( rb, instant: true ) );
	}

	protected virtual Rotation GetForceRotation( Rigidbody rb, in VelocityRelation relEnum )
		=> relEnum switch
		{
			VelocityRelation.Absolute => DefaultRotation,
			VelocityRelation.Trigger => WorldRotation,
			VelocityRelation.Object => rb.IsValid() ? rb.WorldRotation : DefaultRotation,
			_ => DefaultRotation
		};

	protected virtual Vector3 GetLinearVelocity( Rigidbody rb, in bool onEnter )
	{
		if ( !Scene.IsValid() || !rb.IsValid() )
			return default;

		Vector3 vel = rb.Velocity;
		Vector3 velAdd;

		if ( LinearMethod is VelocityMethod.None )
		{
			velAdd = default;
		}
		else if ( onEnter && LinearMethod is not VelocityMethod.Instantaneous )
		{
			velAdd = default;
		}
		else
		{
			var r = GetForceRotation( rb, LinearRelation );
			var force = r * LinearVelocity;

			if ( onEnter )
			{
				velAdd = default;

				Vector3 OpposingForce()
				{
					var opposing = rb.Velocity.Forward( -force.Normal );
					var result = vel - opposing + force;

					return result;
				}

				vel = LinearImpulseNegation switch
				{
					VelocityNegation.Opposing => OpposingForce(), // best hl expansion
					VelocityNegation.Total => force,
					_ => vel + force
				};
			}
			else
			{
				velAdd = force * Scene.FixedDelta;
			}
		}

		// Apply drag.
		if ( !onEnter && Drag && LinearDrag != 0f )
			vel -= rb.Velocity.ClampLength( LinearDrag * Scene.FixedDelta );

		// Add velocity(helpful comment).
		vel += velAdd;

		return vel;
	}

	protected virtual Vector3 GetAngularVelocity( Rigidbody rb, in bool instant )
	{
		if ( !Scene.IsValid() || !rb.IsValid() )
			return default;

		Vector3 vel = rb.AngularVelocity;
		Vector3 velAdd;

		if ( AngularMethod is VelocityMethod.None )
		{
			velAdd = default;
		}
		else if ( instant != AngularMethod is VelocityMethod.Instantaneous )
		{
			velAdd = default;
		}
		else
		{
			var r = GetForceRotation( rb, AngularRelation );

			if ( instant )
				velAdd = r * AngularVelocity;
			else
				velAdd = r * AngularVelocity * Scene.FixedDelta;
		}

		if ( !instant && Drag && AngularDrag != 0f )
			velAdd -= rb.AngularVelocity.ClampLength( AngularDrag * Scene.FixedDelta );

		// Add velocity(helpful comment).
		vel += velAdd;

		return vel;
	}

	public static bool TryGetRigidbody( GameObject obj, out Rigidbody rb )
	{
		if ( !obj.IsValid() )
		{
			rb = null;
			return false;
		}

		return obj.Components.TryGet( out rb, FindMode.EnabledInSelf );
	}

	/// <summary>
	/// Directly modifies the velocity of <paramref name="rb"/>.
	/// </summary>
	public virtual void SetVelocity( Rigidbody rb, in Vector3 linear, in Vector3 angular )
	{
		if ( !rb.IsValid() )
			return;

		rb.Velocity = linear;
		rb.AngularVelocity = angular;
	}

	/// <summary>
	/// Directly modifies velocities of the object's <see cref="Rigidbody"/>(if any).
	/// </summary>
	public void SetVelocity( GameObject obj, in Vector3 linear, in Vector3 angular )
	{
		if ( TryGetRigidbody( obj, out var rb ) )
			SetVelocity( rb, in linear, in angular );
	}
}
