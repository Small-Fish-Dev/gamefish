using Sandbox.Audio;

namespace GameFish;

partial class Entity
{
	/// <summary>
	/// Attaches an existing sound handle to this object.
	/// </summary>
	public SoundHandle AttachSound( SoundHandle soundHandle, in Transform tLocal )
	{
		if ( !soundHandle.IsValid() || !GameObject.IsValid() )
			return null;

		soundHandle.Parent = GameObject;
		soundHandle.FollowParent = true;

		if ( tLocal.IsValid )
			soundHandle.LocalTransform = tLocal;

		return soundHandle;
	}

	/// <summary>
	/// Attaches an existing sound handle to this object using settings.
	/// </summary>
	public SoundHandle AttachSound( SoundHandle soundHandle, in SoundSettings s )
	{
		if ( !soundHandle.IsValid() || !GameObject.IsValid() )
			return null;

		soundHandle.Parent = GameObject;
		soundHandle.FollowParent = true;

		ApplySoundSettings( soundHandle, in s );

		return soundHandle;
	}

	private static void ApplySoundSettings( SoundHandle soundHandle, in SoundSettings s )
	{
		if ( !soundHandle.IsValid() )
			return;

		// Might be overriding the target.
		if ( s.Following.IsValid() )
			soundHandle.Parent = s.Following;

		// Could be a world or local transform.
		var t = s.Transform;

		if ( t.IsValid )
		{
			if ( s.InWorld )
				soundHandle.Transform = t;
			else
				soundHandle.LocalTransform = t;
		}

		// Apply volume/mixer options.
		if ( s.Volume is float sndVol )
			soundHandle.Volume = sndVol;

		if ( s.Pitch is float sndPitch )
			soundHandle.Pitch = sndPitch;

		var sndMixer = s.Mixer;

		if ( !sndMixer.IsBlank() )
		{
			soundHandle.TargetMixer = Mixer.FindMixerByName( sndMixer )
				?? soundHandle.TargetMixer
				?? Mixer.Default;
		}
	}


	/// <summary>
	/// Plays a sound event locally that follows this object.
	/// </summary>
	public SoundHandle EmitSound( SoundEvent soundEvent, in Vector3 localPos = default )
		=> EmitSound( soundEvent, new Transform( localPos ) );

	/// <summary>
	/// Plays a sound event locally that follows this object.
	/// </summary>
	public SoundHandle EmitSound( SoundEvent soundEvent, in Transform tLocal )
	{
		if ( !soundEvent.IsValid() )
			return null;

		return AttachSound( Sound.Play( soundEvent ), tLocal );
	}

	/// <summary>
	/// Plays a sound event locally using settings.
	/// </summary>
	public SoundHandle EmitSound( SoundEvent soundEvent, in SoundSettings s )
	{
		if ( !soundEvent.IsValid() )
			return null;

		return AttachSound( Sound.Play( soundEvent ), in s );
	}


	/// <summary>
	/// Plays a sound file locally that follows this object.
	/// </summary>
	public SoundHandle EmitSound( SoundFile soundFile, in Vector3 localPos = default )
		=> EmitSound( soundFile, new Transform( localPos ) );

	/// <summary>
	/// Plays a sound file locally that follows this object.
	/// </summary>
	public SoundHandle EmitSound( SoundFile soundFile, in Transform tLocal )
	{
		if ( !soundFile.IsValid() )
			return null;

		return AttachSound( Sound.PlayFile( soundFile ), tLocal );
	}

	/// <summary>
	/// Plays a sound file locally using settings.
	/// </summary>
	public SoundHandle EmitSound( SoundFile soundFile, in SoundSettings s )
	{
		if ( !soundFile.IsValid() )
			return null;

		return AttachSound( Sound.PlayFile( soundFile ), in s );
	}


	/// <summary>
	/// Allows the object's owner to broadcast a sound event on this object.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void BroadcastSound( SoundEvent soundEvent, Vector3 localPos = default )
		=> EmitSound( soundEvent, in localPos );

	/// <summary>
	/// Allows the object's owner to broadcast a sound event on this object using settings.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void BroadcastSound( SoundEvent soundEvent, SoundSettings settings )
		=> EmitSound( soundEvent, in settings );


	/// <summary>
	/// Allows the host to broadcast a sound event on this object.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.HostOnly )]
	public void HostBroadcastSound( SoundEvent soundEvent, Vector3 localPos = default )
		=> EmitSound( soundEvent, in localPos );

	/// <summary>
	/// Allows the host to broadcast a sound event on this object using settings.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.HostOnly )]
	public void HostBroadcastSound( SoundEvent soundEvent, SoundSettings settings )
		=> EmitSound( soundEvent, in settings );


	/// <summary>
	/// Allows the object's owner to broadcast a sound file on this object using settings.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.OwnerOnly )]
	public void BroadcastSound( SoundFile soundFile, SoundSettings settings )
		=> EmitSound( soundFile, in settings );

	/// <summary>
	/// Allows the host to broadcast a sound file on this object using settings.
	/// </summary>
	[Rpc.Broadcast( NetFlags.Reliable | NetFlags.HostOnly )]
	public void HostBroadcastSound( SoundFile soundFile, SoundSettings settings )
		=> EmitSound( soundFile, in settings );
}
