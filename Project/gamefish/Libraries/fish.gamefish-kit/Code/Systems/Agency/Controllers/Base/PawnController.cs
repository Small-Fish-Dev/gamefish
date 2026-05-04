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

	protected bool HasValidPhysicsModule => Physics.IsValid();

	/// <summary>
	/// Movement/collision logic tries to stay this far away
	/// from surfaces to prevent getting stuck in them.
	/// </summary>
	[Property]
	[Feature( PAWN ), Group( PHYSICS ), Order( PHYSICS_ORDER - 1 )]
	public ControllerPhysics Physics
	{
		get => _phys.AsValid() ?? this?.GetCached( ref _phys );
		set => _phys = value;
	}

	protected ControllerPhysics _phys;

	public Vector3 Velocity
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
