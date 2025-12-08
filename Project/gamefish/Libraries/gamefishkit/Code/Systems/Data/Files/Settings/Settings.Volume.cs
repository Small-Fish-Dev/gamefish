using Sandbox.Audio;

namespace GameFish;

partial class Settings
{
	public partial class Data : DataClass
	{
		public Dictionary<string, float> Volumes { get; set; } = GetCurrentMixerVolumes();
	}

	/// <summary>
	/// Applies the settings file's configuration
	/// </summary>
	/// <param name="volumes"></param>
	public virtual void ApplyVolumes( Dictionary<string, float> volumes = null )
	{
		volumes ??= GetVolumes();

		foreach ( var (mixerName, volume) in volumes )
			ApplyMixerVolume( mixerName, volume );
	}

	/// <summary>
	/// Finds the mixer by its name and sets its volume.
	/// Does not change or save any kind of configuration!
	/// </summary>
	protected virtual void ApplyMixerVolume( string mixerName, float volume )
	{
		if ( string.IsNullOrWhiteSpace( mixerName ) )
			return;

		var mixer = Mixer.FindMixerByName( mixerName );

		if ( mixer is null )
		{
			DebugLog( $"Couldn't find Mixer from name:[{mixerName}]" );
			return;
		}

		mixer.Volume = volume.Clamp( 0f, 1f );
	}

	/// <summary>
	/// Reads the current volumes from the settings file.
	/// </summary>
	public virtual Dictionary<string, float> GetVolumes()
	{
		var volumes = GetCurrentMixerVolumes() ?? [];

		if ( !Get<Dictionary<string, float>>( nameof( Data.Volumes ), out var volSaved ) )
			return volumes;

		if ( volSaved.Count == 0 )
			return volumes;

		foreach ( var (name, vol) in volSaved )
		{
			if ( string.IsNullOrWhiteSpace( name ) )
				continue;

			if ( volumes.ContainsKey( name ) )
				volumes[name] = vol;
		}

		return volumes;
	}

	public virtual void SetVolume( string mixerName, float volume )
	{
		if ( string.IsNullOrWhiteSpace( mixerName ) )
			return;

		var volumes = GetVolumes() ?? GetCurrentMixerVolumes();

		if ( !volumes.ContainsKey( mixerName ) )
			return;

		volumes[mixerName] = volume;

		ApplyVolumes( volumes );

		Set( nameof( Data.Volumes ), volumes );
	}

	/// <returns> A table of existing mixers and their current volumes. </returns>
	public static Dictionary<string, float> GetCurrentMixerVolumes()
	{
		var mixerVolumes = new Dictionary<string, float>();

		void SetVolume( string name, float volume )
		{
			if ( !string.IsNullOrEmpty( name ) )
				mixerVolumes[name] = volume;
		}

		if ( Mixer.Master is not null )
		{
			SetVolume( Mixer.Master.Name ?? "Master", Mixer.Master.Volume );

			foreach ( var mixer in Mixer.Master.GetChildren() )
				SetVolume( mixer.Name, mixer.Volume );
		}

		return mixerVolumes;
	}
}
