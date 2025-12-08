using System;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;
using Sandbox.Movement;
using Sandbox.VR;

namespace GameFish;

/// <summary>
/// Moves and/or rotates the object.
/// <br /> <br />
/// <b> NOTE: </b> NOT DONE YET!
/// <br />
/// Probably gonna be largely scrapped.
/// <code> func_rotating </code>
/// </summary>
[Hide]
[EditorHandle( Icon = "♻" )]
public partial class Transformer : Component
{
	protected const string OPTIMIZATION = "Optimization";

	protected const string POSITION = "Position";
	protected const string ROTATION = "Rotation";
	protected const string SCALING = "Scaling";

	protected const string OPERATION = "Operation";
	protected const string RELATION = "Relation";
	protected const string OFFSET = "Offset";
	protected const string DEGREES = "Degrees";

	protected const int ORDER_POSITION = 99;
	protected const int ORDER_ROTATION = ORDER_POSITION + 1;
	protected const int ORDER_SCALING = ORDER_ROTATION + 1;

	/// <summary>
	/// If true: always look for valid collision-related components on the target if they havn't been found yet. <br /> <br />
	/// Enable this if you expect the components to change somehow.
	/// </summary>
	[Title( "Auto-Find" )]
	[Property, Feature( TRANSFORM ), Group( OPTIMIZATION )]
	public virtual bool AutoFind { get; set; }

	/// <summary>
	/// THIS SHIT BROKEN. REASON: ENGINE BULLSHIT <br /> <br />
	/// If true: let colliders.. collide. With stuff. Potentially. <br />
	/// If false: just set the target's transform directly with no consideration of collision.
	/// </summary>
	[Property, Feature( TRANSFORM ), Group( PHYSICS )]
	public virtual bool UseCollision => false;

	/// <summary>
	/// If true: velocity is applied instead of directly moving it.
	/// </summary>
	[ShowIf( nameof( UseCollision ), true )]
	[Property, Feature( TRANSFORM ), Group( PHYSICS )]
	public virtual bool UseVelocity { get; set; } = false;

	/*
	/// <summary>
	/// If true: collisions will prevent movement. <br />
	/// If false: it will just crash through stuff.
	/// </summary>
	[ShowIf( nameof( UseCollision ), true )]
	[Property, Feature( TRANSFORM ), Group( GROUP_PHYSICS )]
	public virtual bool Blockable { get; set; } = true;
	*/

	public virtual float DeltaTime => Time.Delta;
	public virtual float TimeScale => Scene?.TimeScale ?? 0f;


	[Title( OPERATION )]
	[Property, Feature( TRANSFORM )]
	[Order( ORDER_POSITION ), Group( POSITION )]
	public virtual TransformOperation MoveOperation { get; set; } = TransformOperation.None;
	public bool HasMoveOperation => MoveOperation is not TransformOperation.None;
	public bool AllowMoving => HasMoveOperation && !UseRotateOrigin && !RotateOrigin.IsValid();

	[Title( RELATION )]
	[Property, Feature( TRANSFORM )]
	[ShowIf( nameof( AllowMoving ), true )]
	[Order( ORDER_POSITION ), Group( POSITION )]
	public virtual RotationRelation MoveRelation { get; set; } = RotationRelation.Object;

	[Property, Feature( TRANSFORM )]
	[ShowIf( nameof( AllowMoving ), true )]
	[Order( ORDER_POSITION ), Group( POSITION )]
	public virtual Vector3 Move { get; set; } = Vector3.Up;


	[Header( "Pitch" )]
	[Title( OPERATION )]
	[Property, Feature( TRANSFORM )]
	[Order( ORDER_POSITION ), Group( ROTATION )]
	public virtual TransformOperation PitchOperation { get; set; } = TransformOperation.Modify;
	protected bool HasPitch => PitchOperation is not TransformOperation.None;

	[Title( RELATION )]
	[Property, Feature( TRANSFORM )]
	[ShowIf( nameof( HasPitch ), true )]
	[Order( ORDER_POSITION ), Group( ROTATION )]
	public virtual RotationRelation PitchRelation { get; set; } = RotationRelation.Axis;
	protected bool HasPitchOffset => HasPitch && PitchRelation is RotationRelation.Absolute;

