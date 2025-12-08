using System;

namespace GameFish;

/// <summary>
/// Looks for properties with <see cref="SettingAttribute"/>.
/// </summary>
[Icon( "display_settings" )]
public partial class Settings : DataFile<Settings, Settings.Data>
{
	protected override NetworkMode NetworkingModeDefault => NetworkMode.Object;

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
	}

	protected override bool InternalTryLoad()
	{
		if ( !base.InternalTryLoad() )
			return false;

		ApplyVolumes();
		return true;
	}
}
