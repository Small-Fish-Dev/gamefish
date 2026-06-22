namespace GameFish;

/// <summary>
/// A trigger that affects the velocity of objects. <br />
/// Can push, spin and/or slow physics objects down. <br />
/// Capable of creating, updating and previewing its collision.
/// <code> trigger_push </code> <code> trigger_catapult </code>
/// </summary>
[Icon( "air" )]
[EditorHandle( Icon = "🌬" )]
public partial class VelocityTrigger : FilterTrigger, Component.ExecuteInEditor
{
	protected const int FORCES_ORDER = TRIGGER_ORDER - 1000;

	public const string TITLE_METHOD = "Method";
	public const string TITLE_RELATION = "Relation";
	public const string TITLE_VELOCITY = "Velocity";
	public const string TITLE_NEGATION = "Negation";

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

	/// <summary>
	/// When/how linear velocity(momentum) should be added.
	/// </summary>
	[Property]
	[Order( FORCES_ORDER )]
	[Title( TITLE_METHOD )]
	[Feature( FORCES ), Group( MOMENTUM )]
	public virtual VelocityMethod LinearMethod { get; set; } = VelocityMethod.Continuous;

	/// <summary>
	/// What direction linear velocity(momentum) should be added towards.
	/// </summary>
	[Property]
	[Order( FORCES_ORDER )]
	[Title( TITLE_RELATION )]
	[Feature( FORCES ), Group( MOMENTUM )]
	public virtual VelocityRelation LinearRelation { get; set; } = VelocityRelation.Absolute;

	/// <summary>
	/// Allows you to (optionally) cancel out all/opposing momentum.
	/// </summary>
	[Property]
	[Order( FORCES_ORDER )]
	[Title( TITLE_NEGATION )]
	[Feature( FORCES ), Group( MOMENTUM )]
	public virtual VelocityNegation LinearNegation { get; set; } = VelocityNegation.Opposing;

	public bool IsOpposing => LinearNegation is VelocityNegation.Opposing;
	public bool HasImpulseOpposition => IsOpposing && LinearMethod is VelocityMethod.Instantaneous;
	public bool HasContinuousOpposition => IsOpposing && LinearMethod is VelocityMethod.Continuous;

	/// <summary>
	/// Instantly multiplies the momentum being being cancelled out in the opposite direction.
	/// </summary>
	[Property]
	[Order( FORCES_ORDER )]
	[Title( "Negation Scale" )]
	[Range( 0f, 1f, clamped: true ), Step( .001f )]
	[ShowIf( nameof( HasImpulseOpposition ), true )]
	[Feature( FORCES ), Group( MOMENTUM )]
	public virtual float LinearImpulseNegationScale { get; set; } = 1f;

	/// <summary>
	/// Increases the speed at which previous velocity is negated.
	/// </summary>
	[Property]
	[Order( FORCES_ORDER )]
	[Title( "Negation Scale" )]
	[Range( 0f, 50f, clamped: false ), Step( 0.5f )]
	[ShowIf( nameof( HasContinuousOpposition ), true )]
	[Feature( FORCES ), Group( MOMENTUM )]
	public virtual float LinearContinuousNegationScale { get; set; } = 10f;

	/// <summary>
	/// How much linear velocity(momentum) to add.
	/// </summary>
	[Property]
	[Order( FORCES_ORDER )]
	[Title( TITLE_VELOCITY )]
	[Feature( FORCES ), Group( MOMENTUM )]
	public virtual Vector3 LinearVelocity { get; set; } = Vector3.Up * 1000f;

	/// <summary>
	/// When/how angular velocity(torque/rotation) should be added.
	/// </summary>
	[Property]
	[Order( FORCES_ORDER )]
	[Title( TITLE_METHOD )]
	[Feature( FORCES ), Group( ROTATION )]
	public virtual VelocityMethod AngularMethod { get; set; } = VelocityMethod.None;

	/// <summary>
	/// What orientation angular velocity(torque/rotation) should be added around.
	/// </summary>
	[Property]
	[Order( FORCES_ORDER )]
	[Title( TITLE_RELATION )]
	[Feature( FORCES ), Group( ROTATION )]
	public virtual VelocityRelation AngularRelation { get; set; } = VelocityRelation.Absolute;