	[Title( OFFSET )]
	[Property, Feature( TRANSFORM )]
	[ShowIf( nameof( HasPitchOffset ), true )]
	[Order( ORDER_POSITION ), Group( ROTATION )]
	public virtual Rotation PitchOffset { get; set; } = Rotation.Identity;

	[Title( DEGREES )]
	[Property, Feature( TRANSFORM )]
	[ShowIf( nameof( HasPitch ), true )]
	[Order( ORDER_POSITION ), Group( ROTATION )]
	public virtual float PitchDegrees { get; set; } = 90f;


	[Header( "Yaw" )]
	[Title( OPERATION )]
	[Property, Feature( TRANSFORM )]
	[Order( ORDER_POSITION ), Group( ROTATION )]
	public virtual TransformOperation YawOperation { get; set; } = TransformOperation.Modify;
	protected bool HasYaw => YawOperation is not TransformOperation.None;

	[Title( RELATION )]
	[Property, Feature( TRANSFORM )]
	[ShowIf( nameof( HasYaw ), true )]
	[Order( ORDER_POSITION ), Group( ROTATION )]
	public virtual RotationRelation YawRelation { get; set; } = RotationRelation.Axis;
	protected bool HasYawOffset => HasYaw && YawRelation is RotationRelation.Absolute;

	[Title( OFFSET )]
	[Property, Feature( TRANSFORM )]
	[ShowIf( nameof( HasYawOffset ), true )]
	[Order( ORDER_POSITION ), Group( ROTATION )]
	public virtual Rotation YawOffset { get; set; } = Rotation.Identity;

	[Title( DEGREES )]
	[ShowIf( nameof( HasYaw ), true )]
	[Property, Feature( TRANSFORM )]
	[Order( ORDER_POSITION ), Group( ROTATION )]
	public virtual float YawDegrees { get; set; } = 90f;


	[Header( "Roll" )]
	[Title( OPERATION )]
	[Property, Feature( TRANSFORM )]
	[Order( ORDER_POSITION ), Group( ROTATION )]
	public virtual TransformOperation RollOperation { get; set; } = TransformOperation.Modify;
	protected bool HasRoll => RollOperation is not TransformOperation.None;

	[Title( RELATION )]
	[ShowIf( nameof( HasRoll ), true )]
	[Property, Feature( TRANSFORM )]
	[Order( ORDER_POSITION ), Group( ROTATION )]
	public virtual RotationRelation RollRelation { get; set; } = RotationRelation.Axis;
	protected bool HasRollOffset => HasRoll && RollRelation is RotationRelation.Absolute;

	[Title( OFFSET )]
	[Property, Feature( TRANSFORM )]
	[ShowIf( nameof( HasRollOffset ), true )]
	[Order( ORDER_POSITION ), Group( ROTATION )]
	public virtual Rotation RollOffset { get; set; } = Rotation.Identity;

	[Title( DEGREES )]
	[ShowIf( nameof( HasRoll ), true )]
	[Property, Feature( TRANSFORM )]
	[Order( ORDER_POSITION ), Group( ROTATION )]
	public virtual float RollDegrees { get; set; } = 90f;


	/*
	[Header( "Result" )]
	[ReadOnly, JsonIgnore]
	[Property, Feature( TRANSFORM )]
	[Order( ORDER_ROTATION ), Group( GROUP_ROTATION )]
	public virtual Angles RotationAngle => new( PitchDegrees, YawDegrees, RollDegrees );
	public bool HasAnyRotation => PitchDegrees != 0f || YawDegrees != 0f || RollDegrees != 0f;
	*/

	/// <summary>
	/// If true: rotation will be as if <see cref="RotateOrigin"/> was at the center.
	/// </summary>
	[Header( "Origin" )]
	[Property, Feature( TRANSFORM )]
	[Order( ORDER_ROTATION ), Group( ROTATION )]
	public virtual bool UseRotateOrigin { get; set; }

