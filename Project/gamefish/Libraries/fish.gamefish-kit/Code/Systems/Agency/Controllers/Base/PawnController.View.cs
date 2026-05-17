using System.Text.Json.Serialization;

namespace GameFish;

partial class PawnController
{
	/// <summary>
	/// How quickly to transition towards the target position.
	/// </summary>
	[Property]
	[Title( "Speed" )]
	[Range( 0.1f, 5f, clamped: false ), Step( 0.01f )]
	[Feature( VIEW ), Group( EYEPOS ), Order( EYEPOS_ORDER )]
	public virtual float EyeMoveSpeed { get; set; } = 1f;

	/// <summary>
	/// Transition speed resistance.
	/// Helps smooth things out(as the name would imply).
	/// </summary>
	[Property]
	[Title( "Smoothing" )]
	[Range( 0f, 1f, clamped: false ), Step( 0.01f )]
	[Feature( VIEW ), Group( EYEPOS ), Order( EYEPOS_ORDER )]
	public virtual float EyeMoveSmoothing { get; set; } = 0.15f;

	protected Vector3 _eyeVel = Vector3.Zero;

	/// <summary>
	/// If enabled: owner input rotates local eye angles.
	/// </summary>
	[Property]
	[Feature( VIEW ), Order( AIMING_ORDER )]
	[ToggleGroup( value: nameof( AllowAiming ), Label = AIMING )]
	public virtual bool AllowAiming { get; set; } = true;

	/// <summary>
	/// If enabled: limits the local eye rotation's pitch/yaw.
	/// </summary>
	[Property]
	[Title( "Pitch Clamping" )]
	[ToggleGroup( nameof( AllowAiming ) )]
	[Feature( VIEW ), Order( AIMING_ORDER )]
	public virtual bool AimPitchClamping { get; set; } = true;

	/// <summary>
	/// If enabled: the maximum pitch/yaw allowed for local eye rotation.
	/// </summary>
	[Property]
	[Range( 0, 180 )]
	[Title( "Pitch Range" )]
	[ToggleGroup( nameof( AllowAiming ) )]
	[Feature( VIEW ), Order( AIMING_ORDER )]
	[ShowIf( nameof( AimPitchClamping ), true )]
	public virtual FloatRange AimPitchRange { get; set; } = new( -89.9f, 89.9f );

	/// <summary>
	/// How quickly should we try to negate roll on local eye rotation?
	/// </summary>
	[Property]
	[Title( "Anti-Roll Speed" )]
	[Range( 1f, 10f, clamped: false )]
	[ToggleGroup( nameof( AllowAiming ) )]
	[Feature( VIEW ), Order( AIMING_ORDER )]
	public virtual float AimRollResetSpeed { get; set; } = 10f;

	/// <summary>
	/// The local(relative) eye angles.
	/// </summary>
	[Property, JsonIgnore]
	[Title( "Eye Angles" )]
	[ToggleGroup( nameof( AllowAiming ) )]
	[Feature( VIEW ), Order( AIMING_ORDER )]
	protected Angles InspectorLocalEyeAngles
	{
		get => LocalEyeRotation;
		set => LocalEyeRotation = value;
	}

	[Sync( SyncFlags.Interpolate )]
	public Vector3 LocalEyePosition
	{
		get => _localEyePos;
		protected set
		{
			if ( !ITransform.IsValid( in value ) )
				return;

			_localEyePos = value;
			OnSetLocalEyePosition( in value );
		}
	}

	protected Vector3 _localEyePos = default;

	[Sync( SyncFlags.Interpolate )]
	public Rotation LocalEyeRotation
	{
		get => _localEyeRotation;
		protected set
		{
			if ( !ITransform.IsValid( in value ) )
				return;

			_localEyeRotation = value;
			OnSetLocalEyeRotation( in value );
		}
	}

	protected Rotation _localEyeRotation = Rotation.Identity;

	public Vector3 EyePosition
	{
		get => Pawn?.EyePosition ?? WorldTransform.PointToWorld( LocalEyePosition );
		set { if ( Pawn.IsValid() ) Pawn.EyePosition = value; }
	}

