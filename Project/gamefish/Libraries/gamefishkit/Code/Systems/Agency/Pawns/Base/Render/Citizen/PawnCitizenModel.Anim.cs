using System;

namespace GameFish;

partial class PawnCitizenModel
{
	/// <summary> Where are the eyes of our character? </summary>
	[Property, Feature( MODEL )]
	public GameObject EyeSource { get; set; }

	/// <summary> How tall are we? </summary>
	[Feature( MODEL )]
	[DefaultValue( 0.5f )]
	[Property, Range( 0.5f, 1.5f ), Title( "Avatar Height Scale" )]
	public float? Height { get; set; }

	/// <summary> Are we looking at something? Useful for stuff like cutscenes, where you want an NPC to stare at you. </summary>
	[Feature( MODEL )]
	[Property, ToggleGroup( nameof( LookAtEnabled ), Label = "Look At" )]
	public bool LookAtEnabled { get; set; } = false;

	/// <summary> Which GameObject should we be looking at? </summary>
	[Feature( MODEL )]
	[Property, ToggleGroup( nameof( LookAtEnabled ) )] public GameObject LookAt { get; set; }

	[Feature( MODEL )]
	[Property, ToggleGroup( nameof( LookAtEnabled ) ), Range( 0, 1 )]
	public float EyesWeight { get; set; } = 1.0f;

	[Feature( MODEL )]
	[Property, ToggleGroup( nameof( LookAtEnabled ) ), Range( 0, 1 )]
	public float HeadWeight { get; set; } = 1.0f;

	[Feature( MODEL )]
	[Property, ToggleGroup( nameof( LookAtEnabled ) ), Range( 0, 1 )]
	public float BodyWeight { get; set; } = 1.0f;


	/// <summary> IK will try to place the limb where this GameObject is in the world. </summary>
	[Feature( MODEL )]
	[Property, Group( "Inverse kinematics" ), Title( "Left Hand" )]
	public GameObject IkLeftHand { get; set; }

	/// <inheritdoc cref="IkLeftHand"/>
	[Feature( MODEL )]
	[Property, Group( "Inverse kinematics" ), Title( "Right Hand" )] public GameObject IkRightHand { get; set; }

	/// <inheritdoc cref="IkLeftHand"/>
	[Feature( MODEL )]
	[Property, Group( "Inverse kinematics" ), Title( "Left Foot" )] public GameObject IkLeftFoot { get; set; }

	/// <inheritdoc cref="IkLeftHand"/>
	[Feature( MODEL )]
	[Property, Group( "Inverse kinematics" ), Title( "Right Foot" )] public GameObject IkRightFoot { get; set; }

	protected override void OnUpdate()
	{
		if ( !SkinRenderer.IsValid() )
			return;

		UpdateOpacity();

		if ( LookAt.IsValid() && LookAtEnabled )
		{
			var eyePos = EyeWorldTransform.Position;

			var dir = (LookAt.WorldPosition - eyePos).Normal;
			WithLook( dir, EyesWeight, HeadWeight, BodyWeight );
		}

		if ( Height.HasValue )
		{
			SkinRenderer.Set( "scale_height", Height.Value );
		}

		if ( IkLeftHand.IsValid() && IkLeftHand.Active ) SkinRenderer.SetIk( "hand_left", IkLeftHand.WorldTransform );
		else SkinRenderer.ClearIk( "hand_left" );

		if ( IkRightHand.IsValid() && IkRightHand.Active ) SkinRenderer.SetIk( "hand_right", IkRightHand.WorldTransform );
		else SkinRenderer.ClearIk( "hand_right" );

		if ( IkLeftFoot.IsValid() && IkLeftFoot.Active ) SkinRenderer.SetIk( "foot_left", IkLeftFoot.WorldTransform );
		else SkinRenderer.ClearIk( "foot_left" );

		if ( IkRightFoot.IsValid() && IkRightFoot.Active ) SkinRenderer.SetIk( "foot_right", IkRightFoot.WorldTransform );
		else SkinRenderer.ClearIk( "foot_right" );
	}

	public void ProceduralHitReaction( DamageInfo info, float damageScale = 1.0f, Vector3 force = default )
	{
		var boneId = info.Hitbox?.Bone?.Index ?? 0;
		var bone = SkinRenderer.GetBoneObject( boneId );

		var localToBone = bone.LocalPosition;
		if ( localToBone == Vector3.Zero ) localToBone = Vector3.One;

		SkinRenderer.Set( "hit", true );
		SkinRenderer.Set( "hit_bone", boneId );
		SkinRenderer.Set( "hit_offset", localToBone );
		SkinRenderer.Set( "hit_direction", force.Normal );
		SkinRenderer.Set( "hit_strength", (force.Length / 1000.0f) * damageScale );
	}

