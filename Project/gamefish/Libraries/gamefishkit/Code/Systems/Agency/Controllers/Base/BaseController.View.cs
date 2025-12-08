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
	/// The relative eye angles.
	/// </summary>
	[Title( "Eye Angles" )]
	[Property, JsonIgnore]
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
	protected Vector3 LocalEyePosition
	{
		get => _localEyePos;
		set
		{
			if ( !ITransform.IsValid( in value ) )
				return;

			_localEyePos = value;
			OnSetLocalEyePosition( in value );
		}
	}

	protected Vector3 _localEyePos;

	[Sync( SyncFlags.Interpolate )]
	protected Rotation LocalEyeRotation
	{
		get => _localEyeRotation;
		set
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


	public virtual Vector3 GetLocalEyePosition()
		=> LocalEyePosition;

	public virtual void SetLocalEyePosition( Vector3 pos )
		=> LocalEyePosition = pos;

	protected virtual void OnSetLocalEyePosition( in Vector3 pos ) { }


	public virtual Rotation GetLocalEyeRotation()
		=> LocalEyeRotation;

	public virtual void SetLocalEyeRotation( Rotation value )
		=> LocalEyeRotation = value;

	protected virtual void OnSetLocalEyeRotation( in Rotation r ) { }


	/// <summary>
	/// The vertical eye offset.
	/// </summary>
	public virtual float EyeHeight
	{
		get => GetLocalEyePosition().z;
		set => SetLocalEyePosition( LocalEyePosition.WithZ( value ) );
	}

	/// <returns> The position the eye wants to be. </returns>
	public virtual Vector3 GetLocalEyeTargetPosition()
		=> Vector3.Zero;

	/// <summary>
	/// Attemps to adds <paramref name="rLook"/> to our local aim rotation.
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

			angAim.roll = angAim.roll.LerpDegreesTo( 0f, deltaTime * 10f );

			LocalEyeAngles = angAim;
		}
		else
		{
			var rAim = LocalEyeRotation;
			var rInverse = rAim.Inverse;

			rAim *= Rotation.FromAxis( rInverse.Up, angLook.yaw );
			rAim *= Rotation.FromPitch( angLook.pitch );

			rAim *= Rotation.FromRoll( -rAim.Roll() * deltaTime * 10f );

			LocalEyeRotation = rAim;
		}

		return true;
	}
}