	public Rotation EyeRotation
	{
		get => Pawn?.EyeRotation ?? WorldTransform.RotationToWorld( LocalEyeRotation );
		set { if ( Pawn.IsValid() ) Pawn.EyeRotation = value; }
	}

	public Transform EyeTransform => Pawn?.EyeTransform ?? new( EyePosition, EyeRotation, WorldScale );

	public Vector3 EyeForward => EyeRotation.Forward;

	public virtual void SetLocalEyePosition( Vector3 pos )
		=> LocalEyePosition = pos;

	protected virtual void OnSetLocalEyePosition( in Vector3 pos ) { }

	public virtual void SetLocalEyeRotation( Rotation value )
		=> LocalEyeRotation = value;

	protected virtual void OnSetLocalEyeRotation( in Rotation r ) { }

	/// <summary>
	/// The vertical eye offset.
	/// </summary>
	public virtual float EyeHeight
	{
		get => LocalEyePosition.z;
		set => SetLocalEyePosition( LocalEyePosition.WithZ( value ) );
	}

	/// <returns> The position the eye wants to be. </returns>
	public virtual Vector3 GetLocalEyeTargetPosition()
		=> Vector3.Zero;

	/// <summary>
	/// Initializes the view.
	/// <br /> <br />
	/// <b> NOTE: </b> A good place to reset/snap transitions.
	/// </summary>
	protected virtual void SetupView()
	{
		_localEyePos = GetLocalEyeTargetPosition();
	}

	/// <summary>
	/// Performs automatic eye position/rotation logic for crouching, wall running, stuff like that.
	/// </summary>
	protected virtual void SimulateView( in float deltaTime )
	{
		UpdateEyePosition( in deltaTime );
		UpdateEyeRotation( in deltaTime );
	}

	/// <summary>
	/// Performs automatic eye positioning such as when crouching.
	/// </summary>
	protected virtual void UpdateEyePosition( in float deltaTime )
	{
		var eyePos = LocalEyePosition;
		var eyeTargetPos = GetLocalEyeTargetPosition();

		LocalEyePosition = Vector3.SmoothDamp( eyePos, eyeTargetPos,
			ref _eyeVel, EyeMoveSmoothing, EyeMoveSpeed * deltaTime );
	}

	/// <summary>
	/// Performs automatic eye rotation such as resetting roll over time.
	/// </summary>
	protected virtual void UpdateEyeRotation( in float deltaTime )
		=> ResetEyeRoll( AimRollResetSpeed, in deltaTime );

	/// <summary>
	/// Negates eye roll over time.
	/// </summary>
	protected virtual void ResetEyeRoll( in float speed, in float deltaTime )
	{
		var objUp = (Pawn?.WorldRotation ?? WorldRotation).Up;

		ResetEyeRotation( EyeForward, in objUp, in speed, in deltaTime );
	}

	/// <summary>
	/// Angles our current eye rotation towards the specified up/forward over time.
	/// </summary>
	protected virtual void ResetEyeRotation( in Vector3 toForward, in Vector3 toUp, in float speed, in float deltaTime )
	{
		var rEye = EyeRotation;

		var vUp = rEye.Up.SlerpTo( in toUp, speed * deltaTime );
		var vForward = rEye.Forward.SlerpTo( in toForward, speed * deltaTime );

		EyeRotation = Rotation.LookAt( vForward, vUp );
	}

	/// <summary>
	/// Attempts to add <paramref name="rLook"/> to our local aim rotation.
	/// </summary>
	/// <returns> If aiming was allowed. </returns>
	public virtual bool TryAim( in Rotation rLook, in float deltaTime )
	{
		Angles angLook = rLook;

		if ( AimPitchClamping )
		{
			Angles angAim = LocalEyeRotation;

			angAim.pitch = (angAim.pitch + angLook.pitch).Clamp( AimPitchRange );
			angAim.yaw += angLook.yaw;

			LocalEyeRotation = angAim;
		}
		else
		{
			var rAim = LocalEyeRotation;
			var rInverse = rAim.Inverse;

			rAim *= Rotation.FromAxis( rInverse.Up, angLook.yaw );
			rAim *= Rotation.FromPitch( angLook.pitch );

			LocalEyeRotation = rAim;
		}

		return true;
	}
}
