using System;

namespace GameFish;

[AttributeUsage( AttributeTargets.Property, AllowMultiple = true, Inherited = true )]
public class SettingAttribute : Attribute
{
	/// <summary>
	/// The identifying string you use to set/get the setting.
	/// </summary>
	public string ID { get; protected set; }

	/// <summary>
	/// The display name (for menus and such).
	/// </summary>
	public string Name { get; protected set; }

	/// <summary>
	/// What this setting changes.
	/// </summary>
	public string Description { get; protected set; }

	/// <summary>
	/// The category this setting belongs to. Multiple settings that share the same category may
	/// be categorized together in UIs, for example.
	/// </summary>
	public string Category { get; set; }

	/// <summary>
	/// The default value of the property.
	/// </summary>
	public object Default { get; protected set; }

	/// <summary>
	/// Allows specifying the value type.
	/// </summary>
	public Type Type { get; set; }

	/// <summary>
	/// Indicates the setting should have a slider control.
	/// </summary>
	public bool Slider { get; set; }

	/// <summary>
	/// The minimum value the number can be.
	/// </summary>
	public float Min { get; set; }

	/// <summary>
	/// The maximum value the number can be.
	/// </summary>
	public float Max { get; set; }

	/// <summary>
	/// Sliders should move the value between notches that are this far apart.
	/// </summary>
	public float Step { get; set; }

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

	public SettingAttribute( string id, string name, string description )
	{
		ID = id;
		Name = name;
		Description = description;
	}

	public SettingAttribute( string id, string name, object @default )
	{
		ID = id;
		Name = name;
		Default = @default;
	}

	public SettingAttribute( string id, string name, string description, object @default )
	{
		ID = id;
		Name = name;
		Description = description;
		Default = @default;
	}
}
