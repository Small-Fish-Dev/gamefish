using System;

namespace GameFish;

[AttributeUsage( AttributeTargets.Property, AllowMultiple = true, Inherited = true )]
public class SettingAttribute : Attribute
{
	/// <summary> The identifying string you use to set/get the setting. </summary>
	public string ID { get; protected set; }

	/// <summary> The display name(for menus etc.). </summary>
	public string Name { get; protected set; }

	/// <summary> The default value of the property. </summary>
	public object Default { get; protected set; }

	/// <summary> Allows specifying the value type. </summary>
	public Type Type { get; protected set; }

	public bool Slider { get; protected set; }

	public float Min { get; protected set; }
	public float Max { get; protected set; }

	public SettingAttribute() { }

	public SettingAttribute( string id )
	{
		ID = id;
	}

	public SettingAttribute( string id, string name )
	{
		ID = id;
		Name = name;
	}

	public SettingAttribute( string id, string name, object @default )
	{
		ID = id;
		Name = name;
		Default = @default;
	}
}