	/// <summary>
	/// The object to be rotated around the position of this object. <br />
	/// If <see cref="UseRotateOrigin"/> is enabled then the offset is created OnStart.
	/// </summary>
	[Property, Feature( TRANSFORM )]
	[ShowIf( nameof( UseRotateOrigin ), true )]
	[Order( ORDER_ROTATION ), Group( ROTATION )]
	public virtual GameObject RotateOrigin { get; set; }
	public Vector3? OriginPosition { get; set; }

	protected Rotation RotationIdentity { get; } = Rotation.Identity;

	public Rotation StartRotation { get; protected set; }

	public Rotation RotationPitch { get; protected set; } = Rotation.Identity;
	public Rotation RotationYaw { get; protected set; } = Rotation.Identity;
	public Rotation RotationRoll { get; protected set; } = Rotation.Identity;

	/// <summary> The resulting rotation of pitch/yaw/roll offsets. </summary>
	public Rotation RotationDelta { get; protected set; } = Rotation.Identity;

	protected override void OnEnabled()
	{
		base.OnEnabled();

		// UpdateColliders();
	}

	protected override void OnStart()
	{
		base.OnStart();

		StartRotation = LocalRotation;
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		var deltaTime = Time.Delta;

		AffectTransform( deltaTime );
	}

	public virtual void AffectTransform( in float deltaTime )
	{
		AffectPosition( deltaTime );
		AffectRotation( deltaTime );
	}

	protected virtual void AffectPosition( in float deltaTime )
	{
		if ( MoveOperation == TransformOperation.None )
			return;

		var rDir = MoveRelation switch
		{
			RotationRelation.Absolute => RotationIdentity,
			RotationRelation.Object => LocalRotation,
			_ => RotationIdentity
		};

		var vMove = rDir * Move;

		switch ( MoveOperation )
		{
			case TransformOperation.Modify:
				WorldPosition += vMove * deltaTime;
				break;

			case TransformOperation.Set:
				LocalPosition = vMove;
				break;
		}
	}

