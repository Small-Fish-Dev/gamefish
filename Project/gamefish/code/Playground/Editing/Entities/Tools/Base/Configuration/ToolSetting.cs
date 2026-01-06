using System;

namespace Fishbox;

/// <summary>
/// Tells the UI to show a property as a tool option.
/// </summary>
[AttributeUsage( AttributeTargets.Property )]
public partial class ToolSettingAttribute : Attribute;
