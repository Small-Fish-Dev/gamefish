using System;
using System.Text.Json.Serialization;
using GameFish.Razor;
using Sandbox.VR;

namespace GameFish;

partial class PawnView
{
	/// <summary>
	/// If enabled: the view can rumble and be pushed around.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	[ToggleGroup( nameof( ShakingEnabled ), Label = SHAKING )]
	public virtual bool ShakingEnabled { get; set; } = true;

	/// <summary>
	/// Returns the offset back to the center faster depending on distance.
	/// <br /> <br />
	/// <b> NOTE: </b> Very useful for reducing the sudden
	/// "snap" when velocity bounces off of the radius.
	/// You could think of it as a more customizable exponential decay.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	[Title( "Softening" )]
	[ToggleGroup( nameof( ShakingEnabled ) )]
	public virtual Curve ShakeSoftening { get; set; } = new( new( 0f, 0f ), new( 1f, 1f ) )
	{
		TimeRange = new( 0f, 1f ),
		ValueRange = new( 0f, 2f ),
	};

	/// <summary>
	/// Multiplies the intensity depending on when a shake was last applied.
	/// <br /> <br />
	/// <b> NOTE: </b> You can tweak this so that shakes
	/// fade out quickly yet linger a while, stuff like that.
	/// </summary>
	[Property]
	[Title( "Intensity" )]
	[Feature( VIEW ), Group( SHAKE )]
	[Range( 1f, 50f, clamped: false )]
	[ToggleGroup( nameof( ShakingEnabled ) )]
	public virtual Curve ShakeIntensity { get; set; } = new( new( 0f, 1f ), new( 1f, 0f ) )
	{
		TimeRange = new( 0f, 1f ),
		ValueRange = new( 0f, 1f ),
	};

	/// <summary>
	/// Multiplier the intensity of all newly added shakes.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	[Title( "Scale" )]
	[Range( 0f, 2f, clamped: false )]
	[ToggleGroup( nameof( ShakingEnabled ) )]
	public virtual float ShakeIntensityScale { get; set; } = 1f;

	/// <summary>
	/// The maximum distance away from the center that shaking can bring us.
	/// </summary>
	[Property]
	[Title( "Radius" )]
	[Feature( VIEW ), Group( SHAKE )]
	[Range( 4f, 16f, clamped: false )]
	[ToggleGroup( nameof( ShakingEnabled ) )]
	public virtual float ShakeRadiusLimit { get; set; } = 7f;

	/// <summary>
	/// A single shake lasts at most this time.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	[Title( "Duration" )]
	[Range( 0f, 5f, clamped: false )]
	[ToggleGroup( nameof( ShakingEnabled ) )]
	public virtual float ShakeDurationLimit { get; set; } = 2f;

	/// <summary>
	/// The shake velocity can't be greater than this.
	/// </summary>
	[Property]
	[Title( "Speed Limit" )]
	[Feature( VIEW ), Group( SHAKE )]
	[Range( 0f, 1000f, clamped: false )]
	[ToggleGroup( nameof( ShakingEnabled ) )]
	public virtual float ShakeSpeedLimit { get; set; } = 250f;

	/// <summary>
	/// Continuously slows down the shake speed.
	/// </summary>
	[Property]
	[Title( "Smoothing" )]
	[Feature( VIEW ), Group( SHAKE )]
	[Range( 0f, 2f, clamped: false )]
	[ToggleGroup( nameof( ShakingEnabled ) )]
	public virtual float ShakeSmoothing { get; set; } = 0.5f;

	/// <summary>
	/// The randomness in angle from rebounding off the radius.
	/// </summary>
	[Property]
	[Feature( VIEW )]
	[Title( "Randomness" )]
	[ToggleGroup( nameof( ShakingEnabled ) )]
	public virtual Fraction ShakeBounceRandomness { get; set; } = 1.0f;

	/// <summary>
	/// The current relative orientation. <br />
	/// Setting this automatically sets the transform.
	/// </summary>
	[Property]
	[JsonIgnore]
	[Feature( VIEW )]
	[Title( "Offset (current)" )]
	[ShowIf( nameof( InGame ), true )]
	[ToggleGroup( nameof( ShakingEnabled ) )]
	protected Vector3 InspectorShakeOffset
	{
		get => ShakeOffset;
		set => ShakeOffset = value;
	}