	/// <summary>
	/// How much angular velocity(torque/rotation) to add.
	/// </summary>
	[Property]
	[Order( FORCES_ORDER )]
	[Title( TITLE_VELOCITY )]
	[Feature( FORCES ), Group( ROTATION )]
	public virtual Vector3 AngularVelocity { get; set; } = default;

	/// <summary>
	/// If true: objects within this move/spin slower.
	/// </summary>
	[Order( FORCES_ORDER )]
	[Property, Feature( FORCES )]
	[ToggleGroup( nameof( DragEnabled ), Label = DRAG )]
	public virtual bool DragEnabled { get; set; } = false;

	/// <summary>
	/// Higher numbers slow down momentum more.
	/// </summary>
	[Title( "Linear" )]
	[Order( FORCES_ORDER )]
	[Property, Feature( FORCES )]
	[ToggleGroup( nameof( DragEnabled ) )]
	public virtual float LinearDrag { get; set; } = 5f;

	/// <summary>
	/// Higher numbers slow down turning/spinning more.
	/// </summary>
	[Title( "Angular" )]
	[Order( FORCES_ORDER )]
	[Property, Feature( FORCES )]
	[ToggleGroup( nameof( DragEnabled ) )]
	public virtual float AngularDrag { get; set; } = 5f;

	public Rotation DefaultRotation { get; } = Rotation.Identity;

	public override Color DefaultGizmoColor { get; } = Color.Cyan.LerpTo( Color.Green, 0.35f ).Desaturate( 0.5f );

	protected override void RenderTrigger( in bool isGizmo )
	{
		base.RenderTrigger( isGizmo: isGizmo );

		if ( !IsOn )
			return;

		// Linear velocity helper arrow.
		if ( LinearMethod is VelocityMethod.None || LinearRelation is VelocityRelation.Object )
			return;

		Vector3 center = Collider switch
		{
			ColliderType.Manual => Vector3.Zero,
			ColliderType.Box => BoxSize.Center,
			ColliderType.Sphere => Sphere?.Center ?? Vector3.Zero,
			_ => Vector3.Zero
		};

		var r = GetForceRotation( DefaultRotation, LinearRelation );
		var v = r * LinearVelocity.Normal * 64f;

		this.DrawArrow( center, center + v, GizmoColor, tWorld: new( WorldPosition ) );
	}

	protected override void UpdateInsideFixed()
	{
		base.UpdateInsideFixed();

		if ( !IsOn )
			return;

		if ( Touching is null )
			return;

		// Continuous forces/drag.
		foreach ( var obj in Touching )
			TryAffectVelocity( obj, instant: false );
	}

	protected override void OnTouchStart( GameObject obj )
	{
		base.OnTouchStart( obj );

		if ( !IsOn )
			return;

		TryAffectVelocity( obj, instant: true );
	}

	protected virtual Rotation GetForceRotation( in Rotation baseRotation, in VelocityRelation relEnum )
		=> relEnum switch
		{
			VelocityRelation.Absolute => DefaultRotation,
			VelocityRelation.Trigger => WorldRotation,
			VelocityRelation.Object => baseRotation,
			_ => DefaultRotation
		};

