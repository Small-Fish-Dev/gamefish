using System;
using System.Runtime.ConstrainedExecution;
using System.Text.Json.Serialization;

namespace GameFish;

partial class Door
{
	[Property]
	[Title( "Speed" )]
	[Order( DOOR_ANIMATION_ORDER )]
	[Range( 0.1f, 10f, clamped: false )]
	[Feature( DOOR ), Group( ANIMATION )]
	public float AnimationSpeed { get; set; } = 3f;

	/// <summary>
	/// Makes the animation ease in and out. Set to zero to disable.
	/// <br /> <br />
	/// <b> NOTE: </b> Effectively reduces speed the higher the value is.
	/// </summary>
	[Property]
	[Title( "Smoothing" )]
	[Order( DOOR_ANIMATION_ORDER )]
	[Range( 0f, 5f, clamped: false )]
	[Feature( DOOR ), Group( ANIMATION )]
	public float AnimationSmoothing { get; set; } = 1f;

	/// <summary>
	/// The current actual fraction representing its position from closed to open.
	/// </summary>
	[Property]
	[JsonIgnore]
	[Title( "Position" )]
	[Order( DOOR_ANIMATION_ORDER )]
	[Range( 0f, 1f, clamped: true )]
	[Feature( DOOR ), Group( ANIMATION )]
	protected float OpenFraction
	{
		get => _openFrac;
		set
		{
			_openFrac = value;
			SetAnimationFraction( in value );
		}
	}

	protected float _openFrac;

	/// <summary>
	/// The child object with the collision and visuals.
	/// </summary>
	[Property]
	[Title( "Child" )]
	[Order( DOOR_ANIMATION_ORDER )]
	[Feature( DOOR ), Group( ANIMATION )]
	public virtual GameObject ModelObject { get; set; }

	/// <summary>
	/// Where the door is when closed.
	/// </summary>
	[Property]
	[Title( "Type" )]
	[WideMode, EnumButtonGroup]
	[Order( DOOR_TRANSFORM_ORDER )]
	[Feature( DOOR ), Group( TRANSFORM )]
	public virtual DoorType Type { get; set; }

	/// <summary>
	/// Where the door is when closed.
	/// </summary>
	[Property]
	[InlineEditor]
	[Title( "Origin" )]
	[Order( DOOR_TRANSFORM_ORDER )]
	[Feature( DOOR ), Group( TRANSFORM )]
	public virtual Offset InitialOffset { get; set; } = new( Vector3.Right * 100f );

	[Property]
	[Title( "Offset" )]
	[Order( DOOR_TRANSFORM_ORDER )]
	[Feature( DOOR ), Group( TRANSFORM )]
	[ShowIf( nameof( Type ), DoorType.Sliding )]
	public virtual Vector3 SlidingDelta { get; set; } = Vector3.Right * -50f;

	[Property]
	[Title( "Pivot" )]
	[Order( DOOR_TRANSFORM_ORDER )]
	[Feature( DOOR ), Group( TRANSFORM )]
	[ShowIf( nameof( Type ), DoorType.Rotating )]
	public virtual Vector3 RotationPivot { get; set; } = Vector3.Zero;

	[Property]
	[Title( "Angles" )]
	[Order( DOOR_TRANSFORM_ORDER )]
	[Feature( DOOR ), Group( TRANSFORM )]
	[ShowIf( nameof( Type ), DoorType.Rotating )]
	public virtual Rotation RotationAngles { get; set; } = Rotation.FromYaw( 90f );

	[Property]
	[InlineEditor]
	[Title( "Offset" )]
	[Order( DOOR_TRANSFORM_ORDER )]
	[Feature( DOOR ), Group( TRANSFORM )]
	[ShowIf( nameof( Type ), DoorType.Manual )]
	public virtual Offset ManualOffset { get; set; } = new( Vector3.Right * -50f );

	/// <summary>
	/// The fraction of the door's position from closed to open that it should slide to.
	/// </summary>
	[Sync]
	public float OpenFractionTarget { get; set; }

	/// <summary>
	/// If the fraction is this close to its destination then it snaps.
	/// </summary>
	protected virtual float AnimationEpsilon => 0.001f;

	protected float _openVel;

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( !IsProxy )
			UpdateAnimation( Time.Delta );
	}

	protected virtual void OnAnimationStart()
	{
		SetAnimationFraction( OpenFraction );
	}

	protected virtual void UpdateAnimation( in float deltaTime )
	{
		UpdatePosition( deltaTime );
	}

	protected virtual void OnAnimationOpening()
	{
		OpenFractionTarget = 1f;
	}

	protected virtual void OnAnimationOpened()
	{
		OpenFractionTarget = 1f;
	}

	protected virtual void OnAnimationClosing()
	{
		OpenFractionTarget = 0f;
	}

	protected virtual void OnAnimationClosed()
	{
		OpenFractionTarget = 0f;
	}

	/// <summary>
	/// Moves the door if it's meant to be moving.
	/// </summary>
	protected virtual void UpdatePosition( in float deltaTime )
	{
		if ( OpenFraction == OpenFractionTarget )
			return;

		var fracTarget = OpenFractionTarget;
		var smooth = AnimationSmoothing;

		float frac;

		if ( smooth > 0 )
		{
			frac = MathX.SmoothDamp( OpenFraction, fracTarget, ref _openVel, smooth, deltaTime * AnimationSpeed );
		}
		else
		{
			frac = OpenFraction.Approach( fracTarget, AnimationSpeed * deltaTime );
		}

		if ( frac.AlmostEqual( fracTarget, AnimationEpsilon ) )
			OpenFraction = fracTarget;
		else
			OpenFraction = frac;
	}

	protected virtual void SetAnimationFraction( in float frac )
	{
		UpdateState( in frac );
		UpdateModel( in frac );
	}

	protected virtual void UpdateState( in float frac )
	{
		if ( IsProxy )
			return;

		if ( frac >= 1 )
		{
			if ( IsOpening )
				State = DoorState.Opened;
		}
		else if ( frac <= 0 )
		{
			if ( IsClosing )
				State = DoorState.Closed;
		}
	}

	protected virtual void UpdateModel( in float frac )
	{
		if ( !ModelObject.IsValid() )
			return;

		Transform t = InitialOffset;

		if ( Type is DoorType.Sliding )
		{
			t.Position += SlidingDelta * frac;
		}
		else if ( Type is DoorType.Rotating )
		{
			var rAdd = RotationAngles * frac;

			t = t.RotateAround( RotationPivot, rAdd );
		}
		else if ( Type is DoorType.Manual )
		{
			t = t.LerpTo( ManualOffset, in frac );
		}

		ModelObject.SetOffset( t );
	}
}

