using System;

namespace Playground;

/// <summary>
/// Lets players inspect and change the value of a property in the editor.
/// </summary>
[AttributeUsage( AttributeTargets.Property, AllowMultiple = false )]
sealed class EntityPropertyAttribute : Attribute
{
	public string ID { get; set; }
}