	protected virtual Vector3 GetLinearVelocity( Vector3 vel, Rotation r, in bool instant )
	{
		if ( !Scene.IsValid() )
			return default;

		if ( LinearMethod is VelocityMethod.None )
			goto Dragging;

		if ( instant && LinearMethod is not VelocityMethod.Instantaneous )
			goto Dragging;

		if ( !instant && LinearMethod is not VelocityMethod.Continuous )
			goto Dragging;

		r = GetForceRotation( r, LinearRelation );
		var force = r * LinearVelocity;

		Vector3 NegatedVelocity( in Vector3 dir )
		{
			if ( vel.Dot( in dir ) > 0f )
				return vel;

			return vel.SubtractDirection( -dir.ClampLength( 1f ) );
		}

		if ( instant )
		{
			if ( LinearMethod is not VelocityMethod.Instantaneous )
				goto Dragging;

			// Instantaneous force.
			vel = LinearNegation switch
			{
				VelocityNegation.Opposing => NegatedVelocity( force.Normal * LinearImpulseNegationScale ) + force,
				VelocityNegation.Total => force,
				VelocityNegation.None => vel + force,
				_ => vel
			};
		}
		else
		{
			if ( LinearMethod is not VelocityMethod.Continuous )
				goto Dragging;

			// Continuous force.
			var dt = Scene.FixedDelta;

			vel = LinearNegation switch
			{
				VelocityNegation.Opposing => NegatedVelocity( force.Normal * LinearContinuousNegationScale * dt ),
				VelocityNegation.Total => Vector3.Zero,
				_ => vel
			};

			vel += force * dt;
		}

		// Apply drag.
		Dragging:

		if ( !instant && DragEnabled && LinearDrag != 0f )
			vel -= vel.ClampLength( LinearDrag.Abs() * Scene.FixedDelta ) * LinearDrag.Sign();

		return vel;
	}

	protected virtual Vector3 GetAngularVelocity( Vector3 angVel, Rotation r, in bool instant )
	{
		if ( !Scene.IsValid() )
			return default;

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
			r = GetForceRotation( r, AngularRelation );

			if ( instant )
				velAdd = r * AngularVelocity;
			else
				velAdd = r * AngularVelocity * Scene.FixedDelta;
		}

		if ( !instant && DragEnabled && AngularDrag != 0f )
			velAdd -= angVel.ClampLength( AngularDrag.Abs() * Scene.FixedDelta ) * AngularDrag.Sign();

		// Add velocity(helpful comment).
		angVel += velAdd;

		return angVel;
	}

	public virtual bool TryAffectVelocity( GameObject obj, bool instant )
	{
		if ( !IsOn )
			return false;

		if ( obj.IsDestroyed() )
			return false;

		if ( TryGetPawn( obj, out var c ) )
		{
			SetVelocity( c, GetLinearVelocity( c.Velocity, c.WorldRotation, instant: instant ) );
			return true;
		}
		else if ( TryGetRigidbody( obj, out var rb ) )
		{
			SetVelocity( rb,
				GetLinearVelocity( rb.Velocity, rb.WorldRotation, instant: instant ),
				GetAngularVelocity( rb.AngularVelocity, rb.WorldRotation, instant: instant )
			);

			return true;
		}

		return false;
	}

	/// <summary>
	/// Directly modifies the velocity of a <see cref="Rigidbody"/>.
	/// </summary>
	public virtual void SetVelocity( Rigidbody rb, in Vector3 linear, in Vector3 angular )
	{
		if ( !rb.IsValid() )
			return;

		rb.Velocity = linear;
		rb.AngularVelocity = angular;
	}

	/// <summary>
	/// Directly modifies the velocity of a <see cref="Pawn"/>.
	/// </summary>
	public virtual void SetVelocity( Pawn pawn, in Vector3 linear )
	{
		if ( pawn.IsValid() )
			pawn.Velocity = linear;
	}

	/// <summary>
	/// Directly modifies velocities of the object.
	/// </summary>
	public virtual void SetVelocity( GameObject obj, in Vector3 linear, in Vector3 angular )
	{
		if ( TryGetPawn( obj, out var pawn ) )
			SetVelocity( pawn, in linear );
		else if ( TryGetRigidbody( obj, out var rb ) )
			SetVelocity( rb, in linear, in angular );
	}

	protected static bool TryGetRigidbody( GameObject obj, out Rigidbody rb )
	{
		if ( !obj.IsValid() )
		{
			rb = null;
			return false;
		}

		return obj.Components.TryGet( out rb, FindMode.EnabledInSelf | FindMode.InAncestors );
	}

	protected static bool TryGetPawn( GameObject obj, out Pawn pawn )
	{
		if ( !obj.IsValid() )
		{
			pawn = null;
			return false;
		}

		return obj.Components.TryGet( out pawn, FindMode.EnabledInSelf | FindMode.InAncestors );
	}
}
