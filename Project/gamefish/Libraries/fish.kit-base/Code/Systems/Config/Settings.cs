using System;
using Sandbox.Audio;

namespace GameFish;

/*
	UI

	Drop Down
	Slider
	Button
	- Disabled
	- OnClick
	Options Menu
*/

public partial class Settings : DataFile<Settings, Settings.Data>
{
	public override string FileName { get; } = "settings.cfg";

	protected struct PropertySetting : IValid
	{
		public readonly bool IsValid => !string.IsNullOrWhiteSpace( Name );

		public string Name { get; set; }
		public Type ValueType { get; set; }
		public Type ParentType { get; set; }

		public PropertySetting() { }

		public PropertySetting( PropertyDescription p )
		{
			if ( p is null )
				return;

			Name = p.Name;
			ValueType = p.PropertyType;
			ParentType = p.TypeDescription.TargetType;
		}
	}

	public partial class Data : DataClass
	{
		public Dictionary<string, float> Volumes { get; set; } = GetMixerVolumes();

		/// <returns> A table of existing mixers and their current volumes. </returns>
		public static Dictionary<string, float> GetMixerVolumes()
		{
			var mixerVolumes = new Dictionary<string, float>();

			void SetVolume( string name, float volume )
			{
				if ( !string.IsNullOrEmpty( name ) )
					mixerVolumes[name] = volume;
			}

			if ( Mixer.Master is not null )
				SetVolume( Mixer.Master.Name ?? "Master", Mixer.Master.Volume );

			foreach ( var mixer in Mixer.Master.GetChildren() )
				SetVolume( mixer.Name, mixer.Volume );

			return mixerVolumes;
		}
	}
}
