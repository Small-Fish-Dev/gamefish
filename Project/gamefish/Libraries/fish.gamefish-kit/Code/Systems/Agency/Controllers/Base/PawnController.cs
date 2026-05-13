using System;
using System.Text.Json.Serialization;

namespace GameFish;

/// <summary>
/// Something that takes input to move around.
/// <br /> <br />
/// <b> NOTE: </b> Meant to be controlled by a <see cref="Pawn"/>.
/// </summary>
[Icon( "directions_run" )]
public abstract partial class PawnController : PawnModule
{
	protected const int AIMING_ORDER = PAWN_ORDER + 100;
	protected const int EYEPOS_ORDER = PAWN_ORDER + 200;

	protected const int SPRINT_ORDER = PAWN_ORDER + 300;
	protected const int DUCKING_ORDER = PAWN_ORDER + 400;
	protected const int JUMPING_ORDER = PAWN_ORDER + 500;

	public Rigidbody Rigidbody => Pawn?.Rigidbody;

	public PawnView View => Pawn?.View;

	/// <summary>
	/// The force of gravity for this controller.
	/// </summary>
	public virtual Vector3 Gravity => SceneGravity;

	/// <summary>
	/// The gravity things fall(if they do) by default.
	/// </summary>
	protected Vector3 SceneGravity => Scene?.PhysicsWorld?.Gravity ?? default;

	protected virtual Vector3 LocalBottom => Vector3.Zero;
	protected virtual Vector3 LocalTop => Vector3.Up * GetLocalEyeTargetPosition();

	public Vector3 Bottom => WorldTransform.PointToWorld( LocalBottom );
	public Vector3 Top => WorldTransform.PointToWorld( LocalTop );

	public override Vector3 Center => WorldTransform.PointToWorld( LocalBottom.LerpTo( LocalTop, 0.5f ) );

	/// <summary>
	/// The rotation that we use for movement direction and such.
	/// </summary>
	public virtual Rotation Perspective => WorldRotation * Rotation.FromYaw( LocalEyeRotation.Yaw() );

	/// <summary> The upward direction from our current perspective. </summary>
	public Vector3 Up => Perspective.Up;
	/// <summary> The downward direction from our current perspective. </summary>
	public Vector3 Down => Perspective.Down;

	/// <summary> East from our current perspective. </summary>
	public Vector3 Left => Perspective.Left;
	/// <summary> West from our current perspective. </summary>
	public Vector3 Right => Perspective.Right;

	/// <summary> South from our current perspective. </summary>
	public Vector3 Back => Perspective.Backward;
	/// <summary> North from our current perspective. </summary>
	public Vector3 Forward => Perspective.Forward;


	/// <summary>
	/// Movement/collision logic tries to stay this far away
	/// from surfaces to prevent getting stuck in them.
	/// </summary>
	[Property]
	[JsonIgnore]
	[Title( "Type" )]
	[TargetType( typeof( ControllerPhysics ) )]
	[ShowIf( nameof( HasValidPhysicsModule ), false )]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER - 1 )]
	[InfoBox( "You must have a component for the controller's physics or it will not be able to move. Select one below.", Icon = "warning", Tint = EditorTint.Red )]
	protected Type AddPhysicsModuleType
	{
		get => null;
		set => Components?.Create( TypeLibrary.GetType( value ) );
	}

	protected virtual bool HasValidPhysicsModule => Physics.IsValid();

	/// <summary>
	/// Handles interactions with the surrounding physical world.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER - 1 )]
	public ControllerPhysics Physics
	{
		get => _phys.AsValid() ?? this?.GetCached( ref _phys );
		set => _phys = value;
	}

	protected ControllerPhysics _phys;

	public virtual Vector3 Velocity
	{
		get => Physics?.Velocity ?? default;
		set
		{
			if ( Physics is var phys && phys.IsValid() )
				phys.Velocity = value;
		}
	}

	public virtual void OnSetVelocity( in Vector3 vel )
	{
	}

	protected override void OnStart()
	{
		base.OnStart();

		SetupView();
	}

	/// <summary>
	/// Ran by the paret pawn just before movement is performed.
	/// </summary>
	public virtual void Simulate( in float deltaTime, in bool isFixedUpdate )
	{
		UpdateInput( in deltaTime );
		SimulateView( in deltaTime );
	}
}