	/// <summary>
	/// The current relative orientation. <br />
	/// Setting this automatically sets the transform.
	/// </summary>
	[Property]
	[JsonIgnore]
	[Feature( VIEW )]
	[Title( "Velocity (current)" )]
	[ShowIf( nameof( InGame ), true )]
	[ToggleGroup( nameof( ShakingEnabled ) )]
	protected Vector3 InspectorShakeVelocity
	{
		get => ShakeVelocity;
		set
		{
			ShakeVelocity = default;
			AddShake( in value );
		}
	}

	/// <summary>
	/// The positional offset.
	/// </summary>
	[Sync( SyncFlags.Interpolate )]
	protected Vector3 ShakeOffset
	{
		get => _shakeOffset;
		set
		{
			if ( _shakeOffset == value )
				return;

			if ( !ITransform.IsValid( value ) )
				return;

			_shakeOffset = value.ClampLength( ShakeRadiusLimit );
			OnSetShakeOffset( in _shakeOffset );
		}
	}

	protected Vector3 _shakeOffset = Vector3.Zero;

	/// <summary>
	/// The speed the shake is still going.
	/// </summary>
	[Sync]
	protected Vector3 ShakeVelocity
	{
		get => _shakeVel;
		set
		{
			if ( _shakeVel == value )
				return;

			if ( !ITransform.IsValid( value ) )
				return;

			_shakeVel = value.ClampLength( ShakeSpeedLimit );
			OnSetShakeVelocity( _shakeVel );
		}
	}

	protected Vector3 _shakeVel = Vector3.Zero;

	[Sync( SyncFlags.Interpolate )]
	public TimeUntil UntilShakeEnds { get; protected set; }

	public virtual bool IsShaking => ShakeTimeRemaining > 0f;
	public virtual float ShakeTimeRemaining => UntilShakeEnds.Relative.Max( 0f );
	public virtual float ShakeFraction => ShakeIntensity.Evaluate( UntilShakeEnds.Fraction.Clamp( 0f, 1f ) );

	[Feature( VIEW )]
	[Button( "Test Shake" )]
	[ShowIf( nameof( InGame ), true )]
	[ToggleGroup( nameof( ShakingEnabled ) )]
	protected virtual void DebugTestShake()
	{
		AddShake( Vector3.Random.Normal * ShakeSpeedLimit );
	}

	protected virtual void OnSetShakeOffset( in Vector3 offset )
	{
		// this.Log( $"Shake Offset: {offset}" );
	}

	protected virtual void OnSetShakeVelocity( in Vector3 vel )
	{
		// this.Log( $"Shake Velocity: {vel}" );
	}

	protected virtual void UpdateShake( in float deltaTime )
	{
		UpdateShakeVelocity( in deltaTime );
		UpdateShakeOffset( in deltaTime );
	}

	protected virtual void UpdateShakeVelocity( in float deltaTime )
	{
		if ( !IsShaking )
		{
			ShakeVelocity = default;
			return;
		}

		var vel = ShakeVelocity;

		// Apply some friction. Might help with nausea
		vel -= vel * (ShakeSmoothing * deltaTime).Clamp( 0f, 1f );

		// Keep it from ever moving too quickly.
		if ( vel.AlmostEqual( default ) )
			vel = default;

		ShakeVelocity = vel;
	}