	/// <summary> The transform of the eyes, in world space. This is worked out from EyeSource is it's set. </summary>
	public virtual Transform EyeWorldTransform
	{
		get
		{
			if ( EyeSource.IsValid() )
				return EyeSource.WorldTransform;

			return WorldTransform;
		}
	}

	/// <summary> Have the player look at this point in the world </summary>
	public void WithLook( Vector3 lookDirection, float eyesWeight = 1.0f, float headWeight = 1.0f, float bodyWeight = 1.0f )
	{
		SkinRenderer.SetLookDirection( "aim_eyes", lookDirection, eyesWeight );
		SkinRenderer.SetLookDirection( "aim_head", lookDirection, headWeight );
		SkinRenderer.SetLookDirection( "aim_body", lookDirection, bodyWeight );
	}

	/// <summary> Have the player animate moving with a set velocity (this doesn't move them! Your character controller is responsible for that) </summary>
	public void WithVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = SkinRenderer.WorldRotation.Forward.Dot( dir );
		var sideward = SkinRenderer.WorldRotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		SkinRenderer.Set( "move_direction", angle );
		SkinRenderer.Set( "move_speed", Velocity.Length );
		SkinRenderer.Set( "move_groundspeed", Velocity.WithZ( 0 ).Length );
		SkinRenderer.Set( "move_y", sideward );
		SkinRenderer.Set( "move_x", forward );
		SkinRenderer.Set( "move_z", Velocity.z );
	}

	/// <summary>
	/// Animates the wish for the character to move in a certain direction. For example, when in the air, your character will swing their arms in that direction.
	/// </summary>
	/// <param name="Velocity"></param>
	public void WithWishVelocity( Vector3 Velocity )
	{
		var dir = Velocity;
		var forward = SkinRenderer.WorldRotation.Forward.Dot( dir );
		var sideward = SkinRenderer.WorldRotation.Right.Dot( dir );

		var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

		SkinRenderer.Set( "wish_direction", angle );
		SkinRenderer.Set( "wish_speed", Velocity.Length );
		SkinRenderer.Set( "wish_groundspeed", Velocity.WithZ( 0 ).Length );
		SkinRenderer.Set( "wish_y", sideward );
		SkinRenderer.Set( "wish_x", forward );
		SkinRenderer.Set( "wish_z", Velocity.z );
	}

	/// <summary>
	/// Where are we aiming?
	/// </summary>
	public Rotation AimAngle
	{
		set
		{
			value = SkinRenderer.WorldRotation.Inverse * value;
			var ang = value.Angles();

			SkinRenderer.Set( "aim_body_pitch", ang.pitch );
			SkinRenderer.Set( "aim_body_yaw", ang.yaw );
		}
	}

	/// <summary> The weight of the aim angle, but specifically for the Citizen's eyes. </summary>
	public float AimEyesWeight
	{
		get => SkinRenderer.GetFloat( "aim_eyes_weight" );
		set => SkinRenderer.Set( "aim_eyes_weight", value );
	}

	/// <summary> The weight of the aim angle, but specifically for the Citizen's head. </summary>
	public float AimHeadWeight
	{
		get => SkinRenderer.GetFloat( "aim_head_weight" );
		set => SkinRenderer.Set( "aim_head_weight", value );
	}


	/// <summary> The weight of the aim angle, but specifically for the Citizen's body. </summary>
	public float AimBodyWeight
	{
		get => SkinRenderer.GetFloat( "aim_body_weight" );
		set => SkinRenderer.Set( "aim_body_weight", value );
	}

	/// <summary>
	/// How much the character is rotating in degrees per second, this controls feet shuffling.
	/// If rotating clockwise this should be positive, if rotating counter-clockwise this should be negative.
	/// </summary>
	public float MoveRotationSpeed
	{
		get => SkinRenderer.GetFloat( "move_rotationspeed" );
		set => SkinRenderer.Set( "move_rotationspeed", value );
	}

	[Obsolete( "Use MoveRotationSpeed" )]
	public float FootShuffle
	{
		get => SkinRenderer.GetFloat( "move_shuffle" );
		set => SkinRenderer.Set( "move_shuffle", value );
	}

	/// <summary> The scale of being ducked (crouched) (0 - 1) </summary>
	public float DuckLevel
	{
		get => SkinRenderer.GetFloat( "duck" );
		set => SkinRenderer.Set( "duck", value );
	}

	/// <summary> How loud are we talking? </summary>
	public float VoiceLevel
	{
		get => SkinRenderer.GetFloat( "voice" );
		set => SkinRenderer.Set( "voice", value );
	}

	/// <summary> Are we sitting down? </summary>
	public bool IsSitting
	{
		get => SkinRenderer.GetBool( "b_sit" );
		set => SkinRenderer.Set( "b_sit", value );
	}

	/// <summary> Are we on the ground? </summary>
	public bool IsGrounded
	{
		get => SkinRenderer.GetBool( "b_grounded" );
		set => SkinRenderer.Set( "b_grounded", value );
	}

	/// <summary> Are we swimming? </summary>
	public bool IsSwimming
	{
		get => SkinRenderer.GetBool( "b_swim" );
		set => SkinRenderer.Set( "b_swim", value );
	}

	/// <summary> Are we climbing? </summary>
	public bool IsClimbing
	{
		get => SkinRenderer.GetBool( "b_climbing" );
		set => SkinRenderer.Set( "b_climbing", value );
	}

	/// <summary> Are we noclipping? </summary>
	public bool IsNoclipping
	{
		get => SkinRenderer.GetBool( "b_noclip" );
		set => SkinRenderer.Set( "b_noclip", value );
	}

	/// <summary>
	/// Is the weapon lowered? By default, this'll happen when the character hasn't been shooting for a while.
	/// </summary>
	public bool IsWeaponLowered
	{
		get => SkinRenderer.GetBool( "b_weapon_lower" );
		set => SkinRenderer.Set( "b_weapon_lower", value );
	}

	public enum HoldTypes
	{
		None,
		Pistol,
		Rifle,
		Shotgun,
		HoldItem,
		Punch,
		Swing,
		RPG
	}

	/// <summary> What kind of weapon are we holding? </summary>
	public HoldTypes HoldType
	{
		get => (HoldTypes)SkinRenderer.GetInt( "holdtype" );
		set => SkinRenderer.Set( "holdtype", (int)value );
	}

	public enum Hand
	{
		Both,
		Right,
		Left
	}

	/// <summary>
	/// What's the handedness of our weapon? Left handed, right handed, or both hands? This is only supported by some holdtypes, like Pistol, HoldItem.
	/// </summary>
	public Hand Handedness
	{
		get => (Hand)SkinRenderer.GetInt( "holdtype_handedness" );
		set => SkinRenderer.Set( "holdtype_handedness", (int)value );
	}

	/// <summary> Triggers a jump animation. </summary>
	public void TriggerJump()
	{
		SkinRenderer.Set( "b_jump", true );
	}

	/// <summary> Triggers a weapon deploy animation. </summary>
	public void TriggerDeploy()
	{
		SkinRenderer.Set( "b_deploy", true );
	}

	public enum MoveStyles
	{
		Auto,
		Walk,
		Run
	}

	/// <summary> We can force the model to walk or run, or let it decide based on the speed. </summary>
	public MoveStyles MoveStyle
	{
		get => (MoveStyles)SkinRenderer.GetInt( "move_style" );
		set => SkinRenderer.Set( "move_style", (int)value );
	}

	public enum SpecialMoveStyle
	{
		None,
		LedgeGrab,
		Roll,
		Slide
	}

	/// <summary>
	/// We can force the model to have a specific movement state, instead of just running around.
	/// <see cref="SpecialMoveStyle.LedgeGrab"/> is good for shimmying across a ledge.
	/// <see cref="SpecialMoveStyle.Roll"/> is good for a platformer game where the character is rolling around continuously.
	/// <see cref="SpecialMoveStyle.Slide"/> is good for a shooter game or a platformer where the character is sliding.
	/// </summary>
	public SpecialMoveStyle SpecialMove
	{
		get => (SpecialMoveStyle)SkinRenderer.GetInt( "special_movement_states" );
		set => SkinRenderer.Set( "special_movement_states", (int)value );
	}

	public enum SittingStyle
	{
		None,
		Chair,
		Floor
	}

	/// <summary> How are we sitting down? </summary>
	public SittingStyle Sitting
	{
		get => (SittingStyle)SkinRenderer.GetInt( "sit" );
		set => SkinRenderer.Set( "sit", (int)value );
	}

	/// <summary> How far up are we sitting down from the floor? </summary>
	public float SittingOffsetHeight
	{
		get => SkinRenderer.GetFloat( "sit_offset_height" );
		set => SkinRenderer.Set( "sit_offset_height", value );
	}

	/// <summary> From 0-1, how much are we actually sitting down. </summary>
	public float SittingPose
	{
		get => SkinRenderer.GetFloat( "sit_pose" );
		set => SkinRenderer.Set( "sit_pose", value );
	}
}