	/// <returns> Where the thing should be moved/rotated to. </returns>
	protected virtual void AffectRotation( in float deltaTime = 1f )
	{
		// No rotating if origin is fucked.
		if ( UseRotateOrigin && (!RotateOrigin.IsValid() || !OriginPosition.HasValue) )
			return;

		var rInverse = LocalRotation.Inverse;
		// Rotation Conjugate() => tDest.Rotation.Conjugate;

		Rotation Absolute( in Rotation rOffset, in Vector3 axis, in float deg )
		{
			var invOffset = rInverse * rOffset;
			return Rotation.FromAxis( (invOffset * axis).Normal, deg );
		}

		Rotation Euler( in Rotation from, in RotationAxis axisEnum, in float deg )
		{
			var vAxis = axisEnum switch
			{
				RotationAxis.Pitch => Vector3.Right,
				RotationAxis.Yaw => Vector3.Up,
				RotationAxis.Roll => Vector3.Forward,
				_ => Vector3.Random.Normal, // heh
			};

			return Rotation.FromAxis( (from.Inverse * (from * vAxis)).Normal, deg );

			// var rAxis = Rotation.FromAxis( rInverse * vAxis, 0f );
			// return Rotation.FromAxis( rAxis * rotationAxis, deg );
		}

		// Pitch
		var fPitch = PitchOperation switch
		{
			TransformOperation.Set => PitchDegrees,
			TransformOperation.Modify => PitchDegrees * deltaTime,
			_ => 0f
		};

		var rPitch = PitchRelation switch
		{
			RotationRelation.Absolute => Absolute( PitchOffset, Vector3.Up, fPitch ),
			RotationRelation.Object => Rotation.FromPitch( fPitch ),
			RotationRelation.Axis => Euler( LocalRotation, RotationAxis.Pitch, fPitch ),
			_ => RotationIdentity
		};

		switch ( PitchOperation )
		{
			case TransformOperation.Modify:
				RotationPitch *= rPitch;
				break;
			case TransformOperation.Set:
				RotationPitch = rPitch;
				break;
		}

		// Yaw
		var fYaw = YawOperation switch
		{
			TransformOperation.Set => YawDegrees,
			TransformOperation.Modify => YawDegrees * deltaTime,
			_ => 0f
		};

		var rYaw = YawRelation switch
		{
			RotationRelation.Absolute => Absolute( YawOffset, Vector3.Up, fYaw ),
			RotationRelation.Object => Rotation.FromYaw( fYaw ),
			RotationRelation.Axis => Euler( LocalRotation, RotationAxis.Yaw, fYaw ),
			_ => RotationIdentity
		};

		// Log.Info( rYaw );

		switch ( YawOperation )
		{
			case TransformOperation.Modify:
				RotationYaw *= rYaw;
				break;
			case TransformOperation.Set:
				RotationYaw = rYaw;
				break;
		}

		// Roll
		var fRoll = RollOperation switch
		{
			TransformOperation.Set => RollDegrees,
			TransformOperation.Modify => RollDegrees * deltaTime,
			_ => 0f
		};

		var rRoll = RollRelation switch
		{
			RotationRelation.Absolute => Absolute( RollOffset, Vector3.Forward, fRoll ),
			RotationRelation.Object => Rotation.FromRoll( fRoll ),
			RotationRelation.Axis => Euler( LocalRotation, RotationAxis.Roll, fRoll ),
			_ => RotationIdentity
		};

		switch ( RollOperation )
		{
			case TransformOperation.Modify:
				RotationRoll *= rRoll;
				break;
			case TransformOperation.Set:
				RotationRoll = rRoll;
				break;
		}

		// Apply Rotation
		RotationDelta = RotationPitch * RotationYaw * RotationRoll;
		// this.Log( "delta: " + RotationDelta );

		LocalRotation = StartRotation * RotationDelta;
		// this.Log( "local: " + RotationDelta );

		// Origin Offset
		if ( UseRotateOrigin && RotateOrigin.IsValid() )
		{
			var vOffset = LocalRotation * (OriginPosition ?? default);
			LocalPosition = RotateOrigin.WorldPosition - vOffset;
		}
	}

	/*
	public virtual void UpdateTargetColliders()
	{
		const FindMode findMode = FindMode.EverythingInSelfAndAncestors | FindMode.InDescendants;

		Colliders = Components.GetAll<Collider>( findMode )
			.Where( c => c.IsValid() && c.KeyframeBody.IsValid() )
			.DistinctBy( c => c.KeyframeBody )
			.DistinctBy( c => c.GameObject );
	}

	protected virtual void SweepMove( PhysicsBody body, in Transform tDest )
	{
		if ( !body.IsValid() || !Scene.IsValid() || !body.GameObject.IsValid() )
			return;

		body.Sleeping = false;
		body.UseController = false;

		var obj = body.GameObject;
		var tObj = obj.WorldTransform;

		var tBodyStart = body.Transform;

		body.Move( tDest, 1f );

		// Setting body position does fuck all so I'll just do it myself.
		var tBodyEnd = body.Transform;

		var tObjEnd = tObj.ToWorld( tObj.ToLocal( tBodyStart ) )
			.ToWorld( tBodyStart.ToLocal( tBodyEnd ) );

		obj.WorldTransform = tObjEnd.WithScale( tObj.Scale );
	}

	protected virtual void VelocityMove( PhysicsBody body, in Transform tDest )
	{
		if ( !body.IsValid() )
			return;

		body.Sleeping = false;
		body.UseController = false;

		var obj = body.GameObject;
		var tObj = obj.WorldTransform;
		var tBodyStart = body.Transform;

		body.SmoothMove( tDest, float.Epsilon, 1f );

		// Setting body position does fuck all so I'll just do it myself.
		var tBodyEnd = body.Transform;

		var tObjEnd = tObj.ToWorld( tObj.ToLocal( tBodyStart ) )
			.ToWorld( tBodyStart.ToLocal( tBodyEnd ) );

		obj.WorldTransform = tObjEnd.WithScale( tObj.Scale );
	}
	*/
}