	protected virtual void UpdateShakeOffset( in float deltaTime )
	{
		var s = ShakeOffset;

		// Apply velocity.
		if ( IsShaking )
			s += ShakeVelocity * ShakeFraction * deltaTime;

		var maxRadius = ShakeRadiusLimit * ShakeFraction;

		// Return to center even without velocity.
		var sDir = s.Normal;
		var dist = s.Length;

		var frac = dist.Remap( 0f, maxRadius );
		var decay = dist * ShakeSoftening.Evaluate( frac );
		dist -= (decay * deltaTime).Clamp( 0f, dist );

		s = sDir * dist;

		// No edging allowed.
		if ( s.AlmostEqual( default ) )
			s = default;

		ShakeOffset = s;

		// Bounce off the radius once reaching it.
		if ( dist >= maxRadius )
		{
			var vel = ShakeVelocity;
			var velDir = vel.Normal;

			if ( ShakeBounceRandomness != 0f )
			{
				var rPerturb = GetShakeBouncePerturbance();
				velDir = velDir.RotateAround( default, rPerturb );
			}

			// Make sure randomness is actually bouncing off.
			if ( velDir.Dot( sDir ) > 0 )
				velDir = velDir.Reflect( sDir );

			ShakeVelocity = velDir * vel.Length;
		}
	}

	/// <returns> The randomness in from bouncing off the radius. </returns>
	protected virtual Rotation GetShakeBouncePerturbance()
	{
		var v = Vector3.Random.Normal * ShakeBounceRandomness;

		return Rotation.LookAt( v.Normal );
	}

	/// <summary>
	/// Adds shake velocity directly without respect to world origin.
	/// </summary>
	protected virtual void AddShakeVelocity( in Vector3 addVel, in float? time = null )
	{
		if ( !ShakingEnabled )
			return;

		if ( addVel == default )
			return;

		var vel = ShakeVelocity;

		if ( !ITransform.IsValid( vel ) )
			vel = default;

		// Apply global added velocity multiplier.
		vel += addVel * ShakeIntensityScale;
		vel = vel.ClampLength( ShakeSpeedLimit );

		ShakeVelocity = vel;

		// Auto-timer with manual override.
		var fTime = time ?? GetShakeDuration( in vel );
		fTime = MathF.Max( UntilShakeEnds, fTime );

		UntilShakeEnds = fTime.Min( ShakeDurationLimit );
	}

	/// <returns> The duration calculated from velocity. </returns>
	protected virtual float GetShakeDuration( in Vector3 vel )
	{
		if ( !ITransform.IsValid( vel ) )
			return 0f;

		return vel.Length.Remap( 0f, ShakeSpeedLimit, 0f, ShakeDurationLimit );
	}

	/// <summary>
	/// Adds shake velocity directly without respect to world origin.
	/// </summary>
	public virtual void AddShake( in Vector3 vel, in float? time = null )
		=> AddShakeVelocity( vel, time );

	/// <summary>
	/// Adds shake from somewhere in the world.
	/// </summary>
	/// <param name="origin"> The shake will push your view from here. </param>
	/// <param name="intensity"> The maximum strength of the shake. </param>
	/// <param name="radius"> The distance from the origin where intensity is at minimum. </param>
	/// <param name="min"> Always has at least this much intensity. </param>
	/// <param name="time"> The manual duration of the shake(or automatic). </param>
	public virtual void AddShakeFrom( in Vector3 origin, float intensity, in float? radius = null, in float? min = null, in float? time = null )
	{
		if ( !ShakingEnabled )
			return;

		var eyePos = PawnEyePosition;

		var dir = origin.Direction( eyePos );
		var dist = origin.Distance( eyePos );

		// Higher intensity the closer you are to the origin
		var fMin = min ?? 0f;
		var fRadius = radius ?? 2048;

		intensity = intensity.Max( fMin );
		intensity = dist.Remap( 0f, fRadius, intensity, fMin );

		AddShakeVelocity( dir * intensity, time );
	}

	public virtual void AddShake( in ShakeData shake )
	{
		if ( !ShakingEnabled )
			return;

		AddShakeFrom( shake.Origin, shake.Intensity, shake.Minimum, shake.Radius, shake.Duration );
	}

	public virtual void ResetShake()
		=> ShakeVelocity = default;

	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostResetShake()
		=> ResetShake();

	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostAddShake( ShakeData shake )
		=> AddShake( shake );

	[Rpc.Owner( NetFlags.Reliable | NetFlags.HostOnly )]
	public void RpcHostAddShake( Vector3 origin, float intensity, float? radius = null, float? min = null, float? time = null )
		=> AddShakeFrom( origin, intensity, radius, min, time );
}
