using System.Text.Json.Serialization;

namespace GameFish;

partial class BaseController
{
	/// <summary>
	/// How quickly to transition towards the target position.
	/// </summary>
	[Property]
	[Title( "Speed" )]
	[Range( 0.1f, 5f, clamped: false ), Step( 0.01f )]
	[Feature( VIEW ), Group( EYE_POS ), Order( EYEPOS_ORDER )]
	public virtual float EyeMoveSpeed { get; set; } = 1f;

	/// <summary>
	/// Transition speed resistance.
	/// Helps smooth things out(as the name would imply).
	/// </summary>
	[Property]
	[Title( "Smoothing" )]
	[Range( 0f, 1f, clamped: false ), Step( 0.01f )]
	[Feature( VIEW ), Group( EYE_POS ), Order( EYEPOS_ORDER )]
	public virtual float EyeMoveSmoothing { get; set; } = 0.15f;

	protected Vector3 _eyeVel = Vector3.Zero;

	/// <summary>
	/// Should the owner's look input rotate their eye angles?
	/// </summary>
	[Property]
	[Feature( VIEW ), Order( AIMING_ORDER )]
	[ToggleGroup( value: nameof( AllowAiming ), Label = "Aiming" )]
	public virtual bool AllowAiming { get; set; } = true;

	[Property]
	[ToggleGroup( nameof( AllowAiming ) )]
	[Feature( VIEW ), Order( AIMING_ORDER )]
	public virtual bool PitchClamping { get; set; } = true;

	[Property]
	[Range( 0, 180 )]
	[ToggleGroup( nameof( AllowAiming ) )]
	[Feature( VIEW ), Order( AIMING_ORDER )]
	[ShowIf( nameof( PitchClamping ), true )]
	public virtual FloatRange PitchRange { get; set; } = new( -89.9f, 89.9f );

	/// <summary>
	/// The local(relative) eye angles.
	/// </summary>
	[Property, JsonIgnore]
	[Title( "Eye Angles" )]
	[ToggleGroup( nameof( AllowAiming ) )]
	[Feature( VIEW ), Order( AIMING_ORDER )]
	protected virtual Angles InspectorLocalEyeAngles
	{
		get => LocalEyeAngles;
		set => LocalEyeAngles = value;
	}

	protected virtual Angles LocalEyeAngles
	{
		get => LocalEyeRotation;
		set => LocalEyeRotation = PitchClamping
			? value.WithPitch( value.pitch.Clamp( PitchRange ) )
			: value;
	}

	protected Rotation _viewRotation = Rotation.Identity;

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

	protected Vector3 _localEyePos;

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

	public Vector3 EyeForward => Pawn?.EyeForward
		?? WorldTransform.RotationToWorld( LocalEyeRotation ).Forward.Normal;


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
	{
		var fRoll = LocalEyeRotation.Roll();
		fRoll = (fRoll * deltaTime * -10f).Clamp( -fRoll, fRoll );

		LocalEyeRotation *= Rotation.FromRoll( fRoll );
	}

	/// <summary>
	/// Attempts to add <paramref name="rLook"/> to our local aim rotation.
	/// </summary>
	/// <returns> If aiming was allowed. </returns>
	public virtual bool TryAim( in Rotation rLook, in float deltaTime )
	{
		Angles angLook = rLook;

		if ( PitchClamping )
		{
			Angles angAim = LocalEyeAngles;

			angAim.pitch = (angAim.pitch + angLook.pitch).Clamp( PitchRange );
			angAim.yaw += angLook.yaw;

			LocalEyeAngles = angAim;
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
